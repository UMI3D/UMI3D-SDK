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

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using inetum.unityUtils;

namespace umi3d.edk.collaboration
{
    public abstract class UMI3DRelay<To,Source,Frame> : ThreadLoop where To : class where Source : class where Frame : class
    {
        protected readonly object framesPerSourceLock = new();
        protected readonly object lastFrameSentToLock = new();

        protected Dictionary<Source, Frame> framesPerSource = new();
        protected Dictionary<To, Dictionary<Source, Frame>> lastFrameSentTo = new();

        public void RemoveSource(Source source)
        {
            lock(framesPerSourceLock)
                framesPerSource.Remove(source);
            lock(lastFrameSentToLock)
                lastFrameSentTo.Select(k => k.Value).ForEach(d =>d.Remove(source));
        }

        public void RemoveTo(To to)
        {
            lock (lastFrameSentToLock)
                lastFrameSentTo.Remove(to);
        }

        public void Clear()
        {
            lock (framesPerSourceLock)
                framesPerSource.Clear();
            lock (lastFrameSentToLock)
                lastFrameSentTo.Clear();
        }

        public void SetFrame(Source source, Frame frame) {
            if(source != null)
                framesPerSource[source] = frame;
        }

        protected abstract IEnumerable<To> GetTargets();
        protected abstract ulong GetTime();
        protected abstract void Send(To to, List<Frame> frames, bool force);

        public bool forceSendToAll;

        protected UMI3DRelay()
        {
            UMI3DCollaborationServer.Instance.OnServerStart.AddListener(() => {
                StartLoop();
            });

            UMI3DCollaborationServer.Instance.OnServerStop.AddListener(() => {
                StopLoop();
            });

            QuittingManager.OnApplicationIsQuitting.AddListener(() => {
                StopLoop();
            });

#if UNITY_EDITOR
            Application.quitting += () =>
            {
                StopLoop();
            };
#endif
        }

        protected override void Update()
        {
            ulong time = GetTime(); //introduce wrong time. TB tested with frame.timestep

            KeyValuePair<Source,Frame>[] _framesPerSource;


            var r = new System.Random();
            lock (framesPerSourceLock)
                _framesPerSource = framesPerSource.OrderBy(s => r.Next()).ToArray();

            var targets = GetTargets();
            foreach(var target in targets)
            {
                if (target == null)
                    continue;

                (List<Frame> frames, bool force) = GetFramesToSend(target, time, _framesPerSource);

                if (frames.Count == 0)
                    continue;

                Send(target, frames, forceSendToAll || force);
            }

            if (forceSendToAll)
                forceSendToAll = false;
        }


        protected virtual (List<Frame> frames, bool force) GetFramesToSend(To to, ulong time, KeyValuePair<Source, Frame>[] framesPerSource)
        {
            List<Frame> frames = new();
            lock (lastFrameSentToLock)
            {
                if (!lastFrameSentTo.ContainsKey(to))
                    lastFrameSentTo.Add(to, new Dictionary<Source, Frame>());

                foreach (var kFrame in framesPerSource)
                {
                    if (!lastFrameSentTo[to].ContainsKey(kFrame.Key) || lastFrameSentTo[to][kFrame.Key] != kFrame.Value)
                    {
                        lastFrameSentTo[to][kFrame.Key] = kFrame.Value;
                        frames.Add(kFrame.Value);
                    }
                }
            }
            return (frames, false);
        }
    }
}