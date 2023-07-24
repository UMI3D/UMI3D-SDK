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
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.pose;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    public class PoseOverriderHandlerUnit_Tests
    {
        private PoseConditionProcessor unit;

        private Mock<IEnvironmentManager> environmentLoaderServiceMock;
        private Mock<IUMI3DCollaborationClientServer> collaborationClientServerMock;
        private TrackedSubskeleton trackedSkeleton;
        private TrackedSubskeletonBone trackedSkeletonBone;
        private GameObject skeletonGo;

        #region Test Setup

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ClearSingletons();
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);

            skeletonGo = new GameObject("Skeleton");

            trackedSkeleton = skeletonGo.AddComponent<TrackedSubskeleton>();
            trackedSkeletonBone = new GameObject("Tracked skeleton bone").AddComponent<TrackedSubskeletonBone>();
            trackedSkeletonBone.boneType = BoneType.Chest;
            trackedSkeleton.bones = new() { { BoneType.Chest, trackedSkeletonBone } };
        }

        [SetUp]
        public void SetUp()
        {
            unit = new PoseConditionProcessor(null);
        }

        [TearDown]
        public void TearDown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();

            if (UMI3DCollaborationClientServer.Exists)
                UMI3DCollaborationClientServer.Destroy();
        }

        #endregion Test Setup

        // TODO: Update UT
        //[Test]
        //public void SetPoseOverriderContainer_Null()
        //{
        //    //Given
        //    UMI3DNodeInstance i = new UMI3DNodeInstance(() => { });
        //    i.gameObject = new GameObject("UMI3D Node");
        //    environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
        //    environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

        //    //When
        //    var result = unit.SetPoseOverriderContainer(null);

        //    //When
        //    Assert.IsFalse(result);
        //    Assert.IsFalse(unit.CheckTriggerConditions());

        //    // teardown
        //    UnityEngine.Object.Destroy(i.gameObject);
        //}

        ///// <summary>
        ///// test that we can handle an empty list of sub skeleton i the compute méthod
        ///// </summary>
        //[Test]
        //public void SetPoseOverriderContainer_Sorting()
        //{
        //    //Given
        //    UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

        //    //When
        //    unit.SetPoseOverriderContainer(container);

        //    //Then
        //    Assert.IsTrue(unit.GetEnvironmentalPoseOverriders().Count == 1);
        //    Assert.IsTrue(unit.GetNonEnvironmentalPoseOverriders().Count == 2);
        //}

        //[Test]
        //public void SetPoseOverriderContainer_FalseCondition()
        //{
        //    //Given
        //    UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

        //    UMI3DNodeInstance i = new UMI3DNodeInstance(() => { });
        //    i.gameObject = new GameObject("UMI3D Node");
        //    i.gameObject.transform.position = new Vector3(15,15,0);
        //    environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
        //    environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

        //    //When
        //    unit.SetPoseOverriderContainer(container);

        //    // THEN
        //    Assert.IsFalse(unit.CheckTriggerConditions());

        //    // teardown
        //    UnityEngine.Object.Destroy(i.gameObject);
        //}

        //[Test]
        //public void SetPoseOverriderContainer_TrueCondition()
        //{
        //    // Given
        //    UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

        //    UMI3DNodeInstance i = new UMI3DNodeInstance(() => { });
        //    i.gameObject = new GameObject("UMI3D Node");
        //    i.gameObject.transform.position = Vector3.zero;

        //    environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
        //    environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

        //    // When
        //    var result = unit.SetPoseOverriderContainer(container);

        //    // Then
        //    Assert.IsTrue(result, "Object was null.");
        //    Assert.IsTrue(unit.CheckTriggerConditions());

        //    // teardown
        //    UnityEngine.Object.Destroy(i.gameObject);
        //}

        #region HelperMethod

        private UMI3DPoseOverriderContainerDto GenerateASimplePoseContainer()
        {
            UMI3DPoseOverriderContainerDto container = new UMI3DPoseOverriderContainerDto()
            {
                poseOverriderDtos = new List<PoseOverriderDto>()
            {
                new PoseOverriderDto()
                {
                    poseIndexInPoseManager = 0,
                    activationMode = (ushort)PoseActivationMode.HOVER_ENTER,
                    poseConditions = new List<AbstractPoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            Magnitude = 1,
                            BoneOrigin = trackedSkeletonBone.boneType,
                            TargetNodeId = 100012
                        }
                    }.ToArray()
                },
                new PoseOverriderDto()
                {
                    poseIndexInPoseManager = 0,
                    activationMode = (ushort)PoseActivationMode.TRIGGER,
                    poseConditions = new List<AbstractPoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            Magnitude = 1,
                            BoneOrigin = trackedSkeletonBone.boneType,
                            TargetNodeId = 100012
                        }
                    }.ToArray()
                },
                new PoseOverriderDto()
                {
                    poseIndexInPoseManager = 0,
                    activationMode = (ushort)PoseActivationMode.NONE,
                    poseConditions = new List<AbstractPoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            Magnitude = 1,
                            BoneOrigin = trackedSkeletonBone.boneType,
                            TargetNodeId = 100012
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