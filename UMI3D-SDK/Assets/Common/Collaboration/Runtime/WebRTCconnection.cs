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
using Unity.WebRTC;
using UnityEngine;

namespace umi3d.common
{
    public class WebRTCconnection
    {
        enum Step { Null, LocalSet, RemoteSet, BothSet, IceChecked, Running };
        Step step = Step.Null;

        private RTCPeerConnection rtc;
        public List<DataChannel> channels;
        private List<RTCRtpSender> Senders;
        public Action<DataChannel> onDataChannelOpen;
        public Action<DataChannel> onDataChannelClose;
        public Action<RTCTrackEvent> onTrack;
        public Action<byte[],DataChannel> onMessage;
        public RTCIceConnectionState connectionState = RTCIceConnectionState.New;
        string name;

        /// <summary>
        /// Initialize the connection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="audio"></param>
        /// <param name="video"></param>
        public void Init(string name,bool instanciateChannel)
        {
            this.name = name;
            Log("GetSelectedSdpSemantics");
            var configuration = GetSelectedSdpSemantics();
            rtc = new RTCPeerConnection(ref configuration);
            rtc.OnIceCandidate = OnIceCandidate;
            rtc.OnIceConnectionChange = OnIceConnectionChange;
            rtc.OnDataChannel = (c)=> {  OnDataChannel(c); };
            rtc.OnNegotiationNeeded = OnNegotiationNeeded;
            rtc.OnTrack = OnTrack;
            Senders = new List<RTCRtpSender>();
            if(instanciateChannel)
                foreach (var channel in channels)
                {
                    CreateDataChannel(channel.reliable, channel.Label);
                }
        }

        public void Close()
        {
            foreach(var c in channels)
            {
                c.Close();
            }
        }

        void OnNegotiationNeeded() {
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

        /// <summary>
        /// Send a string message using the first Data channel found.
        /// </summary>
        /// <param name="text">Message to send.</param>
        /// <param name="reliable">Should the dataChannel be reliable.</param>
        public void Send(string text, bool reliable)
        {
            var channel = channels.Find((c) => c.reliable == reliable && c.type == DataType.Data);
            if (channel == null) throw new Exception("No suitable channel found.");
            if (channel.IsOpen)
                channel.dataChannel.Send(text);
            else
                throw new Exception($"Data Channel {channel.Label} is not open yet");
        }

        /// <summary>
        /// Send a bytes message using the first Data channel found.
        /// </summary>
        /// <param name="data">Message to send.</param>
        /// <param name="reliable">Should the dataChannel be reliable.</param>
        public void Send(byte[] data, bool reliable)
        {
            
            var channel = channels.Find((c) => c.reliable == reliable && c.type == DataType.Data);
            if (channel == null)
            {
                if (connectionState != RTCIceConnectionState.Completed) Debug.LogWarning($"No suitable channel found");
                return;
            }
            if (channel.IsOpen)
            {
                channel.dataChannel.Send(data);
            }
            else
            {
                channel.MessageNotSend.Add(data);
            }
        }

        /// <summary>
        /// Send a bytes message using the first Data channel found.
        /// </summary>
        /// <param name="data">Message to send.</param>
        /// <param name="reliable">Should the dataChannel be reliable.</param>
        public void Send(byte[] data, bool reliable, DataType dataType)
        {
            var channel = channels.Find((c) => c.reliable == reliable && c.type == dataType);
            if (channel == null)
            {
                if (connectionState != RTCIceConnectionState.Completed) Debug.LogWarning($"No suitable channel found for {reliable} && {dataType}");
                return;
            }
            if (channel.IsOpen) 
                channel.dataChannel.Send(data);
            else
                channel.MessageNotSend.Add(data);
        }

        #region offer
        /// <summary>
        /// Action invoke when an offer is created.
        /// </summary>
        public Action<string> onOfferCreated;

        public RTCOfferOptions OfferOptions = new RTCOfferOptions
        {
            iceRestart = false,
            offerToReceiveAudio = false,
            offerToReceiveVideo = false
        };

        /// <summary>
        /// Create an offer
        /// </summary>
        public void Offer()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(CreateOffer());
        }

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
            Log($"Offer from {name}\n{desc.sdp}");
            Log("SetLocalDescription start");
            var op = rtc.SetLocalDescription(ref desc);
            yield return op;

