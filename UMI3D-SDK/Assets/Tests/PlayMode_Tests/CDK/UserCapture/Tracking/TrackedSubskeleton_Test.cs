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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Tracking.CDK
{
    [TestFixture, TestOf(typeof(TrackedSubskeleton))]
    public class TrackedSubskeleton_Test
    {
        private GameObject personalSkeletonGo;
        protected GameObject trackedSkeletonGo;
        protected GameObject viewpointGo;
        protected GameObject firstTrackedBoneGo;
        protected GameObject secondTrackedBoneGo;
        protected GameObject thirdTrackedBoneGo;
        protected GameObject trackedBoneControllerGo;

        protected TrackedSubskeleton trackedSkeleton;
        protected Camera viewpoint;

        protected DistantController firstOtherDistantController;
        protected DistantController secondOtherDistantController;
        protected DistantController thirdOtherDistantController;

        protected List<TrackedSubskeletonBone> trackedBones;
        protected List<DistantController> distantControllers;

        private List<uint> bonetypesValues;

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

            personalSkeletonGo = new GameObject("skeleton");
            personalSkeletonGo.AddComponent<PersonalSkeleton>();

            // TrackedSkeleton instantiation
            trackedSkeletonGo = InstantiateGameObjectWithOffset("Tracked Skeleton");
            trackedSkeletonGo.transform.SetParent(personalSkeletonGo.transform);
            trackedSkeleton = trackedSkeletonGo.AddComponent<TrackedSubskeleton>();

            // TrackedSkeleton's viewpoint setup
            viewpointGo = InstantiateGameObjectWithOffset("Viewpoint");
            viewpoint = viewpointGo.AddComponent<Camera>();
            trackedSkeleton.viewpoint = viewpoint;

            // TrackedBones instantiation
            firstTrackedBoneGo = InstantiateGameObjectWithOffset("First Bone");
            var FirstTSB = firstTrackedBoneGo.AddComponent<TrackedSubskeletonBone>();
            FirstTSB.boneType = GetRandomBonetype();

            secondTrackedBoneGo = InstantiateGameObjectWithOffset("Second Bone");
            var SecondTSB = secondTrackedBoneGo.AddComponent<TrackedSubskeletonBone>();
            SecondTSB.boneType = GetRandomBonetype();

            thirdTrackedBoneGo = InstantiateGameObjectWithOffset("Third Bone");
            var ThirdTSB = thirdTrackedBoneGo.AddComponent<TrackedSubskeletonBone>();
            ThirdTSB.boneType = GetRandomBonetype();

            // TrackedBoneController instantiation
            trackedBoneControllerGo = InstantiateGameObjectWithOffset("Tracked Bone Controller");
            var TSBC = trackedBoneControllerGo.AddComponent<TrackedSubskeletonBoneController>();
            TSBC.boneType = GetRandomBonetype();

            trackedBones = new List<TrackedSubskeletonBone> { FirstTSB, SecondTSB, ThirdTSB, TSBC };

            // DistantControllers setup
            firstOtherDistantController = new DistantController() { isActive = true, position = Vector3.one, rotation = Quaternion.identity, isOverrider = true };
            firstOtherDistantController.boneType = GetRandomBonetype();

            secondOtherDistantController = new DistantController() { isActive = true, position = Vector3.one, rotation = Quaternion.identity, isOverrider = true };
            secondOtherDistantController.boneType = GetRandomBonetype();

            thirdOtherDistantController = new DistantController() { isActive = true, position = Vector3.one, rotation = Quaternion.identity, isOverrider = true };
            thirdOtherDistantController.boneType = GetRandomBonetype();

            distantControllers = new List<DistantController> { firstOtherDistantController, secondOtherDistantController, thirdOtherDistantController };
        }

        protected GameObject InstantiateGameObjectWithOffset(string name)
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
            UnityEngine.Object.Destroy(personalSkeletonGo);
            UnityEngine.Object.Destroy(trackedSkeletonGo);
            UnityEngine.Object.Destroy(viewpointGo);
            UnityEngine.Object.Destroy(firstTrackedBoneGo);
            UnityEngine.Object.Destroy(secondTrackedBoneGo);
            UnityEngine.Object.Destroy(thirdTrackedBoneGo);
            UnityEngine.Object.Destroy(trackedBoneControllerGo);
        }

        #endregion Test SetUp

        #region GetPose

        [Test]
        public void GetPose_NoTrackedSkeletonBone_NoDistantController()
        {

            var hierarchy = new UMI3DSkeletonHierarchy(null);
            // GIVEN
            var targetPose = new PoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSubskeletonBone> bones = new Dictionary<uint, TrackedSubskeletonBone>();

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreEqual(targetPose.bones.Count, pose.bones.Count);
            for (int i = 0; i < pose.bones.Count; i++)
            {
                Assert.AreEqual(targetPose.bones[i].boneType, pose.bones[i].boneType);
            }
        }

        [Test]
        public void GetPose_NoTrackedSkeletonBoneController_NoDistantController()
        {

            var hierarchy = new UMI3DSkeletonHierarchy(null);
            // GIVEN
            var targetPose = new SubSkeletonPoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSubskeletonBone> bones = new Dictionary<uint, TrackedSubskeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);

                targetPose.bones.Add(bone.ToBoneDto());
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);
            /// Not Trackers
            Assert.AreEqual(0, pose.bones.Count);
            for (int i = 0; i < pose.bones.Count; i++)
            {
                Assert.AreEqual(targetPose.bones[i].boneType, pose.bones[i].boneType);
            }
        }

        [Test]
        public void GetPose_NoTrackedSkeletonBoneController()
        {
            var hierarchy = new UMI3DSkeletonHierarchy(null);
            // GIVEN
            var targetPose = new SubSkeletonPoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSubskeletonBone> bones = new Dictionary<uint, TrackedSubskeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);

                targetPose.bones.Add(bone.ToBoneDto());
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);
            /// Not Trackers
            Assert.AreEqual(0, pose.bones.Count);
            for (int i = 0; i < pose.bones.Count; i++)
            {
                Assert.AreEqual(targetPose.bones[i].boneType, pose.bones[i].boneType);
            }
        }

        [Test]
        public void GetPose()
        {
            var hierarchy = new UMI3DSkeletonHierarchy(null);
            // GIVEN
            var targetPose = new SubSkeletonPoseDto() { bones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSubskeletonBone> bones = new Dictionary<uint, TrackedSubskeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);

                if (bone is TrackedSubskeletonBoneController)
                {
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActive = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
                    targetPose.bones.Add(bone.ToBoneDto());
                }
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);
            /// Not Trackers
            Assert.AreEqual((int)trackedBones.LongCount(b => b is TrackedSubskeletonBoneController), pose.bones.Count);
            for (int i = 0; i < pose.bones.Count; i++)
            {
                Assert.AreEqual(targetPose.bones[i].boneType, pose.bones[i].boneType);
            }
        }

        #endregion GetPose

        #region UpdateFrame

        [Test]
        public void UpdateFrame_NoTrackedBones()
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
            trackedSkeleton.UpdateBones(frame);

            // THEN
            Assert.AreEqual(expectedControllers.Count, trackedSkeleton.controllers.Count);
            for (int i = 0; i < expectedControllers.Count; i++)
            {
                Assert.AreEqual(expectedControllers[i].boneType, trackedSkeleton.controllers[i].boneType);
            }
        }

        [Test]
        public void UpdateFrame()
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
                expectedControllers.Add(new DistantController() { boneType = dto.boneType, isActive = true, position = dto.position.Struct(), rotation = dto.rotation.Quaternion(), isOverrider = dto.isOverrider });
            }

            // WHEN
            trackedSkeleton.UpdateBones(frame);

            // THEN
            Assert.AreEqual(expectedControllers.Count, trackedSkeleton.controllers.Count);
            for (int i = 0; i < expectedControllers.Count; i++)
            {
                Assert.AreEqual(expectedControllers[i].boneType, trackedSkeleton.controllers[i].boneType);
            }
        }

        [Test]
        public void UpdateFrame_PreviousDistantControllers()
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
                expectedControllers.Add(new DistantController() { boneType = dto.boneType, isActive = true, position = dto.position.Struct(), rotation = dto.rotation.Quaternion(), isOverrider = dto.isOverrider });
            }

            List<IController> controllers = new List<IController>();

            foreach (var controller in distantControllers)
            {
                controllers.Add(controller);
            }

            trackedSkeleton.controllers = controllers;

            // WHEN
            trackedSkeleton.UpdateBones(frame);

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
        public void WriteTrackingFrame_NoTrackedSkeletonBoneController_NoAsyncBone()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() };

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            // WHEN
            trackedSkeleton.WriteTrackingFrame(frame, option);

            // THEN
            Assert.AreEqual(frameTarget.trackedBones.Count, frame.trackedBones.Count);
        }

        [Test]
        public void WriteTrackingFrame_NoTrackedSkeletonBoneController()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() };

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            Dictionary<uint, TrackedSubskeletonBone> bones = new Dictionary<uint, TrackedSubskeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType) && bone is not TrackedSubskeletonBoneController)
                    bones.Add(bone.boneType, bone);
            }

            foreach (var pair in trackedSkeleton.bones)
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
        public void WriteTrackingFrame_NoAsyncBone()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() { trackedBones[3].ToControllerDto() } };

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSubskeletonBone> bones = new Dictionary<uint, TrackedSubskeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
                if (bone is TrackedSubskeletonBoneController)
                {
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActive = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
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
        public void WriteTrackingFrame()
        {
            // GIVEN
            UserTrackingFrameDto frameTarget = new UserTrackingFrameDto() { trackedBones = new() };

            UserTrackingFrameDto frame = new UserTrackingFrameDto() { trackedBones = new() };

            trackedSkeleton.BonesAsyncFPS = new Dictionary<uint, float>() { { trackedBones[1].boneType, 15f }, { trackedBones[0].boneType, 15f } };

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSubskeletonBone> bones = new Dictionary<uint, TrackedSubskeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
                if (bone is TrackedSubskeletonBoneController)
                {
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActive = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
                    frameTarget.trackedBones.Add(new ControllerDto() { boneType = bone.boneType, isOverrider = true, position = bone.transform.position.Dto(), rotation = bone.transform.rotation.Dto() });
                }
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            foreach (var pair in trackedSkeleton.BonesAsyncFPS)
            {
                frameTarget.trackedBones.Add(new ControllerDto() { boneType = pair.Key, isOverrider = true, position = Vector3.one.Dto(), rotation = Quaternion.identity.Dto() });
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