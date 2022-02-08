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
using System.IO;
using System.Threading;
using UnityEngine;

namespace umi3d.common
{
    public class ThreadWritter
    {
        private readonly string path;
        private Thread thread;
        private readonly int sleepTimeMiliseconde = 500;
        private readonly Queue<string> queue;
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

        public ThreadWritter(string path)
        {
            queue = new Queue<string>();
            this.path = path;
            if (string.IsNullOrEmpty(this.path))
                return;
            Running = true;

            thread = new Thread(ThreadUpdate);
            if (!thread.IsAlive)
                thread.Start();
        }

        public void Stop()
        {
            Running = false;
        }

        public void Write(string data)
        {
            lock (queue)
            {
                queue.Enqueue(data);
            }
        }

        private void ThreadUpdate()
        {
            while (Running)
            {
                try
                {
                    if (!File.Exists(path))
                        File.Create(path);
                    lock (queue)
                        if (queue.Count > 0)
                        {
                            foreach (string s in queue)
                                File.AppendAllText(path, Environment.NewLine + s);
                            queue.Clear();
                        }
                }
                catch (Exception e)
                {
                    Debug.Log($"error {path} {e}");
                }
                Thread.Sleep(sleepTimeMiliseconde);
            }
            thread = null;
        }
    }
}