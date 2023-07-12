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
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.SceneManagement;
using umi3d.common.userCapture.tracking;
using System.Linq;
using System.Reflection;
using System;
using umi3d.cdk.userCapture;

namespace PlayMode_Tests.UserCapture.Tracking.CDK
{
    [TestFixture, TestOf(typeof(TrackedSkeleton))]
    public class TrackedSkeleton_Test
    {
        protected GameObject trackedSkeletonGo;
        protected GameObject viewpointGo;
        protected GameObject firstTrackedBoneGo;
        protected GameObject secondTrackedBoneGo;
        protected GameObject thirdTrackedBoneGo;
        protected GameObject trackedBoneControllerGo;

        protected TrackedSkeleton trackedSkeleton;
        protected Camera viewpoint;

        protected DistantController firstOtherDistantController;
        protected DistantController secondOtherDistantController;
        protected DistantController thirdOtherDistantController;

        protected List<TrackedSkeletonBone> trackedBones;
        protected List<DistantController> distantControllers;

        List<uint> bonetypesValues;

        public TrackingOption option;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            // Returns the list of all bonetypes
            bonetypesValues = new List<uint>();
            IEnumerable<FieldInfo> val = typeof(BoneType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                           .Where(fi => fi.IsLiteral && !fi.IsInitOnly);
            bonetypesValues = val.Select(fi => (uint)fi.GetValue(null)).ToList();

            // Removing None & Viewpoint
            bonetypesValues.Remove(0);
            bonetypesValues.Remove(BoneType.Viewpoint);

            // Shuffle list
            bonetypesValues.OrderBy(x => Guid.NewGuid());

            // TrackedSkeleton instantiation
            trackedSkeletonGo = NewGameObject("Tracked Skeleton");
            trackedSkeleton = trackedSkeletonGo.AddComponent<TrackedSkeleton>();

            // TrackedSkeleton's viewpoint setup
            viewpointGo = NewGameObject("Viewpoint");
            viewpoint = viewpointGo.AddComponent<Camera>();
            trackedSkeleton.Viewpoint = viewpoint;

            // TrackedBones instantiation
            firstTrackedBoneGo = NewGameObject("First Bone");
            var FirstTSB = firstTrackedBoneGo.AddComponent<TrackedSkeletonBone>();
            FirstTSB.boneType = GetRandomBonetype();

            secondTrackedBoneGo = NewGameObject("Second Bone");
            var SecondTSB = secondTrackedBoneGo.AddComponent<TrackedSkeletonBone>();
            SecondTSB.boneType = GetRandomBonetype();

            thirdTrackedBoneGo = NewGameObject("Third Bone");
            var ThirdTSB = thirdTrackedBoneGo.AddComponent<TrackedSkeletonBone>();
            ThirdTSB.boneType = GetRandomBonetype();

            // TrackedBoneController instantiation
            trackedBoneControllerGo = NewGameObject("Tracked Bone Controller");
            var TSBC = trackedBoneControllerGo.AddComponent<TrackedSkeletonBoneController>();
            TSBC.boneType = GetRandomBonetype();

            trackedBones = new List<TrackedSkeletonBone> { FirstTSB, SecondTSB, ThirdTSB, TSBC };

            // DistantControllers setup
            firstOtherDistantController = new DistantController() { isActif = true, position = Vector3.one, rotation = Quaternion.identity, isOverrider = true };
            firstOtherDistantController.boneType = GetRandomBonetype();

            secondOtherDistantController = new DistantController() { isActif = true, position = Vector3.one, rotation = Quaternion.identity, isOverrider = true };
            secondOtherDistantController.boneType = GetRandomBonetype();

            thirdOtherDistantController = new DistantController() { isActif = true, position = Vector3.one, rotation = Quaternion.identity, isOverrider = true };
            thirdOtherDistantController.boneType = GetRandomBonetype();

            distantControllers = new List<DistantController> { firstOtherDistantController, secondOtherDistantController, thirdOtherDistantController };
        }

        protected GameObject NewGameObject(string name)
        {
            GameObject go = new GameObject(name);
            UnityEngine.Object.Instantiate(go);
            go.transform.position = Vector3.one;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go;
        }

        protected uint GetRandomBonetype()
        {
            uint id = bonetypesValues[0];
            bonetypesValues.RemoveAt(0);
            return id;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.Destroy(trackedSkeletonGo);
            UnityEngine.Object.Destroy(viewpointGo);
            UnityEngine.Object.Destroy(firstTrackedBoneGo);
            UnityEngine.Object.Destroy(secondTrackedBoneGo);
            UnityEngine.Object.Destroy(thirdTrackedBoneGo);
            UnityEngine.Object.Destroy(trackedBoneControllerGo);
        }

        #endregion Test SetUp

        #region GetCameraDto

