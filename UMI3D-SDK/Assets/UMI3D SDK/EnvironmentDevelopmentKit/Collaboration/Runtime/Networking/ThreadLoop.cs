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

using UnityEngine;
using System.Threading;
using System;

namespace umi3d.edk.collaboration
{
    public abstract class ThreadLoop
    {
        protected bool running { get; private set; } = false;
        private Thread sendAvatarFramesThread = null;
        private int millisecondsTimeOut;
        protected int MillisecondsTimeOut
        {
            get => millisecondsTimeOut;
            set
            {
                if(value > 0)
                    millisecondsTimeOut = value;
            }
        }

        protected void StartLoop()
        {
            OnLoopStart();
            sendAvatarFramesThread = new Thread(new ThreadStart(Looper));
            sendAvatarFramesThread.Start();
        }

        protected void StopLoop()
        {
            running = false;
            sendAvatarFramesThread = null;
        }

        /// <summary>
        /// Sends <see cref="UserTrackingFrameDto"/> every tick.
        /// </summary>
        private void Looper()
        {
            running = true;
            while (running)
            {
                try
                {
                    Update();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                Thread.Sleep(MillisecondsTimeOut);
            }
            OnLoopStop();
        }

        protected abstract void Update();
        protected virtual void OnLoopStart() { }
        protected virtual void OnLoopStop() { }
    }
}