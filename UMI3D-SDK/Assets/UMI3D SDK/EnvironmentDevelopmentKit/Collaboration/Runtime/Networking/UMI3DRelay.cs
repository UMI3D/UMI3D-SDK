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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public abstract class UMI3DRelay<To, Source, Frame> : ThreadLoop where To : class where Source : class where Frame : class
    {
        protected readonly object framesPerSourceLock = new();
        protected readonly object lastFrameSentToLock = new();

        protected Dictionary<Source, Frame> framesPerSource = new();

        protected Dictionary<To, Dictionary<Source, Frame>> lastFrameSentTo = new();

        protected List<Source> tempSources;

        public void RemoveSource(Source source)
        {
            lock (framesPerSourceLock)
                framesPerSource.Remove(source);
            lock (lastFrameSentToLock)
                lastFrameSentTo.Select(k => k.Value).ForEach(d => d.Remove(source));
        }

        public void RemoveTo(To to)
        {
            lock (lastFrameSentToLock)
                lastFrameSentTo.Remove(to);
        }

        public void Clear()
        {
            this.tempSources.Clear();

            lock (framesPerSourceLock)
                framesPerSource.Clear();
            lock (lastFrameSentToLock)
                lastFrameSentTo.Clear();
        }

        public void SetFrame(Source source, Frame frame)
        {
            lock (framesPerSourceLock)
            {
                if (source != null)
                    framesPerSource[source] = frame;
            }
        }

        protected abstract IEnumerable<To> GetTargets();

        protected abstract ulong GetTime();

        /// <summary>
        /// Send data to <paramref name="to"/> from <paramref name="source"/>.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="source"></param>
        /// <param name="force"></param>
        protected abstract void Send(To to, List<Source> source, bool force);

        public bool forceSendToAll;

        protected UMI3DRelay()
        {
            this.tempSources = new();

            UMI3DCollaborationServer.Instance.OnServerStart.AddListener(() => {
                StartLoop();
            });

            UMI3DCollaborationServer.Instance.OnServerStop.AddListener(() => {
                StopLoop();
            });

            NotificationHub.Default.Subscribe(
                this,
                QuittingManagerNotificationKey.ApplicationIsQuitting,
                null,
                StopLoop
            );

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

            var targets = GetTargets();
            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                this.tempSources.Clear();
                bool force = GetSourcesToSendTo(target, time, this.tempSources);

                if (tempSources.Count == 0)
                    continue;

                Send(target, tempSources, forceSendToAll || force);
            }

            if (forceSendToAll)
                forceSendToAll = false;
        }


        protected virtual bool GetSourcesToSendTo(To to, ulong time, List<Source> sources)
        {
            lock (lastFrameSentToLock)
            {
                if (!lastFrameSentTo.ContainsKey(to))
                    lastFrameSentTo.Add(to, new Dictionary<Source, Frame>());

                foreach (var kFrame in framesPerSource)
                {
                    if (!lastFrameSentTo[to].ContainsKey(kFrame.Key) || lastFrameSentTo[to][kFrame.Key] != kFrame.Value)
                    {
                        lastFrameSentTo[to][kFrame.Key] = kFrame.Value;
                        sources.Add(kFrame.Key);
                    }
                }
            }

            return false;
        }

        protected override void StopLoop()
        {
            base.StopLoop();

            Clear();
        }
    }
}