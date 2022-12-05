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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : AbstractMicrophoneListener<MicrophoneListener>, ILoggable
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Mumble;

        #region Mumble
        #region Init
        protected override void Start()
        {
            base.Start();

            QuittingManager.OnApplicationIsQuitting.AddListener(_OnApplicationQuit);

            UMI3DUser.OnUserMicrophoneIdentityUpdated.AddListener(IdentityUpdate);
            UMI3DUser.OnUserMicrophoneChannelUpdated.AddListener(ChannelUpdate);
            UMI3DUser.OnUserMicrophoneServerUpdated.AddListener(ServerUpdate);
            UMI3DUser.OnUserMicrophoneUseMumbleUpdated.AddListener(UseMumbleUpdate);

            UMI3DCollaborationClientServer.Instance.OnRedirectionStarted.AddListener(Reset);
            UMI3DCollaborationClientServer.Instance.OnLeavingEnvironment.AddListener(Reset);
            UMI3DCollaborationClientServer.Instance.OnLeaving.AddListener(Reset);

            UMI3DCollaborationClientServer.Instance.OnRedirectionAborted.AddListener(Heartbeat);
            UMI3DEnvironmentClient.EnvironementLoaded.AddListener(Heartbeat);

            identity = new Identity(GetIdentity);

            pushToTalkKeycode = KeyCode.M;
        }


        (string, int, string, string) GetIdentity()
        {
            if (UMI3DCollaborationEnvironmentLoader.Exists)
            {
                UMI3DUser user = UMI3DCollaborationEnvironmentLoader.Instance.GetClientUser();
                if (user != null)
                {
                    SetChannelToJoin(user.audioChannel);

                    var ip = ToMumbleUrl(user.audioServer);
                    return (ip.host, ip.port, user.audioLogin, user.audioPassword);
                }
            }
            return (null, 0, null, null);

        }

        private (string host, int port) ToMumbleUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string[] s = url.Split(':');

                var hostName = s[0];
                if (s.Length > 1 && int.TryParse(s[1], out int port))
                    return (hostName, port);
            }
            return (null, 0);
        }

        protected override void _OnApplicationQuit()
        {
            base._OnApplicationQuit();

            UMI3DUser.OnUserMicrophoneIdentityUpdated.RemoveListener(IdentityUpdate);
            UMI3DUser.OnUserMicrophoneChannelUpdated.RemoveListener(ChannelUpdate);
            UMI3DUser.OnUserMicrophoneServerUpdated.RemoveListener(ServerUpdate);
            UMI3DUser.OnUserMicrophoneUseMumbleUpdated.RemoveListener(UseMumbleUpdate);

            UMI3DEnvironmentClient.EnvironementLoaded.RemoveListener(Heartbeat);
        }
        #endregion

        bool reloadOnce = false;
        async void IdentityUpdate(UMI3DUser user)
        {
            if (user.isClient)
            {
                if (!reloadOnce)
                {
                    reloadOnce = true;
                    Reset();
                    await Delay(1000);
                    Heartbeat();
                    reloadOnce = false;
                }
            }
        }
        void ChannelUpdate(UMI3DUser user)
        {
            if (user.isClient)
            {
                SetChannelToJoin(user.audioChannel);
            }
        }
        async void ServerUpdate(UMI3DUser user)
        {
            if (user.isClient)
            {
                if (!reloadOnce)
                {
                    reloadOnce = true;
                    Reset();
                    await Delay(1000);
                    Heartbeat();
                    reloadOnce = false;
                }
            }
        }

        void UseMumbleUpdate(UMI3DUser user)
        {
            if (user.isClient)
            {

            }
        }

        #region Data and settings


        public partial class PacketData
        {
            private static bool running = false;
            private static bool stop = false;
            private MicrophoneListener microphone;
            private int timeStepMilliseconds;

            private PacketData(MicrophoneListener microphone) : this(microphone, 1000)
            { }

            private PacketData(MicrophoneListener microphone, int timeStepMilliseconds)
            {
                this.microphone = microphone;
                this.timeStepMilliseconds = timeStepMilliseconds;
            }

            partial void PacketsReceived(long count);
            partial void PacketsLost(long count);
            partial void PacketsSent(long count);

            public void Start()
            {
                Update(timeStepMilliseconds);
            }

            public void Stop()
            {
                stop = true;
            }

            private async void Update(int timeStepMilliseconds)
            {
                if (!running)
                {
                    running = true;
                    stop = false;

                    while (running && !stop)
                    {
                        if (Instance.mumbleStatus == MumbleStatus.Connected)
                        {

                            long numPacketsReceived = microphone.mumbleClient.NumUDPPacketsSent;
                            long numPacketsSent = microphone.mumbleClient.NumUDPPacketsReceieved;
                            long numPacketsLost = microphone.mumbleClient.NumUDPPacketsLost;

                            while (true)
                            {
                                await UMI3DAsyncManager.Delay(timeStepMilliseconds);

                                if (stop || Instance.mumbleStatus == MumbleStatus.Connected || microphone.mumbleClient == null)
                                    break;

                                long numSentThisSample = microphone.mumbleClient.NumUDPPacketsSent - numPacketsSent;
                                long numRecvThisSample = microphone.mumbleClient.NumUDPPacketsReceieved - numPacketsReceived;
                                long numLostThisSample = microphone.mumbleClient.NumUDPPacketsLost - numPacketsLost;

                                PacketsSent(-numSentThisSample);
                                PacketsReceived(-numRecvThisSample);
                                PacketsLost(-numLostThisSample);

                                numPacketsSent += numSentThisSample;
                                numPacketsReceived += numRecvThisSample;
                                numPacketsLost += numLostThisSample;
                            }
                        }
                        else
                            await UMI3DAsyncManager.Delay(timeStepMilliseconds);
                    }

                    running = false;
                }
            }
        }

        #endregion
        #endregion