        [Test]
        public void Test_GetCameraDto()
        {
            // GIVEN
            var camTarget = new UserCameraPropertiesDto()
            {
                scale = 1f,
                projectionMatrix = viewpoint.projectionMatrix.Dto(),
                boneType = BoneType.Viewpoint,
            };

            // WHEN
            var cameraDto = trackedSkeleton.GetCameraDto();

            // THEN
            Assert.IsNotNull(cameraDto);
            Assert.AreEqual(camTarget.boneType, cameraDto.boneType);
            Assert.IsTrue(camTarget.projectionMatrix.Struct() == cameraDto.projectionMatrix.Struct(), "Projection Matrix are not the same.");
            Assert.AreEqual(camTarget.scale, cameraDto.scale);
        }

        #endregion GetCameraDto

        #region GetPose

        [Test]
        public void Test_GetPose_NoTrackedSkeletonBone_NoDistantController()
        {
            // GIVEN
            var targetPose = new PoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreEqual(targetPose.bones, pose.bones);
        }


        [Test]
        public void Test_GetPose_NoTrackedSkeletonBoneController_NoDistantController()
        {
            // GIVEN
            var targetPose = new PoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreEqual(targetPose.bones, pose.bones);
        }

        [Test]
        public void Test_GetPose_NoTrackedSkeletonBoneController()
        {
            // GIVEN
            var targetPose = new PoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreEqual(targetPose.bones.Count, pose.bones.Count);
            for (int i = 0; i < pose.bones.Count; i++)
            {
                Assert.AreEqual(targetPose.bones[i].boneType, pose.bones[i].boneType);
            }
        }

        [Test]
        public void Test_GetPose()
        {
            // GIVEN
            var targetPose = new PoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
                if (bone.GetType() == typeof(TrackedSkeletonBoneController))
                {
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActif = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
                    targetPose.bones.Add(bone.ToBoneDto());
                }
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreEqual(targetPose.bones.Count, pose.bones.Count);
            for (int i = 0; i < pose.bones.Count; i++)
            {
                Assert.AreEqual(targetPose.bones[i].boneType, pose.bones[i].boneType);
            }
        }

        #endregion GetPose

        #region UpdateFrame

        [Test]
        public void Test_UpdateFrame_NoTrackedBones()
        {
            // GIVEN
            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            List<IController> expectedControllers = new List<IController>();

            List<IController> controllers = new List<IController>();

            foreach (var controller in distantControllers)
            {
                controllers.Add(controller);
            }

            trackedSkeleton.controllers = controllers;

            // WHEN
            trackedSkeleton.UpdateFrame(frame);

            // THEN
            Assert.AreEqual(expectedControllers.Count, trackedSkeleton.controllers.Count);
            for (int i = 0; i < expectedControllers.Count; i++)
            {
                Assert.AreEqual(expectedControllers[i].boneType, trackedSkeleton.controllers[i].boneType);
            }
        }

        [Test]
        public void Test_UpdateFrame()
        {
            // GIVEN
            UserTrackingFrameDto frame = new()
            {
                trackedBones = new() {
                    new ControllerDto() { boneType = GetRandomBonetype(), rotation = Quaternion.identity.Dto(), position = Vector3.zero.Dto(), isOverrider = true,},
                    new ControllerDto() { boneType = GetRandomBonetype(), rotation = Quaternion.identity.Dto(), position = Vector3.zero.Dto(), isOverrider = true },
                    new ControllerDto() { boneType = GetRandomBonetype(), rotation = Quaternion.identity.Dto(), position = Vector3.zero.Dto(), isOverrider = true }
                }
            };

            List<IController> expectedControllers = new List<IController>();

            foreach (var dto in frame.trackedBones)
            {
                expectedControllers.Add(new DistantController() { boneType = dto.boneType, isActif = true, position = dto.position.Struct(), rotation = dto.rotation.Quaternion(), isOverrider = dto.isOverrider });
            }

            // WHEN
            trackedSkeleton.UpdateFrame(frame);

            // THEN
            Assert.AreEqual(expectedControllers.Count, trackedSkeleton.controllers.Count);
            for (int i = 0; i < expectedControllers.Count; i++)
            {
                Assert.AreEqual(expectedControllers[i].boneType, trackedSkeleton.controllers[i].boneType);
            }
        }


        [Test]
        public void Test_UpdateFrame_PreviousDistantControllers()
        {
            // GIVEN
            UserTrackingFrameDto frame = new()
            {
                trackedBones = new () {
                    new ControllerDto() { boneType = GetRandomBonetype(), rotation = Quaternion.identity.Dto(), position = Vector3.zero.Dto(), isOverrider = true,},
                    new ControllerDto() { boneType = GetRandomBonetype(), rotation = Quaternion.identity.Dto(), position = Vector3.zero.Dto(), isOverrider = true },
                    new ControllerDto() { boneType = GetRandomBonetype(), rotation = Quaternion.identity.Dto(), position = Vector3.zero.Dto(), isOverrider = true }
                }
            };

            List<IController> expectedControllers = new List<IController>();

            foreach (var dto in frame.trackedBones)
            {
                expectedControllers.Add(new DistantController() { boneType = dto.boneType, isActif = true, position = dto.position.Struct(), rotation = dto.rotation.Quaternion(), isOverrider = dto.isOverrider });
            }

            List<IController> controllers = new List<IController>();

            foreach (var controller in distantControllers)
            {
                controllers.Add(controller);
            }

            trackedSkeleton.controllers = controllers;

            // WHEN
            trackedSkeleton.UpdateFrame(frame);

            // THEN
            Assert.AreEqual(expectedControllers.Count, trackedSkeleton.controllers.Count);
            for (int i = 0; i < expectedControllers.Count; i++)
            {
                Assert.AreEqual(expectedControllers[i].boneType, trackedSkeleton.controllers[i].boneType);
            }
        }

