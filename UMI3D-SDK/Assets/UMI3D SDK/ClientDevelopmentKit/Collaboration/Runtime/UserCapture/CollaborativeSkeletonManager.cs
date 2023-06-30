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
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.collaboration.userCapture
{
    public interface ICollaborativeSkeletonsManager
    {
        Dictionary<ulong, ISkeleton> skeletons { get; }

        CollaborativeSkeletonsScene collabScene { get; }

        ISkeleton GetSkeletonById(ulong userId);
    }

    public class CollaborativeSkeletonManager : Singleton<CollaborativeSkeletonManager>, ISkeletonManager, ICollaborativeSkeletonsManager
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration;

        public virtual Dictionary<ulong, ISkeleton> skeletons { get; protected set; } = new();

        public virtual PersonalSkeleton personalSkeleton => personnalSkeletonManager.personalSkeleton;

        public virtual CollaborativeSkeletonsScene collabScene => CollaborativeSkeletonsScene.Exists ? CollaborativeSkeletonsScene.Instance : null;

        public event Action<ulong> skeletonEvent;

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        public bool ShouldSendTracking { get; protected set; } = true;

        public UMI3DSkeletonHierarchy StandardHierarchy
        {
            get
            {
                _standardHierarchy ??= new UMI3DSkeletonHierarchy((collaborativeLoaderService.LoadingParameters as UMI3DUserCaptureLoadingParameters).SkeletonHierarchyDefinition);
                return _standardHierarchy;
            }
        }

        private UMI3DSkeletonHierarchy _standardHierarchy;

        private float targetTrackingFPS = 30f;
        private bool sendCameraProperties = true;
        private bool sendTrackingLoopOnce = false;

        public TrackingOption option;

        #region Dependency Injection

        private UMI3DCollaborationClientServer collaborationClientServerService;
        private UMI3DCollaborationEnvironmentLoader collaborativeLoaderService;
        private readonly PersonalSkeletonManager personnalSkeletonManager;

        public CollaborativeSkeletonManager() : base()
        {
            collaborationClientServerService = UMI3DCollaborationClientServer.Instance;
            collaborativeLoaderService = UMI3DCollaborationEnvironmentLoader.Instance;
            personnalSkeletonManager = PersonalSkeletonManager.Instance;
            Init();
        }

        public CollaborativeSkeletonManager(UMI3DCollaborationClientServer collaborationClientServer, UMI3DCollaborationEnvironmentLoader collaborativeLoader) : base()
        {
            this.collaborationClientServerService = collaborationClientServer;
            this.collaborativeLoaderService = collaborativeLoader;
            personnalSkeletonManager = PersonalSkeletonManager.Instance;
            Init();
        }

        #endregion Dependency Injection

        private bool canClearSkeletons = false;

        public void Init()
        {
            UMI3DCollaborationEnvironmentLoader.OnUpdateUserList += () => UpdateSkeletons(collaborativeLoaderService.JoinnedUserList);
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => { InitSkeletons(); SetTrackingSending(ShouldSendTracking); canClearSkeletons = true; });
            UMI3DCollaborationClientServer.Instance.OnLeavingEnvironment.AddListener(() => 
            { 
                if (canClearSkeletons)
                {
                    skeletons.Clear();
                    canClearSkeletons = false;
                }
            });
            UMI3DCollaborationClientServer.Instance.OnRedirection.AddListener(() => 
            {
                if (canClearSkeletons)
                {
                    skeletons.Clear();
                    canClearSkeletons = false;
                } 
            });
        }

        public void InitSkeletons()
        {
            skeletons[collaborationClientServerService.GetUserId()] = personalSkeleton;
            personalSkeleton.UserId = UMI3DClientServer.Instance.GetUserId();
        }

        protected void UpdateSkeletons(List<UMI3DUser> usersList)
        {
            List<ulong> idList = usersList.Select(u => u.id).ToList();
            idList.Remove(UMI3DClientServer.Instance.GetUserId());

            var joinnedUsersId = idList.Except(skeletons.Keys).ToList();
            var deletedUsersId = skeletons.Keys.Except(idList).ToList();

            foreach (var userId in deletedUsersId)
            {
                if (skeletons.TryGetValue(userId, out var skeleton) && skeleton is CollaborativeSkeleton collabSkeleton)
                {
                    UnityEngine.Object.Destroy(collabSkeleton.gameObject);
                    skeletons.Remove(userId);
                }
            }

            foreach (var userId in joinnedUsersId)
            {
                if (userId != UMI3DClientServer.Instance.GetUserId())
                {
                    skeletons[userId] = CreateSkeleton(userId, collabScene.transform, StandardHierarchy);
                    skeletonEvent?.Invoke(userId);
                }
            }
        }

        public virtual CollaborativeSkeleton CreateSkeleton(ulong userId, Transform parent, UMI3DSkeletonHierarchy skeletonHierarchy)
        {
            GameObject go = new GameObject();
            CollaborativeSkeleton cs = go.AddComponent<CollaborativeSkeleton>();
            cs.UserId = userId;
            cs.transform.parent = parent.transform;
            cs.name = userId.ToString();
            cs.SkeletonHierarchy = skeletonHierarchy;
            cs.SetSubSkeletons();

            // consider all bones we should have according to the hierarchy, and set all values to identity
            foreach (var bone in skeletonHierarchy.HierarchyDict.Keys)
            {
                if (cs.Bones.ContainsKey(bone))
                    cs.Bones[bone].s_Rotation = Quaternion.identity;
                else
                    cs.Bones[bone] = new ISkeleton.s_Transform() { s_Rotation = Quaternion.identity };
            }

            return cs;
        }

        public CollaborativeSkeleton GetCollaborativeSkeleton(ulong userId)
        {
            skeletons.TryGetValue(userId, out var cs);
            return cs as CollaborativeSkeleton;
        }

        public List<CollaborativeSkeleton> GetCollaborativeSkeletons()
        {
            return skeletons.Values.Where(x => x is CollaborativeSkeleton).Select(x => x as CollaborativeSkeleton).ToList();
        }

        public ISkeleton GetSkeletonById(ulong userId)
        {
            if (skeletons.ContainsKey(userId))
                return skeletons[userId];
            return null;
        }

        public UserTrackingFrameDto GetFrame()
        {
            var frame = personalSkeleton.GetFrame(option);
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
            if (!skeletons.TryGetValue(frame.userId, out ISkeleton skeleton))
            {
                UMI3DLogger.LogWarning($"Skeleton of user {frame.userId} not found. Cannot apply skeleton frame update.", scope);
                return;
            }

            skeleton.UpdateFrame(frame);
            skeleton.Compute();
        }

        public UserCameraPropertiesDto GetCameraProperty()
        {
            return personalSkeleton.GetCameraProperty();
        }

        private async void SendTrackingLoop()
        {
            if (sendTrackingLoopOnce)
                return;
            sendTrackingLoopOnce = true;
            while (Exists && ShouldSendTracking && UMI3DCollaborationClientServer.Instance.status != StatusType.NONE)
            {
                if (targetTrackingFPS > 0)
                {
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

            while (Exists && ShouldSendTracking && personalSkeleton.BonesAsyncFPS.ContainsKey(boneType))
            {
                if (personalSkeleton.BonesAsyncFPS[boneType] > 0)
                {
                    try
                    {
                        if (personalSkeleton.TrackedSkeleton.bones.ContainsKey(boneType))
                        {
                            var boneData = personalSkeleton.TrackedSkeleton.GetBone(boneType);

                            if (boneData != null)
                            {
                                boneData.userId = UMI3DCollaborationClientServer.Instance.GetUserId();
                                UMI3DClientServer.SendTracking(boneData);
                            }
                        }
                    }
                    catch (System.Exception e) { UnityEngine.Debug.LogException(e); }

                    await UMI3DAsyncManager.Delay((int)(1000f / personalSkeleton.BonesAsyncFPS[boneType]));
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
                personalSkeleton.BonesAsyncFPS[boneType] = newFPSTarget;
                SendAsyncBoneData(boneType);
            }
            else if (personalSkeleton.BonesAsyncFPS.ContainsKey(boneType))
                personalSkeleton.BonesAsyncFPS.Remove(boneType);
        }

        public void SyncBoneFPS(uint boneType)
        {
            personalSkeleton.BonesAsyncFPS.Remove(boneType);
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
        /// Setter for <see cref="ShouldSendTracking"/>.
        /// </summary>
        /// <param name="activeSending"></param>
        public void SetTrackingSending(bool activeSending)
        {
            this.ShouldSendTracking = activeSending;
            if (ShouldSendTracking)
                SendTrackingLoop();
        }

        public void HandlePoseRequest(ApplyPoseDto playPoseDto)
        {
            PoseDto poseDto = PoseManager.Instance.GetPose(playPoseDto.poseKey, playPoseDto.indexInList);
            skeletons.TryGetValue(playPoseDto.userID, out ISkeleton skeleton);
            if (playPoseDto.stopPose)
            {
                (skeleton as PersonalSkeleton)?.PoseSkeleton.StopPose(new List<PoseDto> { poseDto });
                (skeleton as CollaborativeSkeleton)?.PoseSkeleton.StopPose(new List<PoseDto> { poseDto });
            }
            else
            {
                (skeleton as PersonalSkeleton)?.PoseSkeleton.SetPose(false, new List<PoseDto> { poseDto }, true);
                (skeleton as CollaborativeSkeleton)?.PoseSkeleton.SetPose(false, new List<PoseDto> { poseDto }, true);
            }
        }
    }
}