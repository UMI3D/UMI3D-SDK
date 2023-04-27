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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    public class SkeletonManager : Singleton<SkeletonManager>
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        public Dictionary<ulong, ISkeleton> skeletons { get; protected set; } = new Dictionary<ulong, ISkeleton>();

        public PersonalSkeleton skeleton => PersonalSkeleton.Exists ? PersonalSkeleton.Instance : null;

        public List<CollaborativeSkeleton> collaborativeSkeletons { get; protected set; }

        public ISkeleton GetSkeletonById(ulong id)
        {
            if ((skeleton as ISkeleton).userId == id)
            {
                return skeleton;
            }
            else
            {
                return collaborativeSkeletons.FirstOrDefault(cs => (cs as ISkeleton).userId == id);
            }
        }

        CollaborativeSkeletonsScene collabScene => CollaborativeSkeletonsScene.Exists ? CollaborativeSkeletonsScene.Instance : null;

        public class SkeletonEvent : UnityEvent<ulong> { };

        public SkeletonEvent skeletonEvent = new SkeletonEvent();

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        public bool sendTracking => _sendTracking;

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        [Tooltip("If true the avatar tracking is sent.")]
        protected bool _sendTracking = true;

        float targetTrackingFPS = 30f;
        bool sendCameraProperties = true;
        bool sendTrackingLoopOnce = false;

        public TrackingOption option;

        #region Dependency Injection

        private UMI3DCollaborationClientServer collaborationClientServerService;
        private UMI3DCollaborationEnvironmentLoader collaborativeLoaderService;

        public SkeletonManager() : base()
        {
            collaborationClientServerService = UMI3DCollaborationClientServer.Instance;
            collaborativeLoaderService = UMI3DCollaborationEnvironmentLoader.Instance;
            Init();
        }

        public SkeletonManager(UMI3DCollaborationClientServer collaborationClientServer, UMI3DCollaborationEnvironmentLoader collaborativeLoader) : base()
        {
            this.collaborationClientServerService = collaborationClientServer;
            this.collaborativeLoaderService = collaborativeLoader;
            Init();
        }

        #endregion Dependency Injection

        public void Init()
        {
            collaborationClientServerService.OnRedirection.AddListener(() => { skeletons.Clear(); InitSkeletons(); SetTrackingSending(_sendTracking); });
            collaborationClientServerService.OnReconnect.AddListener(() => { skeletons.Clear(); InitSkeletons(); SetTrackingSending(_sendTracking); });

            UMI3DCollaborationEnvironmentLoader.OnUpdateUserList += () => UpdateSkeletons(collaborativeLoaderService.JoinnedUserList);
        }

        protected async void InitSkeletons()
        {
            while (!UMI3DClientServer.Exists || UMI3DClientServer.Instance.GetUserId() == 0)
            {
                await UMI3DAsyncManager.Yield();
            }

            skeletons[UMI3DClientServer.Instance.GetUserId()] = skeleton;
        }

        protected void UpdateSkeletons(List<UMI3DUser> usersList)
        {
            List<ulong> idList = usersList.Select(u => u.id).ToList();

            var joinnedUsersId = idList.Except(skeletons.Keys).ToList();
            var deletedUsersId = skeletons.Keys.Except(idList).ToList();

            foreach (var userId in deletedUsersId)
            {
                if (skeletons.TryGetValue(userId, out var skeleton))
                {
                    GameObject.Destroy((skeleton as CollaborativeSkeleton).gameObject);
                    skeletons.Remove(userId);
                }
            }

            foreach (var userId in joinnedUsersId)
            {
                if (userId != UMI3DClientServer.Instance.GetUserId())
                {
                    GameObject go = new GameObject();
                    CollaborativeSkeleton cs = go.AddComponent<CollaborativeSkeleton>();
                    skeletons[userId] = cs;
                    cs.transform.parent = collabScene.transform;
                    cs.SetSubSkeletons();
                    cs.name = userId.ToString();
                    skeletonEvent.Invoke(userId);
                }
            }
        }

        public CollaborativeSkeleton GetCollaborativeSkeleton(ulong userId)
        {
            skeletons.TryGetValue(userId, out var cs);
            return cs as CollaborativeSkeleton;
        }

        public UserTrackingFrameDto GetFrame()
        {
            UnityEngine.Debug.Log("GetFrame");
            var frame = skeleton.GetFrame(option);
            frame.userId = collaborationClientServerService.GetUserId();
            //frame.refreshFrequency = targetTrackingFPS;
            return frame;
        }

        public void UpdateFrames(List<UserTrackingFrameDto> frames)
        {
            foreach (var frame in frames)
                UpdateFrame(frame);
        }

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            ISkeleton skeleton;

            if (!skeletons.TryGetValue(frame.userId, out skeleton))
                UMI3DLogger.LogWarning("User Avatar not found.", scope);

            skeleton.UpdateFrame(frame);
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

                        //Camera properties are not sent
                        if (sendCameraProperties)
                            GetCameraProperty();
                    }
                    catch (System.Exception e) { UnityEngine.Debug.LogException(e); }

                    await UMI3DAsyncManager.Delay((int)(1000f / targetTrackingFPS));
                }
                else
                    await UMI3DAsyncManager.Yield();
            }
            sendTrackingLoopOnce = false;
        }

        private async void SendAsyncBoneData(uint boneType)
        {
            if (sendTrackingLoopOnce)
                return;

            while (Exists && sendTracking && PersonalSkeleton.Instance.BonesAsyncFPS.ContainsKey(boneType)) 
            {
                if (PersonalSkeleton.Instance.BonesAsyncFPS[boneType] > 0)
                {
                    try
                    {
                        if (PersonalSkeleton.Instance.TrackedSkeleton.bones.ContainsKey(boneType))
                        {
                            var boneData = PersonalSkeleton.Instance.TrackedSkeleton.GetBone(boneType);

                            if (boneData != null)
                            {
                                boneData.userId = UMI3DCollaborationClientServer.Instance.GetUserId();
                                UMI3DClientServer.SendTracking(boneData);
                            }
                        }
                    }
                    catch (System.Exception e) { UnityEngine.Debug.LogException(e); }

                    await UMI3DAsyncManager.Delay((int)(1000f / PersonalSkeleton.Instance.BonesAsyncFPS[boneType]));

                }
                else
                    await UMI3DAsyncManager.Yield();
            }
        }

        /// <summary>
        /// Set the number of tracked frame per second that are sent to the server.
        /// </summary>
        /// <param name="newFPSTarget"></param>
        public void SetFPSTarget(float newFPSTarget)
        {
            targetTrackingFPS = newFPSTarget;
        }

        public void SetBoneFPSTarget(uint boneType, float newFPSTarget)
        {
            if (newFPSTarget != targetTrackingFPS)
            {
                PersonalSkeleton.Instance.BonesAsyncFPS[boneType] = newFPSTarget;
                SendAsyncBoneData(boneType);
            }

            else if (PersonalSkeleton.Instance.BonesAsyncFPS.ContainsKey(boneType))
                PersonalSkeleton.Instance.BonesAsyncFPS.Remove(boneType);
        }

        public void SyncBoneFPS(uint boneType)
        {
            PersonalSkeleton.Instance.BonesAsyncFPS.Remove(boneType);
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

        public void HandlePoseRequest(PlayPoseDto playPoseDto)
        {
            PoseDto poseDto = PoseManager.Instance.GetPose(playPoseDto.poseKey, playPoseDto.indexInList);
            skeletons.TryGetValue(playPoseDto.userID, out ISkeleton skeleton);
            if (playPoseDto.stopPose)
            {
                (skeleton as PersonalSkeleton)?.poseSkeleton.StopPose(false, new List<PoseDto> { poseDto});
                (skeleton as CollaborativeSkeleton)?.poseSkeleton.StopPose(false, new List<PoseDto> { poseDto });
            }
            else
            {
                (skeleton as PersonalSkeleton)?.poseSkeleton.SetPose(false, new List<PoseDto> { poseDto }, true);
                (skeleton as CollaborativeSkeleton)?.poseSkeleton.SetPose(false, new List<PoseDto> { poseDto }, true);
            }
        }
    }
}