            if (op.IsError)
                Log($"SetLocalDescription error: {op.Error}");
            else
            {
                Log("SetLocalDescription complete");
                step = (step == Step.RemoteSet) ? Step.BothSet : Step.LocalSet;
            }

            if (onOfferCreated == null)
                Debug.LogError("No onOfferCreated listener!");
            else
                onOfferCreated.Invoke(desc.sdp);
        }

        #endregion

        #region answer

        public Action<string> onAnswerCreated;

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
                step = (step == Step.LocalSet) ? Step.BothSet : Step.RemoteSet;
            }

            if (onAnswerCreated == null)
                Debug.LogError("No onAnswerCreated listener!");
            else
                onAnswerCreated.Invoke(desc.sdp);
        }

        #endregion

        #region remote session

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
                step = (step == Step.LocalSet) ? Step.BothSet : Step.RemoteSet;
            }
        }

        #endregion

        #region ice

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
                    Log("IceConnectionState: Disconnected");
                    break;
                case RTCIceConnectionState.Failed:
                    Log("IceConnectionState: Failed");
                    break;
                case RTCIceConnectionState.Max:
                    Log("IceConnectionState: Max");
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region data

        private void CreateDataChannel(bool reliable,string dataChannelName)
        {
            RTCDataChannelInit conf = new RTCDataChannelInit(reliable);
            OnDataChannel(rtc.CreateDataChannel(dataChannelName, ref conf));
        }

        private void OnDataChannel(RTCDataChannel channel)
        {
            Log($"new dataChannel {channel.Label}");
            bool reliable = false;
            var dc = channels.Find((c) => c.Label == channel.Label);
            bool isOpen = false;
            if (dc == null)
            {
                dc = new DataChannel(channel.Label, false, DataType.Data);
                channels.Add(dc);
                Log($"create new channel [{channel.Label}]");
            }
            else
            {
                isOpen = true;
            }
            reliable = dc.reliable;
            dc.dataChannel = channel;
            channel.OnMessage = (bytes) => OnDataChannelMessage(dc,bytes);
            channel.OnOpen = () => OnDataChannelOpen(dc);
            channel.OnClose = () => OnDataChannelClose(dc);
            dc.Created();
            if (isOpen) channel.OnOpen.Invoke();
        }

        private void OnDataChannelMessage(DataChannel channel, byte[] bytes)
        {
            Log($"message [{channel.Label}]");
            channel.Message(bytes);
            if (onMessage != null)
                onMessage.Invoke(bytes, channel);
        }

        private void OnDataChannelClose(DataChannel channel)
        {
            Log("Data channel closed");
            channel.Close();
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

        private void OnTrack(RTCTrackEvent e)
        {
            Log("yo");
            //Senders.Add(rtc.AddTrack(e.Track, videoStream));
            if (onTrack != null)
                onTrack.Invoke(e);
        }

        /// <summary>
        /// Add a DataChannel
        /// </summary>
        /// <param name="channel"></param>
        public void AddDataChannel(DataChannel channel)
        {
            if (!channels.Contains(channel))
            {
                channels.Add(channel);
                CreateDataChannel(channel.reliable, channel.Label);
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
                channel.dataChannel.Close();
            }
        }

        #endregion

        #region configuration

        public string[] iceServers = new string[] { "stun:stun.l.google.com:19302" };

        RTCConfiguration GetSelectedSdpSemantics()
        {
            RTCConfiguration config = default;
            config.iceServers = new RTCIceServer[]
            {
            new RTCIceServer { urls = iceServers }
            };
            return config;
        }

        #endregion

        #region logs

        public string logPrefix = "WebRTC";

        void Log(string message)
        {
//#if UNITY_EDITOR
//            Debug.Log($"[{logPrefix}]: " + message);
//#endif
        }

        #endregion

    }
}