#if UNITY_STANDALONE
        public bool UseNoiseReduction
        {
            get
            {
                if (!(mumbleMic is NAudioMicrophone nAudio)) return false;
                return nAudio.UseNoiseReducer;
            }
            set
            {
                if (!(mumbleMic is NAudioMicrophone nAudio)) return;
                nAudio.UseNoiseReducer = value;
            }
        }
#endif

        public List<DebugInfo> GetInfos()
        {
            return new List<DebugInfo>()
            {
                new DebugInfo<string>("Microphone",()=>{ return GetCurrentMicrophoneName(); }),
                new DebugInfo<bool>("HeartBeat",()=>{ return running; }),
                new DebugInfo<string>("Current Channels",()=>{ return channel; }),
                new DebugInfo<string>("Pending Channels",()=>{ return channel; }),

                new DebugInfo<string>("Mumble Status",()=>{ return mumbleStatus.ToString(); }),
                new DebugInfo<string>("Microphone Status",()=>{ return microphoneStatus.ToString(); }),
                new DebugInfo<bool>("| Is Sending Audio",()=>{ return mumbleMic?.isRecording ?? false; }),
                new DebugInfo<string>("| Push To Talk Key",()=>{ return pushToTalkKeycode.ToString(); }),
                new DebugInfo<float>("| Min Amplitude",()=>{ return minAmplitudeToSend; }),
                new DebugInfo<float>("| stop delay",()=>{ return voiceStopingDelaySeconds; }),

                new DebugInfo<bool>("Sampeling",()=>{ return debugSampling; }),
                new DebugInfo<float>("RMS",()=>{ return rms; }),
                new DebugInfo<float>("DB",()=>{ return db; }),
                new DebugInfo<bool>("Saturated",()=>{ return saturated; })
            };

        }

        public string GetLogName()
        {
            return "Microphone";
        }

        protected override void Log(object log)
        {
            UMI3DLogger.Log(log, scope);
        }
        protected override void LogException(Exception log)
        {
            UMI3DLogger.LogException(log, scope);
        }
        protected override void LogError(object log)
        {
            UMI3DLogger.LogError(log, scope);
        }

        protected override async Task Yield()
        {
            await UMI3DAsyncManager.Yield();
        }

        protected override async Task Delay(int millisecondsDelay)
        {
            await UMI3DAsyncManager.Delay(millisecondsDelay);
        }
    }
}