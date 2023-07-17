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
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.collaboration.userCapture
{
    public class CollaborationSkeletonsManager : Singleton<CollaborationSkeletonsManager>, ISkeletonManager, ICollaborationSkeletonsManager
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration;

        #region Fields

        public virtual IReadOnlyDictionary<ulong, ISkeleton> Skeletons => skeletons;
        protected Dictionary<ulong, ISkeleton> skeletons = new();

        public virtual PersonalSkeleton personalSkeleton => personalSkeletonManager.personalSkeleton;

        public Vector3 worldSize => personalSkeleton.worldSize;

        public virtual CollaborativeSkeletonsScene CollabSkeletonsScene => CollaborativeSkeletonsScene.Exists ? CollaborativeSkeletonsScene.Instance : null;

        /// <summary>
        /// Invoked when a <see cref="CollaborativeSkeleton"/> is created. Parameter is the corresponding user id.
        /// </summary>
        public event Action<ulong> CollaborativeSkeletonCreated;

        public UMI3DSkeletonHierarchy StandardHierarchy
        {
            get
            {
                _standardHierarchy ??= new UMI3DSkeletonHierarchy((collaborativeLoaderService.AbstractLoadingParameters as IUMI3DUserCaptureLoadingParameters).SkeletonHierarchyDefinition);
                return _standardHierarchy;
            }
        }

        private UMI3DSkeletonHierarchy _standardHierarchy;

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        public bool ShouldSendTracking
        { 
            get
            {
                return shouldSendTracking;
            }
            set
            {
                shouldSendTracking = value;
                if (ShouldSendTracking)
                    SendTrackingLoop();
            }
        }
        protected bool shouldSendTracking = true;

        public IReadOnlyDictionary<uint, float> BonesAsyncFPS => personalSkeletonManager.BonesAsyncFPS;

        /// <summary>
        /// Number of tracked frame per second that are sent to the server.
        /// </summary>
        public float TargetTrackingFPS { get; set; } = 30f;
        public bool ShouldSendCameraProperties { get; set; } = true;
        private bool sendTrackingLoopOnce = false;

        #endregion Fields

        #region Dependency Injection

        private readonly IUMI3DCollaborationClientServer collaborationClientServerService;
        private readonly ILoadingManager collaborativeLoaderService;
        private readonly ICollaborationEnvironmentManager collaborativeEnvironmentManagementService;
        private readonly IPoseManager poseManager;
        private readonly ILateRoutineService routineService;
        private readonly ISkeletonManager personalSkeletonManager;

        public CollaborationSkeletonsManager() : base()
        {
            collaborationClientServerService = UMI3DCollaborationClientServer.Instance;
            collaborativeLoaderService = UMI3DCollaborationEnvironmentLoader.Instance;
            collaborativeEnvironmentManagementService = UMI3DCollaborationEnvironmentLoader.Instance;
            poseManager = PoseManager.Instance;
            routineService = CoroutineManager.Instance;
            personalSkeletonManager = PersonalSkeletonManager.Instance;
            Init();
        }

        public CollaborationSkeletonsManager(IUMI3DCollaborationClientServer collaborationClientServer,
                                            ILoadingManager collaborativeLoader,
                                            ICollaborationEnvironmentManager collaborativeEnvironmentManagementService,
                                            IPoseManager poseManager,
                                            ISkeletonManager personalSkeletonManager,
                                            ILateRoutineService routineService) : base()
        {
            this.collaborationClientServerService = collaborationClientServer;
            this.collaborativeLoaderService = collaborativeLoader;
            this.personalSkeletonManager = personalSkeletonManager;
            this.poseManager = poseManager;
            this.routineService = routineService;
            this.collaborativeEnvironmentManagementService = collaborativeEnvironmentManagementService;
            Init();
        }

        #endregion Dependency Injection

        #region LifeCycle

        private bool canClearSkeletons = false;

        private void Init()
        {
            collaborativeEnvironmentManagementService.OnUpdateUserList += () => UpdateSkeletons(collaborativeEnvironmentManagementService.UserList);
            collaborativeLoaderService.onEnvironmentLoaded.AddListener(() => { InitSkeletons(); if (ShouldSendTracking) SendTrackingLoop(); canClearSkeletons = true; });
            collaborationClientServerService.OnLeavingEnvironment.AddListener(Clear);
            collaborationClientServerService.OnRedirection.AddListener(Clear);
        }

        private void Clear()
        {
            if (canClearSkeletons)
            {
                skeletons.Clear();
                canClearSkeletons = false;
            }

            if (computeCoroutine != null)
                routineService.DettachLateRoutine(computeCoroutine);
        }

        private void InitSkeletons()
        {
            personalSkeleton.UserId = collaborationClientServerService.GetUserId();
            skeletons[personalSkeleton.UserId] = personalSkeleton;
        }

        private void UpdateSkeletons(IEnumerable<UMI3DUser> users)
        {
            List<ulong> readyUserIdList = users.Where(u => u.status >= StatusType.READY).Select(u => u.id).ToList();
            readyUserIdList.Remove(collaborationClientServerService.GetUserId());

            var joinedUsersId = readyUserIdList.Except(Skeletons.Keys).ToList();
            var deletedUsersId = Skeletons.Keys.Except(readyUserIdList).ToList();

            foreach (var userId in deletedUsersId)
            {
                if (Skeletons.TryGetValue(userId, out var skeleton) && skeleton is CollaborativeSkeleton collabSkeleton)
                {
                    UnityEngine.Object.Destroy(collabSkeleton.gameObject);
                    skeletons.Remove(userId);
                }
            }

            foreach (var userId in joinedUsersId)
            {
                if (userId != collaborationClientServerService.GetUserId())
                {
                    CreateSkeleton(userId, CollabSkeletonsScene.transform, StandardHierarchy);
                }
            }
        }

        public virtual CollaborativeSkeleton CreateSkeleton(ulong userId, Transform parent, UMI3DSkeletonHierarchy skeletonHierarchy)
        {
            GameObject go = new GameObject();
            CollaborativeSkeleton cs = go.AddComponent<CollaborativeSkeleton>();
            cs.UserId = userId;
            cs.name = $"skeleton_user_{userId}";

            if (parent != null)
                cs.transform.SetParent(parent);
            else
                cs.transform.SetParent(CollabSkeletonsScene.transform);

            cs.SkeletonHierarchy = skeletonHierarchy;

            var trackedSkeletonPrefab = (collaborativeLoaderService.AbstractLoadingParameters as IUMI3DCollabLoadingParameters).CollabTrackedSkeleton;
            var trackedSkeleton = UnityEngine.Object.Instantiate(trackedSkeletonPrefab, cs.transform).GetComponent<TrackedSkeleton>();
            
            var poseSkeleton = new PoseSubskeleton(poseManager);

            cs.SetSubSkeletons(trackedSkeleton, poseSkeleton);

            // consider all bones we should have according to the hierarchy, and set all values to identity
            foreach (var bone in skeletonHierarchy.Relations.Keys)
            {
                if (cs.Bones.ContainsKey(bone))
                    cs.Bones[bone].s_Rotation = Quaternion.identity;
                else
                    cs.Bones[bone] = new ISkeleton.s_Transform() { s_Rotation = Quaternion.identity };
            }

            skeletons[userId] = cs;
            CollaborativeSkeletonCreated?.Invoke(userId);
            return cs;
        }

        #endregion LifeCycle

        #region Skeleton getters

        public CollaborativeSkeleton GetCollaborativeSkeleton(ulong userId)
        {
            Skeletons.TryGetValue(userId, out var cs);
            return cs as CollaborativeSkeleton;
        }

        public IEnumerable<CollaborativeSkeleton> GetCollaborativeSkeletons()
        {
            return Skeletons.Values.Where(x => x is CollaborativeSkeleton).Cast<CollaborativeSkeleton>();
        }

        public ISkeleton TryGetSkeletonById(ulong userId)
        {
            if (Skeletons.ContainsKey(userId))
                return Skeletons[userId];
            return null;
        }

        #endregion Skeleton getters

        #region Tracking management

        public void UpdateFrames(List<UserTrackingFrameDto> frames)
        {
            foreach (var frame in frames)
                UpdateFrame(frame);
            computeCoroutine ??= routineService.AttachLateRoutine(ComputeCoroutine());
        }

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            if (!Skeletons.TryGetValue(frame.userId, out ISkeleton skeleton))
            {
                UMI3DLogger.LogWarning($"Skeleton of user {frame.userId} not found. Cannot apply skeleton frame update.", scope);
                return;
            }

            skeleton.UpdateFrame(frame);
        }

        private async void SendTrackingLoop()
        {
            if (sendTrackingLoopOnce)
                return;
            sendTrackingLoopOnce = true;
            while (ShouldSendTracking && collaborationClientServerService.status != StatusType.NONE)
            {
                if (TargetTrackingFPS > 0)
                {
                    var frame = personalSkeleton.GetFrame(option);
                    frame.userId = personalSkeleton.UserId;

                    if (frame != null && personalSkeleton.UserId != 0)
                        collaborationClientServerService.SendTracking(frame);

                    // UNDONE: camera properties are not sent
                    if (ShouldSendCameraProperties)
                        GetCameraProperty();

                    await UMI3DAsyncManager.Delay((int)(1000f / TargetTrackingFPS));
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

            while (ShouldSendTracking && BonesAsyncFPS.ContainsKey(boneType))
            {
                if (BonesAsyncFPS[boneType] > 0)
                {
                    if (personalSkeleton.TrackedSubskeleton.bones.ContainsKey(boneType))
                    {
                        var boneData = personalSkeleton.TrackedSubskeleton.GetController(boneType);

                        if (boneData != null)
                        {
                            boneData.userId = personalSkeleton.UserId;
                            collaborationClientServerService.SendTracking(boneData);
                        }
                    }

                    await UMI3DAsyncManager.Delay((int)(1000f / personalSkeleton.BonesAsyncFPS[boneType]));
                }
                else
                    await UMI3DAsyncManager.Yield();
            }
        }

        public void SetBoneFPSTarget(uint boneType, float newFPSTarget)
        {
            if (newFPSTarget != TargetTrackingFPS)
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
            // UNDONE: Implement
        }

        #endregion Tracking management

        #region Compute Final Skeleton

        private IEnumerator computeCoroutine;
        private TrackingOption option;

        private IEnumerator ComputeCoroutine()
        {
            while (Skeletons.Count > 1)
            {
                foreach (var skeletonPair in Skeletons)
                {
                    var skeleton = skeletonPair.Value;
                    if (skeleton.UserId != personalSkeleton.UserId)
                    {
                        skeleton.Compute();
                    }
                }
                yield return null;
            }
            routineService.DettachLateRoutine(computeCoroutine);
            computeCoroutine = null;
        }

        #endregion Compute Final Skeleton

        public UserCameraPropertiesDto GetCameraProperty()
        {
            return personalSkeleton.GetCameraDto();
        }

        #region Pose

        public void HandlePoseRequest(ApplyPoseDto playPoseDto)
        {
            PoseDto poseDto = PoseManager.Instance.GetPose(playPoseDto.userID, playPoseDto.indexInList);
            Skeletons.TryGetValue(playPoseDto.userID, out ISkeleton skeleton);

            if (playPoseDto.stopPose)
                skeleton.PoseSubskeleton.StopPose(new List<PoseDto> { poseDto });
            else
                skeleton.PoseSubskeleton.SetPose(false, new List<PoseDto> { poseDto }, true);
        }

        #endregion Pose
    }
}