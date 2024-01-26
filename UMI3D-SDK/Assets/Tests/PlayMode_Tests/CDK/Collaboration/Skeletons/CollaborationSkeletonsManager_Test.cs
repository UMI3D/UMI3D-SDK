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
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TestUtils.UserCapture;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.collaboration.userCapture;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.Collaboration.UserCapture.CDK
{
    [TestFixture, TestOf(typeof(CollaborationSkeletonsManager))]
    public class CollaborationSkeletonsManager_Test
    {
        private CollaborationSkeletonsManager collaborativeSkeletonManager;
        private GameObject skeletonGo;
        private GameObject trackedSubskeletonGo;
        private Mock<IUMI3DCollaborationClientServer> collaborationClientServerServiceMock;
        private Mock<ILoadingManager> collaborativeLoaderServiceMock;
        private Mock<ICollaborationEnvironmentManager> collaborativeEnvironmentManagementServiceMock;
        private Mock<IPoseManager> poseManagerMock;
        private Mock<ILateRoutineService> routineServiceMock;
        private Mock<ISkeletonManager> personalSkeletonManagerMock;
        private GameObject hipsTracked;
        private Mock<CollaborationSkeletonsManager> collaborativeSkeletonManagerMock;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);

            _ = CollaborativeSkeletonsScene.Instance;
        }

        [SetUp]
        public void SetUp()
        {
            collaborationClientServerServiceMock = new();
            collaborativeLoaderServiceMock = new();
            collaborativeEnvironmentManagementServiceMock = new();
            routineServiceMock = new();
            personalSkeletonManagerMock = new();

            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(new List<UMI3DUser>());
            collaborativeLoaderServiceMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEvent());
            collaborationClientServerServiceMock.Setup(x => x.OnLeavingEnvironment).Returns(new UnityEvent());
            collaborationClientServerServiceMock.Setup(x => x.OnRedirection).Returns(new UnityEvent());

            collaborativeSkeletonManagerMock = new Mock<CollaborationSkeletonsManager>(collaborationClientServerServiceMock.Object,
                                                                            collaborativeLoaderServiceMock.Object,
                                                                            collaborativeEnvironmentManagementServiceMock.Object,
                                                                            personalSkeletonManagerMock.Object,
                                                                            routineServiceMock.Object);
            collaborativeSkeletonManagerMock.CallBase = true;
            collaborativeSkeletonManager = collaborativeSkeletonManagerMock.Object;

            skeletonGo = new GameObject("Skeleton created");
            skeletonGo.AddComponent<CollaborativeSkeleton>();
            trackedSubskeletonGo = new GameObject("Tracked subskeleton");
            trackedSubskeletonGo.transform.SetParent(skeletonGo.transform);
            var trackedSubskeleton = trackedSubskeletonGo.AddComponent<TrackedSubskeleton>();
            hipsTracked = new GameObject("Hips tracked");
            hipsTracked.transform.SetParent(trackedSubskeletonGo.transform);

            trackedSubskeleton.hips = hipsTracked.transform;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.Destroy(hipsTracked);
            UnityEngine.Object.Destroy(trackedSubskeletonGo);
            UnityEngine.Object.Destroy(skeletonGo);

            CollaborationSkeletonsManager.Destroy();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            CollaborativeSkeletonsScene.Destroy();
        }

        #endregion Test SetUp

        #region Create Skeleton

        [Test]
        public void CreateSkeleton()
        {
            // GIVEN
            ulong userId = 1005uL;
            UserDto userDto = new UserDto() { id = userId };
            UMI3DUser user = new UMI3DUser(UMI3DGlobalID.EnvironmentId, userDto);
            var userList = new List<UMI3DUser>() { user };

            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.AbstractLoadingParameters).Returns(parameterMock.Object);
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(userList);

            // WHEN
            var skeleton = collaborativeSkeletonManager.CreateSkeleton(UMI3DGlobalID.EnvironmentId, userId, skeletonGo.transform, hierarchy);

            // THEN
            Assert.IsNotNull(skeleton);
            Assert.AreEqual(userId, skeleton.UserId);

            Assert.Greater(skeleton.Bones.Count, 0);
            Assert.IsNotNull(skeleton.SkeletonHierarchy);
            Assert.IsNotNull(skeleton.HipsAnchor);
            Assert.IsNotNull(skeleton.transform.parent);

            Assert.IsNotNull(skeleton.PoseSubskeleton);
            Assert.IsNotNull(skeleton.TrackedSubskeleton);
        }

        [Test]
        public void CreateSkeleton_NullParent()
        {
            // GIVEN
            ulong userId = 1005uL;
            UserDto userDto = new UserDto() { id = userId };
            UMI3DUser user = new UMI3DUser(UMI3DGlobalID.EnvironmentId, userDto);
            var userList = new List<UMI3DUser>() { user };


            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.AbstractLoadingParameters).Returns(parameterMock.Object);
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(userList);

            // WHEN
            var skeleton = collaborativeSkeletonManager.CreateSkeleton(UMI3DGlobalID.EnvironmentId, userId, null, hierarchy);

            // THEN
            Assert.IsNotNull(skeleton);
            Assert.AreEqual(userId, skeleton.UserId);

            Assert.Greater(skeleton.Bones.Count, 0);
            Assert.IsNotNull(skeleton.SkeletonHierarchy);
            Assert.IsNotNull(skeleton.HipsAnchor);
            Assert.IsNotNull(skeleton.transform.parent);

            Assert.IsNotNull(skeleton.PoseSubskeleton);
            Assert.IsNotNull(skeleton.TrackedSubskeleton);
        }

        [Test]
        public void CreateSkeleton_NullHierarchy()
        {
            // GIVEN
            ulong userId = 1005uL;
            UserDto userDto = new UserDto() { id = userId };
            UMI3DUser user = new UMI3DUser(UMI3DGlobalID.EnvironmentId, userDto);
            var userList = new List<UMI3DUser>() { user };

            GameObject go = new GameObject("Skeleton created");
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(userList);
            // WHEN
            Action action = () => { _ = collaborativeSkeletonManager.CreateSkeleton(UMI3DGlobalID.EnvironmentId, userId, go.transform, null); };

            // THEN
            Assert.Throws<System.ArgumentNullException>(() => action());

            // tear down
            UnityEngine.Object.Destroy(go);
        }

        #endregion Create Skeleton

        #region GetCollaborativeSkeletons

        [Test]
        public void GetCollaborativeSkeletons()
        {
            // given
            ulong userId = 10050uL;
            UserDto userDto = new UserDto()
            {
                id = userId,
                userSize = new umi3d.common.Vector3Dto()
            };
            UMI3DUser user = new UMI3DUser(UMI3DGlobalID.EnvironmentId, userDto);
            var userList = new List<UMI3DUser>() { user };


            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.AbstractLoadingParameters).Returns(parameterMock.Object);
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(userList);

            GameObject testSkeletonGo = new GameObject("Test collaborative skeleton");
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            _ = collaborativeSkeletonManager.CreateSkeleton(UMI3DGlobalID.EnvironmentId, userId, testSkeletonGo.transform, hierarchy);

            // when
            CollaborativeSkeleton resultSkeleton = collaborativeSkeletonManager.GetCollaborativeSkeleton((UMI3DGlobalID.EnvironmentId,userId));

            // then
            Assert.AreEqual(resultSkeleton.UserId, userId);
        }

        [Test]
        public void GetCollaborativeSkeletons_NoSkeletonFound()
        {
            // given
            ulong userId = 10050uL;
            UserDto userDto = new UserDto()
            {
                id = userId,
                userSize = new umi3d.common.Vector3Dto()
            };
            UMI3DUser user = new UMI3DUser(UMI3DGlobalID.EnvironmentId, userDto);
            var userList = new List<UMI3DUser>() { user };

            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.AbstractLoadingParameters).Returns(parameterMock.Object);
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(userList);

            GameObject testSkeletonGo = new GameObject("Test collaborative skeleton");
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            _ = collaborativeSkeletonManager.CreateSkeleton(UMI3DGlobalID.EnvironmentId, userId, testSkeletonGo.transform, hierarchy);

            // when
            CollaborativeSkeleton resultSkeleton = collaborativeSkeletonManager.GetCollaborativeSkeleton((UMI3DGlobalID.EnvironmentId, userId + 1));

            // then
            Assert.IsNull(resultSkeleton);
        }

        #endregion GetCollaborativeSkeletons

        #region TryGetSkeletonById

        [Test]
        public void TryGetSkeletonById()
        {
            // given
            ulong userId = 10050uL;
            UserDto userDto = new UserDto()
            {
                id = userId,
                userSize = new umi3d.common.Vector3Dto()
            };
            UMI3DUser user = new UMI3DUser(UMI3DGlobalID.EnvironmentId, userDto);
            var userList = new List<UMI3DUser>() { user };

            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.AbstractLoadingParameters).Returns(parameterMock.Object);
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(userList);

            GameObject testSkeletonGo = new GameObject("Test collaborative skeleton");
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            _ = collaborativeSkeletonManager.CreateSkeleton(UMI3DGlobalID.EnvironmentId, userId, testSkeletonGo.transform, hierarchy);

            // when
            ISkeleton resultSkeleton = collaborativeSkeletonManager.TryGetSkeletonById(UMI3DGlobalID.EnvironmentId, userId + 1);

            // then
            Assert.IsNull(resultSkeleton);
        }

        [Test]
        public void TryGetSkeletonById_NoSkeletonFound()
        {
            // given
            ulong userId = 10050uL;
            UserDto userDto = new UserDto()
            {
                id = userId,
                userSize = new umi3d.common.Vector3Dto()
            };
            UMI3DUser user = new UMI3DUser(UMI3DGlobalID.EnvironmentId, userDto);
            var userList = new List<UMI3DUser>() { user };

            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.AbstractLoadingParameters).Returns(parameterMock.Object);
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(userList);

            GameObject testSkeletonGo = new GameObject("Test collaborative skeleton");
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            _ = collaborativeSkeletonManager.CreateSkeleton(UMI3DGlobalID.EnvironmentId, userId, testSkeletonGo.transform, hierarchy);

            // when
            CollaborativeSkeleton resultSkeleton = collaborativeSkeletonManager.GetCollaborativeSkeleton((UMI3DGlobalID.EnvironmentId, userId + 1));

            // then
            Assert.IsNull(resultSkeleton);
        }

        #endregion TryGetSkeletonById

        #region GetFrame

        [Test]
        public void UpdateSkeleton_NoFrame()
        {
            // given
            List<UserTrackingFrameDto> frames = new();
            var mockRoutine = new Mock<IEnumerator>();
            routineServiceMock.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false)).Returns(mockRoutine.Object);

            // when
            collaborativeSkeletonManager.UpdateSkeleton(frames);

            // then
            routineServiceMock.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false), Times.Never);
        }

        [Test]
        public void UpdateSkeleton_Null()
        {
            // given
            List<UserTrackingFrameDto> frames = null;
            var mockRoutine = new Mock<IEnumerator>();
            routineServiceMock.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false)).Returns(mockRoutine.Object);

            // when
            System.Action action = () => collaborativeSkeletonManager.UpdateSkeleton(frames);

            // then
            routineServiceMock.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false), Times.Never);
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void UpdateSkeleton_SkeletonNotFound()
        {
            // given
            UserTrackingFrameDto frame1 = new UserTrackingFrameDto()
            {
                userId = 1005uL,
            };
            UserTrackingFrameDto frame2 = new UserTrackingFrameDto();

            List<UserTrackingFrameDto> frames = new()
            {
                frame1,
            };
            var mockRoutine = new Mock<IEnumerator>();
            routineServiceMock.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false)).Returns(mockRoutine.Object);

            // when
            collaborativeSkeletonManager.UpdateSkeleton(frames);

            // then
            routineServiceMock.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false), Times.Never);
        }

        [Test]
        public void UpdateSkeleton()
        {
            // given
            UserTrackingFrameDto frame1 = new UserTrackingFrameDto()
            {
                userId = 1005uL,
                environmentId = UMI3DGlobalID.EnvironmentId
            };
            UserTrackingFrameDto frame2 = new UserTrackingFrameDto()
            {
                userId = 1006uL,
                environmentId = UMI3DGlobalID.EnvironmentId
            };

            List<UserTrackingFrameDto> frames = new()
            {
                frame1, frame2
            };

            Mock<ISkeleton> skeletonMock1 = new Mock<ISkeleton>();
            Mock<ISkeleton> skeletonMock2 = new Mock<ISkeleton>();
            skeletonMock1.Setup(x => x.UpdateBones(frame1));
            skeletonMock2.Setup(x => x.UpdateBones(frame2));

            Dictionary<(ulong,ulong), ISkeleton> dictSkeletons = new()
            {
                { (UMI3DGlobalID.EnvironmentId,frame1.userId), skeletonMock1.Object},
                { (UMI3DGlobalID.EnvironmentId, frame2.userId), skeletonMock2.Object },
            };

            var mockRoutine = new Mock<IEnumerator>();
            routineServiceMock.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false)).Returns(mockRoutine.Object);

            collaborativeSkeletonManagerMock.CallBase = false;
            collaborativeSkeletonManagerMock.Setup(x => x.UpdateSkeleton(frames)).CallBase();
            collaborativeSkeletonManagerMock.Setup(x => x.UpdateSkeleton(It.IsAny<UserTrackingFrameDto>())).CallBase();
            collaborativeSkeletonManagerMock.Setup(x => x.Skeletons).Returns(dictSkeletons);

            // when
            collaborativeSkeletonManager.UpdateSkeleton(frames);

            // then
            routineServiceMock.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false), Times.Once);
            skeletonMock1.Verify(x => x.UpdateBones(frame1), Times.Once);
            skeletonMock2.Verify(x => x.UpdateBones(frame2), Times.Once);
        }

        #endregion GetFrame

        #region SetBoneFPSTarget

        [Test]
        public void SetBoneFPSTarget()
        {
            // given
            uint boneType = BoneType.Hips;
            float FPSTarget = 1.0f;

            collaborativeSkeletonManagerMock.CallBase = false;
            collaborativeSkeletonManagerMock.Setup(x => x.SetBoneFPSTarget(boneType, FPSTarget)).CallBase();
            collaborativeSkeletonManagerMock.Setup(x => x.TargetTrackingFPS).Returns(2f);
            collaborativeSkeletonManagerMock.Setup(x => x.ShouldSendTracking).Returns(false);

            var boneFPSDict = new Dictionary<uint, float>() { { BoneType.Hips, 3.0f } };

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            personalSkeletonMock.Setup(x => x.BonesAsyncFPS).Returns(boneFPSDict);

            personalSkeletonManagerMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            collaborativeSkeletonManagerMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);

            // when
            collaborativeSkeletonManager.SetBoneFPSTarget(boneType, FPSTarget);

            // then
            Assert.IsTrue(boneFPSDict.ContainsKey(boneType));
            Assert.AreEqual(FPSTarget, boneFPSDict[boneType]);
        }

        [Test]
        public void SetBoneFPSTarget_SameFPSTarget()
        {
            // given
            uint boneType = BoneType.Hips;
            float FPSTarget = 2.0f;

            collaborativeSkeletonManagerMock.CallBase = false;
            collaborativeSkeletonManagerMock.Setup(x => x.SetBoneFPSTarget(boneType, FPSTarget)).CallBase();

            collaborativeSkeletonManagerMock.Setup(x => x.TargetTrackingFPS).Returns(2f);
            collaborativeSkeletonManagerMock.Setup(x => x.SyncBoneFPS(boneType));

            var boneFPSDict = new Dictionary<uint, float>() { { BoneType.Hips, 3.0f } };

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            personalSkeletonMock.Setup(x => x.BonesAsyncFPS).Returns(boneFPSDict);
            personalSkeletonManagerMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            collaborativeSkeletonManagerMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);

            // when
            collaborativeSkeletonManager.SetBoneFPSTarget(boneType, FPSTarget);

            // then
            collaborativeSkeletonManagerMock.Verify(x => x.SyncBoneFPS(boneType), Times.Once());
        }

        [Test]
        public void SetBoneFPSTarget_NegativeFPSTarget()
        {
            // given
            uint boneType = BoneType.Hips;
            float FPSTarget = -1.0f;
            collaborativeSkeletonManagerMock.Setup(x => x.SetBoneFPSTarget(boneType, FPSTarget)).CallBase();

            // when
            Action action = () => collaborativeSkeletonManager.SetBoneFPSTarget(boneType, FPSTarget);

            // then
            Assert.Throws<ArgumentException>(() => action());
        }

        #endregion SetBoneFPSTarget

        #region SyncBoneFPS

        [Test]
        public void SyncBoneFPS()
        {
            // given
            uint boneType = BoneType.Hips;

            collaborativeSkeletonManagerMock.CallBase = false;
            collaborativeSkeletonManagerMock.Setup(x => x.SyncBoneFPS(boneType)).CallBase();

            var boneFPSDict = new Dictionary<uint, float>() { { BoneType.Hips, 3.0f } };

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            personalSkeletonMock.Setup(x => x.BonesAsyncFPS).Returns(boneFPSDict);
            personalSkeletonManagerMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            collaborativeSkeletonManagerMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);

            // when
            collaborativeSkeletonManager.SyncBoneFPS(boneType);

            // then
            Assert.IsFalse(boneFPSDict.ContainsKey(boneType));
        }

        #endregion SyncBoneFPS

        #region ApplyPoseRequest

        [Test, TestOf(nameof(CollaborationSkeletonsManager.ApplyPoseRequest))]
        public void ApplyPoseRequest_Start()
        {
            // given
            ulong userId = 1005uL;
            var poses = new List<PoseClip>(2) { new PoseClip(null), new PoseClip(null) };

            collaborativeSkeletonManagerMock.CallBase = false;
            collaborativeSkeletonManagerMock.Setup(x => x.ApplyPoseRequest(UMI3DGlobalID.EnvironmentId,It.IsAny<PlayPoseClipDto>())).CallBase();


            var poseSkeletonMock = new Mock<IPoseSubskeleton>();
            poseSkeletonMock.Setup(x => x.StopPose(It.IsAny<PoseClip>()));

            var skeletonMock = new Mock<ISkeleton>();
            skeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSkeletonMock.Object);

            var skeletons = new Dictionary<(ulong, ulong), ISkeleton>(1) { { (UMI3DGlobalID.EnvironmentId, userId), skeletonMock.Object } };
            collaborativeSkeletonManagerMock.Setup(x => x.Skeletons).Returns(skeletons);
            ulong poseId = 1uL;
            var playPoseDto = new PlayPoseClipDto()
            {
                userID = userId,
                poseId = poseId,
                stopPose = false
            };

            PoseClip poseClip = poses[0];
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.TryGetEntity(UMI3DGlobalID.EnvironmentId, poseId, out poseClip));

            // when
            collaborativeSkeletonManager.ApplyPoseRequest(UMI3DGlobalID.EnvironmentId,playPoseDto);

            // then
            poseSkeletonMock.Setup(x => x.StartPose(It.IsAny<PoseClip>(), It.IsAny<bool>()));
        }

        [Test]
        public void ApplyPoseRequest_Stop()
        {
            // given
            ulong userId = 1005uL;
            var poses = new List<PoseClip>(2) { new PoseClip(null), new PoseClip(null) };

            collaborativeSkeletonManagerMock.CallBase = false;
            collaborativeSkeletonManagerMock.Setup(x => x.ApplyPoseRequest(UMI3DGlobalID.EnvironmentId, It.IsAny<PlayPoseClipDto>())).CallBase();

            var poseSkeletonMock = new Mock<IPoseSubskeleton>();
            poseSkeletonMock.Setup(x => x.StopPose(It.IsAny<PoseClip>()));

            var skeletonMock = new Mock<ISkeleton>();
            skeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSkeletonMock.Object);

            var skeletons = new Dictionary<(ulong,ulong), ISkeleton>(1) { { (UMI3DGlobalID.EnvironmentId, userId), skeletonMock.Object } };
            collaborativeSkeletonManagerMock.Setup(x => x.Skeletons).Returns(skeletons);
            ulong poseId = 1uL;
            var playPoseDto = new PlayPoseClipDto()
            {
                userID = userId,
                poseId = poseId,
                stopPose = true
            };

            PoseClip poseClip = poses[0];
            collaborativeEnvironmentManagementServiceMock.Setup(x => x.TryGetEntity(UMI3DGlobalID.EnvironmentId, poseId, out poseClip));

            // when
            collaborativeSkeletonManager.ApplyPoseRequest(UMI3DGlobalID.EnvironmentId, playPoseDto);

            // then
            poseSkeletonMock.Setup(x => x.StopPose(It.IsAny<PoseClip>()));
        }

        #endregion ApplyPoseRequest
    }
}