        #endregion UpdateFrame

        #region WriteFrame

        [Test]
        public void Test_WriteTrackingFrame_NoTrackedSkeletonBoneController_NoAsyncBone()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() };

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            Mock<ISkeletonManager> personalSkeletonServiceMock = new Mock<ISkeletonManager> ();
            personalSkeletonServiceMock.Setup(x => x.BonesAsyncFPS).Returns(new Dictionary<uint, float>());
            trackedSkeleton.skeletonManager = personalSkeletonServiceMock.Object;

            // WHEN
            trackedSkeleton.WriteTrackingFrame(frame, option);

            // THEN
            Assert.AreEqual(frameTarget.trackedBones.Count, frame.trackedBones.Count);
        }

        [Test]
        public void Test_WriteTrackingFrame_NoTrackedSkeletonBoneController()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() };

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            Mock<ISkeletonManager> personalSkeletonServiceMock = new();
            personalSkeletonServiceMock.Setup(x => x.BonesAsyncFPS).Returns(new Dictionary<uint, float>() { { trackedBones[0].boneType, 15f }, { trackedBones[2].boneType, 15f } });
            trackedSkeleton.skeletonManager = personalSkeletonServiceMock.Object;

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType) && bone is not TrackedSkeletonBoneController)
                    bones.Add(bone.boneType, bone);
            }

            foreach (var pair in personalSkeletonServiceMock.Object.BonesAsyncFPS)
            {
                frameTarget.trackedBones.Add(new ControllerDto() { boneType = pair.Key });
            }

            trackedSkeleton.bones = bones;

            // WHEN
            trackedSkeleton.WriteTrackingFrame(frame, option);

            // THEN
            Assert.AreEqual(frameTarget.trackedBones.Count, frame.trackedBones.Count);
            for (int i = 0; i < frameTarget.trackedBones.Count; i++)
            {
                Assert.AreEqual(frameTarget.trackedBones[i].boneType, frame.trackedBones[i].boneType);
            }
        }

        [Test]
        public void Test_WriteTrackingFrame_NoAsyncBone()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() { trackedBones[3].ToControllerDto() }};

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            Mock<ISkeletonManager> personalSkeletonServiceMock = new();
            personalSkeletonServiceMock.Setup(x => x.BonesAsyncFPS).Returns(new Dictionary<uint, float>());
            trackedSkeleton.skeletonManager = personalSkeletonServiceMock.Object;

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
                if (bone.GetType() == typeof(TrackedSkeletonBoneController))
                {
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActif = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
                }
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            trackedSkeleton.WriteTrackingFrame(frame, option);

            // THEN
            Assert.AreEqual(frameTarget.trackedBones.Count, frame.trackedBones.Count);
            for (int i = 0; i < frameTarget.trackedBones.Count; i++)
            {
                Assert.AreEqual(frameTarget.trackedBones[i].boneType, frame.trackedBones[i].boneType);
            }
        }

        [Test]
        public void Test_WriteTrackingFrame()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() };

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            Mock<ISkeletonManager> personalSkeletonServiceMock = new();
            personalSkeletonServiceMock.Setup(x => x.BonesAsyncFPS).Returns(new Dictionary<uint, float>() { { trackedBones[1].boneType, 15f }, { trackedBones[0].boneType, 15f } });
            trackedSkeleton.skeletonManager = personalSkeletonServiceMock.Object;

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
                if (bone.GetType() == typeof(TrackedSkeletonBoneController))
                {
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActif = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
                    frameTarget.trackedBones.Add(new ControllerDto() { boneType = bone.boneType, isOverrider = true, position = bone.transform.position.Dto(), rotation = bone.transform.rotation.Dto() });
                }
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            foreach (var pair in personalSkeletonServiceMock.Object.BonesAsyncFPS)
            {
                frameTarget.trackedBones.Add(new ControllerDto() { boneType = pair.Key , isOverrider = true, position = Vector3.one.Dto(), rotation = Quaternion.identity.Dto()});
            }

            // WHEN
            trackedSkeleton.WriteTrackingFrame(frame, option);

            // THEN
            Assert.AreEqual(frameTarget.trackedBones.Count, frame.trackedBones.Count);
            for (int i = 0; i < frameTarget.trackedBones.Count; i++)
            {
                Assert.AreEqual(frameTarget.trackedBones[i].boneType, frame.trackedBones[i].boneType);
            }
        }

        #endregion WriteFrame
    }
}
