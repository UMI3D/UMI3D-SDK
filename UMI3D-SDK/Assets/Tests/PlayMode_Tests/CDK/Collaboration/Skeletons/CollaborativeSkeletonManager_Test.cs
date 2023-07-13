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
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.collaboration.userCapture;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.Collaboration.UserCapture.CDK
{
    [TestFixture, TestOf(typeof(CollaborativeSkeletonManager))]
    public class CollaborativeSkeletonManager_Test
    {
        private CollaborativeSkeletonManager collaborativeSkeletonManager;
        private GameObject skeletonGo;
        private GameObject trackedSubskeletonGo;
        private Mock<IUMI3DCollaborationClientServer> collaborationClientServerServiceMock;
        private Mock<ILoadingManager> collaborativeLoaderServiceMock;
        private Mock<ICollaborationEnvironmentManager> collaborativeEnvironmentManagementServiceMock;
        private Mock<IPoseManager> poseManagerMock;
        private Mock<ILateRoutineService> routineServiceMock;
        private Mock<ISkeletonManager> personalSkeletonManagerMock;
        private GameObject hipsTracked;

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
            poseManagerMock = new();
            routineServiceMock = new();
            personalSkeletonManagerMock = new();

            collaborativeEnvironmentManagementServiceMock.Setup(x => x.UserList).Returns(new List<UMI3DUser>());
            collaborativeLoaderServiceMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEvent());
            collaborationClientServerServiceMock.Setup(x => x.OnLeavingEnvironment).Returns(new UnityEvent());
            collaborationClientServerServiceMock.Setup(x => x.OnRedirection).Returns(new UnityEvent());

            collaborativeSkeletonManager = new CollaborativeSkeletonManager(collaborationClientServerServiceMock.Object,
                                                                            collaborativeLoaderServiceMock.Object,
                                                                            collaborativeEnvironmentManagementServiceMock.Object,
                                                                            poseManagerMock.Object,
                                                                            personalSkeletonManagerMock.Object,
                                                                            routineServiceMock.Object);

            skeletonGo = new GameObject("Skeleton created");
            trackedSubskeletonGo = new GameObject("Tracked subskeleton");
            var trackedSubskeleton = trackedSubskeletonGo.AddComponent<TrackedSkeleton>();
            hipsTracked = new GameObject("Hips tracked");
            hipsTracked.transform.SetParent(trackedSubskeletonGo.transform);
            trackedSubskeleton.Hips = hipsTracked.transform;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.Destroy(hipsTracked);
            UnityEngine.Object.Destroy(trackedSubskeletonGo);
            UnityEngine.Object.Destroy(skeletonGo);

            CollaborativeSkeletonManager.Destroy();
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

            UMI3DSkeletonHierarchy hierarchy = new UMI3DSkeletonHierarchy(null);

            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.LoadingParameters).Returns(parameterMock.Object);

            // WHEN
            var skeleton = collaborativeSkeletonManager.CreateSkeleton(userId, skeletonGo.transform, hierarchy);

            // THEN
            Assert.IsNotNull(skeleton);
            Assert.AreEqual(userId, skeleton.UserId);

            Assert.Greater(skeleton.Bones.Count, 0);
            Assert.IsNotNull(skeleton.SkeletonHierarchy);
            Assert.IsNotNull(skeleton.HipsAnchor);
            Assert.IsNotNull(skeleton.transform.parent);

            Assert.IsNotNull(skeleton.PoseSkeleton);
            Assert.IsNotNull(skeleton.TrackedSkeleton);
        }

        [Test]
        public void CreateSkeleton_NullParent()
        {
            // GIVEN
            ulong userId = 1005uL;

            UMI3DSkeletonHierarchy hierarchy = new UMI3DSkeletonHierarchy(null);

            var parameterMock = new Mock<IUMI3DCollabLoadingParameters>();
            parameterMock.Setup(x => x.CollabTrackedSkeleton).Returns(trackedSubskeletonGo);
            collaborativeLoaderServiceMock.Setup(x => x.LoadingParameters).Returns(parameterMock.Object);

            // WHEN
            var skeleton = collaborativeSkeletonManager.CreateSkeleton(userId, null, hierarchy);

            // THEN
            Assert.IsNotNull(skeleton);
            Assert.AreEqual(userId, skeleton.UserId);

            Assert.Greater(skeleton.Bones.Count, 0);
            Assert.IsNotNull(skeleton.SkeletonHierarchy);
            Assert.IsNotNull(skeleton.HipsAnchor);
            Assert.IsNotNull(skeleton.transform.parent);

            Assert.IsNotNull(skeleton.PoseSkeleton);
            Assert.IsNotNull(skeleton.TrackedSkeleton);
        }

        [Test]
        public void CreateSkeleton_NullHierarchy()
        {
            // GIVEN
            ulong userId = 1005uL;
            GameObject go = new GameObject("Skeleton created");

            // WHEN
            Action action = () => { _ = collaborativeSkeletonManager.CreateSkeleton(userId, go.transform, null); };

            // THEN
            Assert.Throws<System.ArgumentNullException>(() => action());

            // tear down
            UnityEngine.Object.Destroy(go);
        }

        #endregion Create Skeleton



        // GetCollaborativeSkeletons

        // TryGetSkeletonById

        // Get frame (a remonter ?)

        // update frame

        // get camera property

        // personal skeleton tracker ?
    }
}