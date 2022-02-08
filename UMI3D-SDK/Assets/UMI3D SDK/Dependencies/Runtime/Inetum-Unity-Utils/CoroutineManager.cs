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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace inetum.unityUtils
{
    public class CoroutineManager
    {
        public bool shouldStop { get => _shouldStop() || !running; }
        readonly Func<bool> _shouldStop;
        readonly Func<bool> DefaultShouldStop = () => false;
        readonly MonoBehaviour mono;
        public object Current => null;

        bool running = false;

        public CoroutineManager(MonoBehaviour monoBehaviour, Func<bool> shouldStop)
        {
            mono = monoBehaviour;
            _shouldStop = shouldStop ?? DefaultShouldStop;
            running = true;
        }

        public void Stop()
        {
            running = false;
            Clear();
        }

        public void Clear()
        {
            lock (enumerators)
            {
                enumerators.ForEach(c => mono.StopCoroutine(c));
                enumerators.Clear();
            }
        }

        public Coroutine Start(IEnumerator e)
        {
            Coroutine c = mono.StartCoroutine(Wrapper(e));
            lock (enumerators)
                enumerators.Add(c);
            return c;
        }
        public void Stop(Coroutine c)
        {
            lock (enumerators)
                enumerators.Remove(c);
            mono.StopCoroutine(c);
        }

        List<Coroutine> enumerators = new List<Coroutine>();

        IEnumerator Wrapper(IEnumerator e)
        {
            if (e != null)
                while (e.MoveNext())
                {
                    var c = e.Current;
                    if (running && c is IShouldYield b && !b.shouldYield)
                        continue;
                    yield return c;
                }
            yield break;
        }

        public interface IShouldYield
        {
            Task Task { get; }
            bool shouldYield { get; }
        }

        public class Breaker : IShouldYield
        {

            object taskLocker = new object();
            float maxDuration = 0.1f;
            float nextBreackTime;
            int frame = -1;

            public Breaker(float maxDuration)
            {
                this.maxDuration = maxDuration;
            }

            public bool shouldYield
            {
                get
                {
                    lock (taskLocker)
                    {
                        if (frame != Time.frameCount)
                        {
                            frame = Time.frameCount;
                            NextFrame();
                            return false;
                        }
                        if (Time.realtimeSinceStartup > nextBreackTime)
                        {
                            NextFrame();
                            return true;
                        }
                        return false;
                    }
                }
            }

            public Task Task => _Task();

            async Task _Task()
            {
                bool shouldAwait = false;
                lock (taskLocker)
                {
                    if (frame != Time.frameCount)
                    {
                        frame = Time.frameCount;
                        NextFrame();
                        return;
                    }
                    if (Time.realtimeSinceStartup > nextBreackTime)
                    {
                        NextFrame();
                        shouldAwait = true;
                    }
                }
                if (shouldAwait)
                    await Task.Yield();

            }

            void NextFrame()
            {
                nextBreackTime = Time.realtimeSinceStartup + maxDuration;
            }
        }
    }
}
