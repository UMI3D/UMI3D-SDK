/*
Copyright 2019 Gfi Informatique

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

using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_WEBRTC
using Unity.WebRTC;
#endif
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class WebRTCconnection : IWebRTCconnection
    {
        public string id;

        enum Step { Null, LocalSet, RemoteSet, BothSet, IceChecked, Running };
        Step step = Step.Null;

        public List<DataChannel> channels;
        //private List<RTCRtpSender> Senders;
        public Action<DataChannel> onDataChannelOpen;
        public Action<DataChannel> onDataChannelClose;
#if UNITY_WEBRTC
        private RTCPeerConnection rtc;
        public Action<RTCTrackEvent> onTrack;
        public RTCIceConnectionState connectionState = RTCIceConnectionState.New;
#endif
        public Action<byte[], DataChannel> onMessage;
        public Action onDisconected;

        public string targetId { get; private set; }

        public IceServer[] iceServers;

        public bool useRTC = true;

        /// <summary>
        /// Initialize the connection
        /// </summary>
        /// <param name="targetId"></param>
        public void Init(string id, string targetId, bool instanciateChannel)
        {
            this.id = id;
            this.targetId = targetId;
#if UNITY_WEBRTC
            Log("GetSelectedSdpSemantics");
            var configuration = GetSelectedSdpSemantics();
            rtc = new RTCPeerConnection(ref configuration);
            rtc.OnIceCandidate = OnIceCandidate;
            rtc.OnIceConnectionChange = OnIceConnectionChange;
            rtc.OnDataChannel = (c) => { OnDataChannel(c); };
            rtc.OnNegotiationNeeded = OnNegotiationNeeded;
            rtc.OnTrack = OnTrack;
            //Senders = new List<RTCRtpSender>();
#endif
            if (instanciateChannel)
                foreach (var channel in channels)
                {
                    CreateDataChannel(channel.reliable, channel.Label);
                }
        }

        public void Close()
        {
            foreach (var c in channels)
            {
                c.Closed();
            }
        }

#if UNITY_WEBRTC
        void OnNegotiationNeeded()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_OnNegotiationNeeded());
        }

        IEnumerator _OnNegotiationNeeded()
        {
            Log("Negotiation Needed");
            var op = rtc.CreateOffer(ref OfferOptions);
            yield return op;
            if (op.IsError)
                Log($"failed to create session: ${op.Error}");
            else
                UnityMainThreadDispatcher.Instance().Enqueue(OnCreateOfferSuccess(op.Desc));
        }
#endif
        /// <summary>
        /// Send a message via a datachannel.
        /// </summary>
        /// <param name="data">Message.</param>
        /// <param name="channel">Datachannel.</param>
        public void Send(byte[] data, DataChannel channel, bool tryToSendAgain = true)
        {

            if (channel == null)
            {
#if UNITY_WEBRTC
                if (connectionState != RTCIceConnectionState.Completed)
                    Debug.LogError($"Channel should not be null {connectionState}");
#endif
                return;
            }
#if UNITY_WEBRTC
            if (connectionState == RTCIceConnectionState.Disconnected)
                Debug.LogWarning($"Connection state is {connectionState}, should not try to send data");
#endif
            switch (channel.State)
            {
                case ChannelState.Opening:
                    if (tryToSendAgain)
                        channel.MessageNotSend.Add(data);
                    break;
                case ChannelState.Open:
                    channel.Send(data);
                    break;
                case ChannelState.Close:

                    break;
            }

        }

        /// <summary>
        /// Send a bytes message using the first Data channel found.
        /// </summary>
        /// <param name="data">Message to send.</param>
        /// <param name="reliable">Should the dataChannel be reliable.</param>
        public void Send(byte[] data, bool reliable, bool tryToSendAgain = true)
        {

            var channel = channels.Find((c) => c.reliable == reliable && c.type == DataType.Data);
            if (channel == null)
            {
#if UNITY_WEBRTC
                if (connectionState != RTCIceConnectionState.Completed)
                    Debug.LogWarning($"No suitable channel found {connectionState}");
#endif
                Debug.Log("arf");
                return;
            }
            Send(data, channel, tryToSendAgain);
        }

        /// <summary>
        /// Send a bytes message using the first Data channel found.
        /// </summary>
        /// <param name="data">Message to send.</param>
        /// <param name="reliable">Should the dataChannel be reliable.</param>
        public void Send(byte[] data, bool reliable, DataType dataType, bool tryToSendAgain = true)
        {
            var channel = channels.Find((c) => c.reliable == reliable && c.type == dataType);
            if (channel == null)
            {
#if UNITY_WEBRTC
                if (connectionState != RTCIceConnectionState.Completed)
                    Debug.LogWarning($"No suitable channel found for {reliable} && {dataType}  {connectionState}");
#endif
                return;
            }
            Send(data, channel, tryToSendAgain);
        }

        #region offer

        /// <summary>
        /// Action invoke when an offer is created.
        /// </summary>
        public Action<string> onOfferCreated;

#if UNITY_WEBRTC
        public RTCOfferOptions OfferOptions = new RTCOfferOptions
        {
            iceRestart = true,
            offerToReceiveAudio = false,
            offerToReceiveVideo = false
        };
#endif

        /// <summary>
        /// Create an offer
        /// </summary>
        public void Offer()
        {
#if UNITY_WEBRTC
            UnityMainThreadDispatcher.Instance().Enqueue(CreateOffer());
#endif
        }

#if UNITY_WEBRTC
        IEnumerator CreateOffer()
        {
            //foreach(var channel in channels)
            //{
            //    CreateDataChannel(channel.reliable, channel.Label);
            //}

            Log("createOffer start");
            var op = rtc.CreateOffer(ref OfferOptions);
            yield return op;
            if (op.IsError)
                Log($"failed to create session: ${op.Error}");
            else
                UnityMainThreadDispatcher.Instance().Enqueue(OnCreateOfferSuccess(op.Desc));
        }

        IEnumerator OnCreateOfferSuccess(RTCSessionDescription desc)
        {
            Log($"Offer from {targetId}\n{desc.sdp}");
            Log("SetLocalDescription start");
            var op = rtc.SetLocalDescription(ref desc);
            yield return op;

            if (op.IsError)
                Log($"SetLocalDescription error: {op.Error}");
            else
            {
                Log("SetLocalDescription complete");
                step = step == Step.RemoteSet ? Step.BothSet : Step.LocalSet;
            }

            if (onOfferCreated == null)
                Debug.LogError("No onOfferCreated listener!");
            else
                onOfferCreated.Invoke(desc.sdp);
        }
#endif

        #endregion

        #region answer

        public Action<string> onAnswerCreated;

#if UNITY_WEBRTC
        /// <summary>
        /// 
        /// </summary>
        public RTCAnswerOptions AnswerOptions = new RTCAnswerOptions
        {
            iceRestart = false,
        };


        /// <summary>
        /// Create an Answer and set description
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public IEnumerator CreateAnswer(RTCSessionDescription description)
        {

            Log($"SetRemoteDescription start {description.type} {description.sdp}");

            var op2 = rtc.SetRemoteDescription(ref description);
            yield return op2;
            if (op2.IsError)
                Log($"SetRemoteDescription error: {op2.Error} {op2.Error.errorType} {description}");
            else
                Log("SetRemoteDescription complete");

            Log("CreateAnswer start");
            // Since the 'remote' side has no media stream we need
            // to pass in the right constraints in order for it to
            // accept the incoming offer of audio and video.

            var op3 = rtc.CreateAnswer(ref AnswerOptions);
            yield return op3;

            if (op3.IsError)
                Log($"CreateAnswer error: {op3.Error}");
            else
                yield return OnCreateAnswerSuccess(op3.Desc);

        }


        IEnumerator OnCreateAnswerSuccess(RTCSessionDescription desc)
        {
            Log($"SetLocalDescription start {desc.type} {desc.sdp}");
            var op = rtc.SetLocalDescription(ref desc);
            yield return op;

            if (op.IsError)
                Log($"SetLocalDescription error: {op.Error.errorType}");
            else
            {
                Log("SetLocalDescription complete");
                step = step == Step.LocalSet ? Step.BothSet : Step.RemoteSet;
            }

            if (onAnswerCreated == null)
                Debug.LogError("No onAnswerCreated listener!");
            else
                onAnswerCreated.Invoke(desc.sdp);
        }
#endif
        #endregion

        #region remote session
#if UNITY_WEBRTC
        /// <summary>
        /// Set remote Session.
        /// </summary>
        /// <param name="sdp"></param>
        public void SetRemoteSession(string sdp)
        {
            RTCSessionDescription description = new RTCSessionDescription();
            description.sdp = sdp;
            description.type = RTCSdpType.Answer;
            UnityMainThreadDispatcher.Instance().Enqueue(OnSetRemoteSession(description));
        }

        IEnumerator OnSetRemoteSession(RTCSessionDescription desc)
        {
            Log("SetRemoteDescription start");
            var op2 = rtc.SetRemoteDescription(ref desc);
            yield return op2;
            if (op2.IsError)
                Log($"SetRemoteDescription {op2.Error}: {op2.Error.errorType}");
            else
            {
                Log("SetRemoteDescription complete");
                step = step == Step.LocalSet ? Step.BothSet : Step.RemoteSet;
            }
        }
#endif

        #endregion

        #region ice
#if UNITY_WEBRTC
        public Action<RTCIceCandidate> onIceCandidate;

        /// <summary>
        /// Add a IceCandidate
        /// </summary>
        /// <param name="candidate"></param>
        public void AddIceCandidate(RTCIceCandidate candidate)
        {
            Log($"Try Add Ice Candidate {candidate}");
            UnityMainThreadDispatcher.Instance().Enqueue(AddIceCandidateWhenReady(candidate));
        }

        IEnumerator AddIceCandidateWhenReady(RTCIceCandidate candidate)
        {
            var wait = new WaitForFixedUpdate();
            while (step < Step.BothSet)
                yield return wait;
            Log($"Add Ice Candidate {candidate}");
            rtc.AddIceCandidate(ref candidate);
        }

        void OnIceCandidate(RTCIceCandidate candidate)
        {
            Log($"Create Ice Candidate {candidate}");
            if (onIceCandidate == null)
                Log("no listener for ICE candidate!");
            else
                onIceCandidate.Invoke(candidate);
        }

        void OnAddIceCandidateSuccess()
        {
            Log("addIceCandidate success");
        }

        void OnAddIceCandidateError(RTCError error)
        {
            Log($"failed to add ICE Candidate: ${error}");
        }

        void OnIceConnectionChange(RTCIceConnectionState state)
        {
            connectionState = state;
            switch (state)
            {
                case RTCIceConnectionState.New:
                    Log("IceConnectionState: New");
                    break;
                case RTCIceConnectionState.Checking:
                    Log("IceConnectionState: Checking");
                    break;
                case RTCIceConnectionState.Closed:
                    Log("IceConnectionState: Closed");
                    break;
                case RTCIceConnectionState.Completed:
                    Log("IceConnectionState: Completed");
                    break;
                case RTCIceConnectionState.Connected:
                    Log("IceConnectionState: Connected");
                    break;
                case RTCIceConnectionState.Disconnected:
                    onDisconected?.Invoke();
                    Log("IceConnectionState: Disconnected");
                    break;
                case RTCIceConnectionState.Failed:
                    useRTC = false;
                    channels.ForEach(d => { if (d is WebRTCDataChannel wd) wd.useWebrtc = false;});
                    Log("IceConnectionState: Failed");
                    break;
                case RTCIceConnectionState.Max:
                    Log("IceConnectionState: Max");
                    break;
                default:
                    break;
            }
        }
#endif
        #endregion

        #region data

        public bool Any(Func<DataChannel, bool> predicate)
        {
            return channels.Any(predicate);
        }

        /// <summary>
        /// Find first suitable channel.
        /// </summary>
        /// <param name="predicate">predicate for suitable channel.</param>
        /// <returns>First suitable channel or null if no match</returns>
        public DataChannel Find(Func<DataChannel, bool> predicate)
        {
            return channels.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Find first suitable channel.
        /// </summary>
        /// <param name="predicate">predicate for suitable channel.</param>
        /// <param name="channel">First matching DataChannel.</param>
        /// <returns>True if a channel was found</returns>
        public bool Find(Func<DataChannel, bool> predicate, out DataChannel channel)
        {
            channel = Find(predicate);
            return channel != default;
        }

        /// <summary>
        /// Find first suitable channel.
        /// </summary>
        /// <param name="reliable">should this channel be reliable.</param>
        /// <param name="dataType">datatype of the channel.</param>
        /// <param name="channel">First matching DataChannel.</param>
        /// <returns>True if a channel was found</returns>
        public bool Find(bool reliable, DataType dataType, out DataChannel channel)
        {
            return Find((c) => c.reliable == reliable && c.type == DataType.Data, out channel);
        }

        private void CreateDataChannel(bool reliable, string dataChannelName)
        {
#if UNITY_WEBRTC
            RTCDataChannelInit conf = new RTCDataChannelInit(reliable);
            OnDataChannel(rtc.CreateDataChannel(dataChannelName, ref conf));
#else
            var dc = channels.Find((c) => c.Label == dataChannelName) as WebRTCDataChannel;
            bool isOpen = false;
            if (dc == null)
            {
                dc = new WebRTCDataChannel(id, targetId, dataChannelName, reliable, DataType.Data);
                channels.Add(dc);
                Log($"create new channel [{dataChannelName}]");
            }
            else
            {
                isOpen = true;
            }
            reliable = dc.reliable;
            dc.Created();
#endif
        }

#if UNITY_WEBRTC
        private void OnDataChannel(RTCDataChannel channel)
        {
            Log($"new dataChannel {channel.Label}");
            bool reliable = false;
            var dc = channels.Find((c) => c.Label == channel.Label) as WebRTCDataChannel;
            bool isOpen = false;
            if (dc == null)
            {
                dc = new WebRTCDataChannel(id, targetId, channel.Label, false, DataType.Data);
                channels.Add(dc);
                Log($"create new channel [{channel.Label}]");
            }
            else
            {
                isOpen = true;
            }
            reliable = dc.reliable;
            dc.dataChannel = channel;
            channel.OnMessage = (bytes) => OnDataChannelMessage(dc, bytes);
            channel.OnOpen = () => OnDataChannelOpen(dc);
            channel.OnClose = () => OnDataChannelClose(dc);
            dc.Created();
            if (isOpen) channel.OnOpen.Invoke();
        }
#endif

        private void OnDataChannelMessage(DataChannel channel, byte[] bytes)
        {
            Log($"message [{channel.Label}]");
            channel.Messaged(bytes);
            if (onMessage != null)
                onMessage.Invoke(bytes, channel);
        }

        private void OnDataChannelClose(DataChannel channel)
        {
            Log("Data channel closed");
            channel.Closed();
            if (onDataChannelClose != null)
                onDataChannelClose.Invoke(channel);
        }

        private void OnDataChannelOpen(DataChannel channel)
        {
            Log("Data channel openned");
            channel.Open();
            //AddTracks();
            if (onDataChannelOpen != null)
                onDataChannelOpen.Invoke(channel);
        }

        #endregion

        #region track
        [Obsolete("Do not use")]
        public void AddTracks()
        {
            //Log("yo2");
            //if (audioStream != null)
            //    foreach (var track in audioStream.GetTracks())
            //    {
            //        Senders.Add(rtc.AddTrack(track, audioStream));
            //    }
            //if (videoStream != null)
            //    foreach (var track in videoStream.GetTracks())
            //    {
            //        Senders.Add(rtc.AddTrack(track, videoStream));
            //    }
        }
        [Obsolete("Do not use")]
        public void RemoveTracks()
        {
            //foreach (var sender in Senders)
            //{
            //    rtc.RemoveTrack(sender);
            //}
            //foreach (var sender in Senders)
            //{
            //    rtc.RemoveTrack(sender);
            //}
            //Senders.Clear();
        }

#if UNITY_WEBRTC
        private void OnTrack(RTCTrackEvent e)
        {
            Log("yo");
            //Senders.Add(rtc.AddTrack(e.Track, videoStream));
            if (onTrack != null)
                onTrack.Invoke(e);
        }
#endif

        /// <summary>
        /// Add a DataChannel
        /// </summary>
        /// <param name="channel"></param>
        public void AddDataChannel(DataChannel channel, bool instanciateChannel = true)
        {
            if (!channels.Contains(channel))
            {
                channels.Add(channel);

                if (instanciateChannel)
#if UNITY_WEBRTC
                    if (connectionState != RTCIceConnectionState.Failed && useRTC)
                        CreateDataChannel(channel.reliable, channel.Label);
                    else
#endif
                        if (channel is WebRTCDataChannel wChannel)
                            wChannel.useWebrtc = false;

            }
        }

        /// <summary>
        /// Remove a DataChannel
        /// </summary>
        /// <param name="channel"></param>
        public void RemoveDataChannel(DataChannel channel)
        {
            if (channels.Contains(channel))
            {
                channels.Remove(channel);
                channel.Close();
            }
        }

        #endregion

        #region configuration

#if UNITY_WEBRTC
        public RTCIceServer[] ToRTCIceServers(common.IceServer[] servers)
        {
            return servers.Select(s =>
            {
                RTCIceCredentialType cred;
                switch (s.credentialType)
                {
                    case IceCredentialType.Password:
                        cred = RTCIceCredentialType.Password;
                        break;
                    case IceCredentialType.OAuth:
                        cred = RTCIceCredentialType.OAuth;
                        break;
                    default:
                        Debug.Log($"Credential type {s.credentialType}");
                        cred = RTCIceCredentialType.Password;
                        break;
                }


                return new RTCIceServer()
                {
                    credential = s.credential,
                    credentialType = cred,
                    urls = s.urls,
                    username = s.username
                };
            }).ToArray();
        }

        RTCConfiguration GetSelectedSdpSemantics()
        {
            RTCConfiguration config = default;
            config.iceServers = ToRTCIceServers(iceServers);
            return config;
        }
#endif
        #endregion

        #region logs

        public string logPrefix = "WebRTC";

        public WebRTCconnection()
        {
        }

        void Log(string message)
        {
            //#if UNITY_EDITOR
            //Debug.Log($"[{logPrefix}]: " + message);
            //#endif
        }

        #endregion

    }
}