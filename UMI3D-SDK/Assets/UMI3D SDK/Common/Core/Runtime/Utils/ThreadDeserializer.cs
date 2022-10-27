/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Helper to read a DTO in a separated thread.
    /// </summary>
    public class ThreadDeserializer
    {
        private Thread thread;
        private readonly int sleepTimeMiliseconde = 50;
        private readonly Queue<(byte[], Action<UMI3DDto>, Newtonsoft.Json.TypeNameHandling)> bsonQueue;
        private readonly object runningLock = new object();
        private bool running;

        private bool Running
        {
            get
            {
                lock (runningLock)
                    return this.running;
            }
            set
            {
                lock (runningLock)
                    this.running = value;
            }
        }

        public ThreadDeserializer()
        {
            bsonQueue = new Queue<(byte[], Action<UMI3DDto>, Newtonsoft.Json.TypeNameHandling)>();
            Running = true;
            thread = new Thread(ThreadUpdate);
            if (!thread.IsAlive)
                thread.Start();
        }

        public void Stop()
        {
            Running = false;
        }

        public void FromBson(byte[] bson, Action<UMI3DDto> action, Newtonsoft.Json.TypeNameHandling type = Newtonsoft.Json.TypeNameHandling.All)
        {
            if (action != null)
            {
                if (bson != null)
                    lock (bsonQueue)
                        bsonQueue.Enqueue((bson, action, type));
                else
                    action.Invoke(null);
            }
        }

        public async Task<UMI3DDto> FromBson(byte[] bson, Newtonsoft.Json.TypeNameHandling type = Newtonsoft.Json.TypeNameHandling.All)
        {
            if (bson != null)
            {
                UMI3DDto result = null;
                bool completed = false;
                Action<UMI3DDto> action = (v) =>
                {
                    result = v;
                    completed = true;
                };
                FromBson(bson, action, type);
                while (!completed)
                {
                    await Task.Yield();
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        throw new Exception("Application is not playing");
                    }
#endif
                }
                return result;
            }
            return null;
        }

        private void ThreadUpdate()
        {
            while (Running)
            {
                try
                {
                    (byte[], Action<UMI3DDto>, Newtonsoft.Json.TypeNameHandling) c = default;
                    bool set = false;
                    lock (bsonQueue)
                        if (bsonQueue.Count > 0)
                        {
                            c = bsonQueue.Dequeue();
                            set = true;
                        }
                    if (set)
                    {
                        var dto = UMI3DDto.FromBson(c.Item1, c.Item3);
                        MainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() => c.Item2.Invoke(dto));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"error {e}");
                }
                Thread.Sleep(sleepTimeMiliseconde);
            }
            thread = null;
        }
    }
}