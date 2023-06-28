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

using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture.pose;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    public class PoseOverriderHandlerUnit_Tests
    {
        private PoseOverriderContainerHandlerUnit unit = null;

        private Mock<UMI3DEnvironmentLoader> environmentLoaderServiceMock;

        private Mock<UMI3DCollaborationClientServer> collaborationClientServerMock;

        private Mock<TrackedSkeleton> TrackedSkeletonMock;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ClearSingletons();
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            environmentLoaderServiceMock = new Mock<UMI3DEnvironmentLoader>();
            collaborationClientServerMock = new Mock<UMI3DCollaborationClientServer>();
            TrackedSkeletonMock = new Mock<TrackedSkeleton>();

            unit = new PoseOverriderContainerHandlerUnit(environmentLoaderServiceMock.Object, TrackedSkeletonMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            unit = null;

            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();

            if (UMI3DCollaborationClientServer.Exists)
                UMI3DCollaborationClientServer.Destroy();

            if (UMI3DLoadingHandler.Exists)
                Object.Destroy(UMI3DLoadingHandler.Instance);
        }

        [Test]
        public void TestNullData()
        {
            //Given
            UMI3DPoseOverriderContainerDto container = null;

            TrackedSkeletonMock.Setup(x => x.GetBonePosition(13)).Returns(new Vector3(15, 15, 0));
            UMI3DNodeInstance i = new UMI3DNodeInstance(() => Debug.Log("loaded"));
            i.gameObject = new GameObject("hey");
            environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
            environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

            //When
            Assert.IsFalse(unit.SetPoseOverriderContainer(container));

            Assert.IsFalse(unit.CheckTriggerConditions());
        }

        /// <summary>
        /// test that we can handle an empty list of sub skeleton i the compute méthod
        /// </summary>
        [Test]
        public void TestPoseOverriderSorting()
        {
            //Given
            UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

            //When
            unit.SetPoseOverriderContainer(container);

            //Then
            Assert.IsTrue(unit.GetEnvironmentalPoseOverriders().Count == 1);
            Assert.IsTrue(unit.GetNonEnvironmentalPoseOverriders().Count == 2);
        }

        [Test]
        public void TestOnTrigger_FalseCondtion()
        {
            //Given
            UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

            TrackedSkeletonMock.Setup(x => x.GetBonePosition(13)).Returns(new Vector3(15, 15, 0));
            UMI3DNodeInstance i = new UMI3DNodeInstance(() => { });
            i.gameObject = new GameObject("UMI3D Node");
            environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
            environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

            //When
            unit.SetPoseOverriderContainer(container);

            Assert.IsFalse(unit.CheckTriggerConditions());
        }

        [Test]
        public void TestOnTrigger_TrueCondtion()
        {
            // Given
            UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

            TrackedSkeletonMock.Setup(x => x.GetBonePosition(13)).Returns(new Vector3(0, 0, 0));

            UMI3DNodeInstance i = new UMI3DNodeInstance(() => { });
            i.gameObject = new GameObject("UMI3D Node");
            i.gameObject.transform.position = Vector3.zero;

            environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
            environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

            // When
            var result = unit.SetPoseOverriderContainer(container);

            // Then
            Assert.IsTrue(result, "Object was null.");
            Assert.IsTrue(unit.CheckTriggerConditions());
        }

        #region HelperMethod

        private UMI3DPoseOverriderContainerDto GenerateASimplePoseContainer()
        {
            UMI3DPoseOverriderContainerDto container = new UMI3DPoseOverriderContainerDto()
            {
                poseOverriderDtos = new List<PoseOverriderDto>()
            {
                new PoseOverriderDto()
                {
                    poseIndexinPoseManager = 0,
                    isHoverEnter = true,
                    poseConditions = new List<PoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            Magnitude = 1,
                            BoneOrigine = 13,
                            TargetObjectId = 100012
                        }
                    }.ToArray()
                },
                new PoseOverriderDto()
                {
                    poseIndexinPoseManager = 0,
                    isTrigger = true,
                    poseConditions = new List<PoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            Magnitude = 1,
                            BoneOrigine = 13,
                            TargetObjectId = 100012
                        }
                    }.ToArray()
                },
                new PoseOverriderDto()
                {
                    poseIndexinPoseManager = 0,
                    poseConditions = new List<PoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            Magnitude = 1,
                            BoneOrigine = 13,
                            TargetObjectId = 100012
                        }
                    }.ToArray()
                },
            }.ToArray(),
            };

            return container;
        }

        #endregion HelperMethod
    }
}