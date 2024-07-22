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
using System.Threading;
using System.Threading.Tasks;
using umi3d.cdk.navigation;
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
    /// <summary>
    /// Skeleton manager that handles collaborative skeletons and personal skeleton in a collaborative context.
    /// </summary>
    public class CollaborationSkeletonsManager : Singleton<CollaborationSkeletonsManager>, ICollaborationSkeletonsManager
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration;

        #region Fields

        public INavigationDelegate navigation;

        /// <inheritdoc/>
        public virtual IReadOnlyDictionary<(ulong, ulong), ISkeleton> Skeletons => skeletons;
        protected Dictionary<(ulong, ulong), ISkeleton> skeletons = new();

        /// <inheritdoc/>
        public virtual IPersonalSkeleton PersonalSkeleton => personalSkeletonManager.PersonalSkeleton;

        /// <inheritdoc/>
        public Vector3 WorldSize => PersonalSkeleton.worldSize;

        /// <inheritdoc/>
        public virtual CollaborativeSkeletonsScene CollabSkeletonsScene => CollaborativeSkeletonsScene.Exists ? CollaborativeSkeletonsScene.Instance : null;

        /// <summary>
        /// Invoked when a <see cref="CollaborativeSkeleton"/> is created. Parameter is the corresponding user id.
        /// </summary>
        public event Action<ulong> CollaborativeSkeletonCreated;

        /// <inheritdoc/>
        public UMI3DSkeletonHierarchy StandardHierarchy
        {
            get
            {
                var userCaptureLoadingParameters = (IUMI3DUserCaptureLoadingParameters)collaborativeLoaderService.AbstractLoadingParameters;
                _standardHierarchy ??= new UMI3DSkeletonHierarchy(userCaptureLoadingParameters.SkeletonHierarchyDefinition, userCaptureLoadingParameters.SkeletonMusclesDefinition);
                return _standardHierarchy;
            }
        }

        private UMI3DSkeletonHierarchy _standardHierarchy;

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        public virtual bool ShouldSendTracking
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

        /// <inheritdoc/>
        public IReadOnlyDictionary<uint, float> BonesAsyncFPS => PersonalSkeleton.BonesAsyncFPS as IReadOnlyDictionary<uint, float>;

        /// <inheritdoc/>
        public virtual float TargetTrackingFPS { get; set; } = 30f;

        /// <inheritdoc/>
        public bool ShouldSendCameraProperties { get; set; } = true;
        private bool sendTrackingLoopOnce = false;

        #endregion Fields

        #region Dependency Injection

        private readonly IUMI3DCollaborationClientServer collaborationClientServerService;
        private readonly ILoadingManager collaborativeLoaderService;
        private readonly ICollaborationEnvironmentManager collaborativeEnvironmentManagementService;
        private readonly ILateRoutineService routineService;
        private readonly ISkeletonManager personalSkeletonManager;

        public CollaborationSkeletonsManager() : this(collaborationClientServer: UMI3DCollaborationClientServer.Instance,
                                                        collaborativeLoader: UMI3DCollaborationEnvironmentLoader.Instance,
                                                        collaborativeEnvironmentManagementService: UMI3DCollaborationEnvironmentLoader.Instance,
                                                        routineService: CoroutineManager.Instance,
                                                        personalSkeletonManager: PersonalSkeletonManager.Instance)
        {
        }

        public CollaborationSkeletonsManager(IUMI3DCollaborationClientServer collaborationClientServer,
                                            ILoadingManager collaborativeLoader,
                                            ICollaborationEnvironmentManager collaborativeEnvironmentManagementService,
                                            ISkeletonManager personalSkeletonManager,
                                            ILateRoutineService routineService) : base()
        {
            this.collaborationClientServerService = collaborationClientServer;
            this.collaborativeLoaderService = collaborativeLoader;
            this.personalSkeletonManager = personalSkeletonManager;
            this.routineService = routineService;
            this.collaborativeEnvironmentManagementService = collaborativeEnvironmentManagementService;
            Init();
        }

        #endregion Dependency Injection

        #region LifeCycle

        private bool canClearSkeletons = false;
        private bool canUpdateSkeletons = false;

        private void Init()
        {
            collaborativeEnvironmentManagementService.OnUpdateJoinnedUserList += () => UpdateSkeletons(collaborativeEnvironmentManagementService.UserList);
            collaborativeLoaderService.onEnvironmentLoaded.AddListener(() => { InitSkeletons(); if (ShouldSendTracking) SendTrackingLoop(); canClearSkeletons = true; canUpdateSkeletons = true; });
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
            {
                routineService.DetachLateRoutine(computeCoroutine);
                computeCoroutine = null;
            }
        }

        private void InitSkeletons()
        {
            PersonalSkeleton.UserId = collaborationClientServerService.GetUserId();
            PersonalSkeleton.EnvironmentId = UMI3DGlobalID.EnvironmentId;
            skeletons[(UMI3DGlobalID.EnvironmentId, PersonalSkeleton.UserId)] = PersonalSkeleton;
        }

        private void UpdateSkeletons(IEnumerable<UMI3DUser> users)
        {
            try
            {
                List<(ulong EnvironmentId, ulong id)> readyUserIdList = users.Where(u => u.status >= StatusType.READY).Select(u => (u.EnvironmentId, u.id)).ToList();
                readyUserIdList.Remove((UMI3DGlobalID.EnvironmentId, collaborationClientServerService.GetUserId()));

                List<(ulong EnvironmentId, ulong id)> joinedUsersId = readyUserIdList.Except(Skeletons.Keys).ToList();
                List<(ulong EnvironmentId, ulong id)> deletedUsersId = Skeletons.Keys.Except(readyUserIdList).ToList();

                foreach (var userId in deletedUsersId)
                    DestroySkeleton(userId);

                foreach (var userId in joinedUsersId)
                {
                    if (userId.EnvironmentId != default || userId.id != collaborationClientServerService.GetUserId())
                    {
                        CreateSkeleton(userId.EnvironmentId, userId.id, CollabSkeletonsScene.transform, StandardHierarchy);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public virtual CollaborativeSkeleton CreateSkeleton(ulong environmentId, ulong userId, Transform parent, UMI3DSkeletonHierarchy skeletonHierarchy)
        {
            GameObject go = new GameObject();
            CollaborativeSkeleton cs = go.AddComponent<CollaborativeSkeleton>();
            cs.UserId = userId;
            cs.EnvironmentId = environmentId;
            cs.name = $"skeleton_user_{environmentId}_{userId}";

            if (parent != null)
                cs.transform.SetParent(parent);
            else
                cs.transform.SetParent(CollabSkeletonsScene.transform);

            cs.transform.localScale = collaborativeEnvironmentManagementService.UserList.First(u => u.id == userId).userSize.Struct();

            cs.SkeletonHierarchy = skeletonHierarchy;

            var trackedSkeletonPrefab = (collaborativeLoaderService.AbstractLoadingParameters as IUMI3DCollabLoadingParameters).CollabTrackedSkeleton;
            var trackedSkeleton = UnityEngine.Object.Instantiate(trackedSkeletonPrefab, cs.transform).GetComponent<TrackedSubskeleton>();
            trackedSkeleton.EnvironmentId = environmentId;

            var poseSkeleton = new PoseSubskeleton(environmentId,
                                                                parentSkeleton: cs,
                                                                environmentManagerService: collaborativeEnvironmentManagementService);

            cs.Init(trackedSkeleton, poseSkeleton);

            skeletons[(environmentId, userId)] = cs;
            CollaborativeSkeletonCreated?.Invoke(userId);
            return cs;
        }

        /// <inheritdoc/>
        /// In the same way than EnvironmentLaoder.WaitForEntityToBeLoaded, risk of waiting infinitely.
        public virtual async Task<ISkeleton> WaitForSkeleton(ulong environmentId, ulong userId, List<CancellationToken> tokens = null)
        {
            if (Skeletons.TryGetValue((environmentId, userId), out ISkeleton skeleton))
                return skeleton;

            void WaitSkeletonCreation(ulong createdSkeletonUserId)
            {
                if (createdSkeletonUserId != userId)
                    return;

                if (Skeletons.TryGetValue((environmentId, userId), out ISkeleton c))
                {
                    skeleton = c;
                    CollaborativeSkeletonCreated -= WaitSkeletonCreation;
                }
            };
            CollaborativeSkeletonCreated += WaitSkeletonCreation;

            while (skeleton == null)
                await UMI3DAsyncManager.Yield(tokens);

            if (skeleton == null)
                UMI3DLogger.LogWarning($"Impossible to get skeleton of user ({environmentId},{userId}). Skeleton does not exist.", scope);

            return skeleton;
        }

        public virtual void DestroySkeleton((ulong environmentId, ulong userId) userIdentifier)
        {
            if (!Skeletons.TryGetValue(userIdentifier, out var skeleton) 
                || skeleton is not CollaborativeSkeleton collabSkeleton)
                return;

            UnityEngine.Object.Destroy(collabSkeleton.gameObject);
            skeletons.Remove(userIdentifier);
        }


        #endregion LifeCycle

        #region Skeleton getters

        public CollaborativeSkeleton GetCollaborativeSkeleton((ulong, ulong) userId)
        {
            Skeletons.TryGetValue(userId, out var cs);
            return cs as CollaborativeSkeleton;
        }

        public IEnumerable<CollaborativeSkeleton> GetCollaborativeSkeletons()
        {
            return Skeletons.Values.Where(x => x is CollaborativeSkeleton).Cast<CollaborativeSkeleton>();
        }

        public ISkeleton TryGetSkeletonById(ulong environmentId, ulong userId)
        {
            Skeletons.TryGetValue((environmentId, userId), out var cs);
            return cs;
        }

        #endregion Skeleton getters

        #region Tracking management

        public virtual void UpdateSkeleton(IEnumerable<UserTrackingFrameDto> frames)
        {
            if (Application.isBatchMode || !canUpdateSkeletons)
                return;

            if (frames is null)
                throw new ArgumentNullException(nameof(frames));

            if (!frames.Any())
                return;

            foreach (var frame in frames)
                UpdateSkeleton(frame);
        }

        public virtual void UpdateSkeleton(UserTrackingFrameDto frame)
        {
            if (frame is null)
                throw new ArgumentNullException(nameof(frame));

            if (!Skeletons.TryGetValue((frame.environmentId, frame.userId), out ISkeleton skeleton))
            {
                UMI3DLogger.LogWarning($"Skeleton of user {frame.userId} not found. Cannot apply skeleton frame update. [environmentId : {frame.environmentId}]", scope);
                return;
            }

            skeleton.UpdateBones(frame);
            computeCoroutine ??= routineService.AttachLateRoutine(ComputeCoroutine());
        }

        private async void SendTrackingLoop()
        {
            if (sendTrackingLoopOnce)
                return;
            sendTrackingLoopOnce = true;
            while (ShouldSendTracking && collaborationClientServerService.status != StatusType.NONE)
            {
                if (TargetTrackingFPS > 0 && !PersonalSkeletonManager.instance.IsPersonalSkeletonNull)
                {
                    var frame = PersonalSkeleton.GetFrame(option);
                    frame.userId = PersonalSkeleton.UserId;

                    var navigationData = navigation.GetNavigationData();

                    frame.speed = navigationData.speed;
                    frame.grounded = navigationData.grounded;
                    frame.jumping = navigationData.jumping;
                    frame.crouching = navigationData.crouching;

                    if (frame != null && PersonalSkeleton.UserId != 0)
                        collaborationClientServerService.SendTracking(frame);

                    // UNDONE: camera properties are not sent
                    if (ShouldSendCameraProperties)
                        PersonalSkeleton.GetCameraDto();

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

            while (ShouldSendTracking && PersonalSkeleton.BonesAsyncFPS.ContainsKey(boneType))
            {
                if (BonesAsyncFPS[boneType] > 0)
                {
                    if (PersonalSkeleton.TrackedSubskeleton.TrackedBones.ContainsKey(boneType))
                    {
                        var boneData = PersonalSkeleton.TrackedSubskeleton.GetController(boneType);

                        if (boneData != null)
                        {
                            boneData.userId = PersonalSkeleton.UserId;
                            collaborationClientServerService.SendTracking(boneData);
                        }
                    }

                    await UMI3DAsyncManager.Delay((int)(1000f / PersonalSkeleton.BonesAsyncFPS[boneType]));
                }
                else
                    await UMI3DAsyncManager.Yield();
            }
        }

        public virtual void SetBoneFPSTarget(uint boneType, float newFPSTarget)
        {
            if (newFPSTarget < 0)
                throw new ArgumentException("New period should not be negative.", nameof(newFPSTarget));

            if (newFPSTarget != TargetTrackingFPS)
            {
                PersonalSkeleton.BonesAsyncFPS[boneType] = newFPSTarget;
                SendAsyncBoneData(boneType);
            }
            else if (PersonalSkeleton.BonesAsyncFPS.ContainsKey(boneType))
                SyncBoneFPS(boneType);
        }

        public virtual void SyncBoneFPS(uint boneType)
        {
            if (PersonalSkeleton.BonesAsyncFPS.ContainsKey(boneType))
                PersonalSkeleton.BonesAsyncFPS.Remove(boneType);
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
                    if (skeleton.UserId != PersonalSkeleton.UserId)
                    {
                        skeleton.Compute();
                    }
                }
                yield return null;
            }
            routineService.DetachLateRoutine(computeCoroutine);
            computeCoroutine = null;
        }

        #endregion Compute Final Skeleton

        #region Pose

        public virtual void ApplyPoseRequest(ulong environmentId, PlayPoseClipDto playPoseDto)
        {
            if (!Skeletons.TryGetValue((environmentId, collaborationClientServerService.GetUserId()), out ISkeleton skeleton))
            {
                UMI3DLogger.LogWarning($"Cannot apply pose request for own user. Skeleton not found.", scope);
                return;
            }

            if (!collaborativeEnvironmentManagementService.TryGetEntity(environmentId, playPoseDto.poseId, out PoseClip pose))
            {
                UMI3DLogger.LogWarning($"Cannot apply pose request for own user. Pose {playPoseDto.poseId} not found.", scope);
                return;
            }

            if (playPoseDto.stopPose)
                skeleton.PoseSubskeleton.StopPose(pose);
            else
                skeleton.PoseSubskeleton.StartPose(pose, 
                                                   isOverriding: false,
                                                   parameters: playPoseDto.transitionDuration < 0 ? null : new() 
                                                   { 
                                                       startTransitionDuration = playPoseDto.transitionDuration, 
                                                       endTransitionDuration = playPoseDto.transitionDuration
                                                   });
        }

        #endregion Pose
    }
}