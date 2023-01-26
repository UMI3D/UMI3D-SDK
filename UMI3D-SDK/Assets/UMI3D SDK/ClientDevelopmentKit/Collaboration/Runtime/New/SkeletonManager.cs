/*
Copyright 2019 - 2023 Inetum

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
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class SkeletonManager : Singleton<SkeletonManager>
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;
        public Dictionary<ulong,ISkeleton> skeletons { get; protected set; }

        PersonalSkeleton skeleton => PersonalSkeleton.Exists ? PersonalSkeleton.Instance : null;

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        public bool sendTracking => _sendTracking;

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        [Tooltip("If true the avatar tracking is sent.")]
        protected bool _sendTracking = true;

        float targetTrackingFPS = 1f;
        bool sendCameraProperties = true;
        bool sendTrackingLoopOnce = false;

        public TrackingOption option;

        public SkeletonManager() : base()
        {
            UnityEngine.Debug.Log("<color=green>New</color>");
            SetTrackingSending(_sendTracking);
        }

        public UserTrackingFrameDto GetFrame()
        {
            var frame = skeleton.GetFrame(option);
            frame.userId = UMI3DCollaborationClientServer.Instance.GetUserId();
            frame.refreshFrequency = targetTrackingFPS;
            return frame;
        }

        public void UpdateFrames(List<UserTrackingFrameDto> frames)
        {
            foreach (var frame in frames)
                UpdateFrame(frame);
        }

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            ISkeleton userAvatar;

            if (!skeletons.TryGetValue(frame.userId, out userAvatar))
                UMI3DLogger.LogWarning("User Avatar not found.", scope);

            userAvatar.UpdateFrame(frame);
        }

        public UserCameraPropertiesDto GetCameraProperty()
        {
            return skeleton.GetCameraProperty();
        }

        private async void SendTrackingLoop()
        {
            UnityEngine.Debug.Log("<color=green>Send loop</color>");
            if (sendTrackingLoopOnce)
                return;
            sendTrackingLoopOnce = true;
            while (Exists && sendTracking)
            {
                if (targetTrackingFPS > 0)
                {
                    UnityEngine.Debug.Log("<color=green>Send</color>");
                    try
                    {
                        var frame = GetFrame();

                        if (frame != null && UMI3DClientServer.Exists && UMI3DClientServer.Instance.GetUserId() != 0)
                            UMI3DClientServer.SendTracking(frame);

                        if (sendCameraProperties)
                            GetCameraProperty();
                    }
                    catch (System.Exception e){ UnityEngine.Debug.LogException(e); }

                    await UMI3DAsyncManager.Delay((int)(1000f / targetTrackingFPS));
                }
                else
                    await UMI3DAsyncManager.Yield();
            }
            sendTrackingLoopOnce = false;
        }

        /// <summary>
        /// Set the number of tracked frame per second that are sent to the server.
        /// </summary>
        /// <param name="newFPSTarget"></param>
        public void SetFPSTarget(int newFPSTarget)
        {
            targetTrackingFPS = newFPSTarget;
        }

        /// <summary>
        /// Set the list of streamed bones.
        /// </summary>
        /// <param name="bonesToStream"></param>
        public void SetStreamedBones(List<uint> bonesToStream)
        {
            UMI3DLogger.LogWarning("TODO", scope);
        }

        /// <summary>
        /// Setter for <see cref="sendCameraProperties"/>.
        /// </summary>
        /// <param name="activeSending"></param>
        public void SetCameraPropertiesSending(bool activeSending)
        {
            sendCameraProperties = activeSending;
        }

        /// <summary>
        /// Setter for <see cref="sendTracking"/>.
        /// </summary>
        /// <param name="activeSending"></param>
        public void SetTrackingSending(bool activeSending)
        {
            this._sendTracking = activeSending;
            if (_sendTracking)
                SendTrackingLoop();
        }
    }
}