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
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.cdk.userCapture.binding;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using umi3d.common.userCapture.tracking;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine.Rendering;
using inetum.unityUtils;
using umi3d.common;

namespace PlayMode_Tests.UserCapture.Animation.CDK
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

        protected TrackedSkeletonBone[] trackedBones;
        protected DistantController[] distantControllers;
        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            // returns the list of all bonetypes
            var values = new List<uint>();
            IEnumerable<FieldInfo> val = typeof(BoneType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                           .Where(fi => fi.IsLiteral && !fi.IsInitOnly);
            values = val.Select(fi => (uint)fi.GetValue(null)).ToList();

            values.Remove(0);

            values.OrderBy(x => Guid.NewGuid());

            trackedSkeletonGo = new GameObject("Tracked Skeleton");
            UnityEngine.Object.Instantiate(trackedSkeletonGo);
            trackedSkeletonGo.transform.position = Vector3.zero;
            trackedSkeletonGo.transform.rotation = Quaternion.identity;
            trackedSkeletonGo.transform.localScale = Vector3.one;
            trackedSkeleton = trackedSkeletonGo.AddComponent<TrackedSkeleton>();

            viewpointGo = new GameObject("Viewpoint");
            UnityEngine.Object.Instantiate(viewpointGo);
            viewpointGo.transform.position = Vector3.zero; // replace by random value
            viewpointGo.transform.rotation = Quaternion.identity; // replace by random value
            viewpointGo.transform.localScale = Vector3.one;
            viewpoint = viewpointGo.AddComponent<Camera>();

            firstTrackedBoneGo = new GameObject("First Bone");
            UnityEngine.Object.Instantiate(firstTrackedBoneGo);
            firstTrackedBoneGo.transform.position = Vector3.zero; // replace by random value
            firstTrackedBoneGo.transform.rotation = Quaternion.identity; // replace by random value
            firstTrackedBoneGo.transform.localScale = Vector3.one;
            var FirstTSB = firstTrackedBoneGo.AddComponent<TrackedSkeletonBone>();
            FirstTSB.boneType = values[0];
            values.RemoveAt(0);

            secondTrackedBoneGo = new GameObject("Second Bone");
            UnityEngine.Object.Instantiate(secondTrackedBoneGo);
            secondTrackedBoneGo.transform.position = Vector3.zero; // replace by random value
            secondTrackedBoneGo.transform.rotation = Quaternion.identity; // replace by random value
            secondTrackedBoneGo.transform.localScale = Vector3.one;
            var SecondTSB = secondTrackedBoneGo.AddComponent<TrackedSkeletonBone>();
            SecondTSB.boneType = values[0];
            values.RemoveAt(0);

            thirdTrackedBoneGo = new GameObject("Third Bone");
            UnityEngine.Object.Instantiate(thirdTrackedBoneGo);
            thirdTrackedBoneGo.transform.position = Vector3.zero; // replace by random value
            thirdTrackedBoneGo.transform.rotation = Quaternion.identity; // replace by random value
            thirdTrackedBoneGo.transform.localScale = Vector3.one;
            var ThirdTSB = thirdTrackedBoneGo.AddComponent<TrackedSkeletonBone>();
            ThirdTSB.boneType = values[0];
            values.RemoveAt(0);

            trackedBoneControllerGo = new GameObject("Tracked Bone Controller");
            UnityEngine.Object.Instantiate(trackedBoneControllerGo);
            trackedBoneControllerGo.transform.position = Vector3.zero; // replace by random value
            trackedBoneControllerGo.transform.rotation = Quaternion.identity; // replace by random value
            trackedBoneControllerGo.transform.localScale = Vector3.one;
            var TSBC = trackedBoneControllerGo.AddComponent<TrackedSkeletonBoneController>();
            TSBC.boneType = values[0];
            values.RemoveAt(0);

            // Factoriser

            firstOtherDistantController = new DistantController() { isActif = true, position = Vector3.zero, rotation = Quaternion.identity, isOverrider = true };
            firstOtherDistantController.boneType = values[0];
            values.RemoveAt(0);

            secondOtherDistantController = new DistantController() { isActif = true, position = Vector3.zero, rotation = Quaternion.identity, isOverrider = true };
            secondOtherDistantController.boneType = values[0];
            values.RemoveAt(0);

            thirdOtherDistantController = new DistantController() { isActif = true, position = Vector3.zero, rotation = Quaternion.identity, isOverrider = true };
            thirdOtherDistantController.boneType = values[0];
            values.RemoveAt(0);

            trackedBones = new TrackedSkeletonBone[] { FirstTSB, SecondTSB, ThirdTSB, TSBC };
            distantControllers = new DistantController[] { firstOtherDistantController, secondOtherDistantController, thirdOtherDistantController };
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
        public void Test_GetCameraDto_NoSkeleton()
        {
            // GIVEN
            TrackedSkeleton emptyTrackedSkeleton = null;

            // WHEN
            var cameraDto = emptyTrackedSkeleton.GetCameraDto();

            // THEN
            Assert.IsNull(cameraDto);
        }

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
            Assert.Equals(cameraDto, camTarget);
        }

        #endregion GetCameraDto

        #region GetPose

        [Test]
        public void Test_GetPose_NoTrackedSkeletonBone_NoDistantController()
        {
            // GIVEN
            var targetPose = new PoseDto();

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }


        [Test]
        public void Test_GetPose_NoTrackedSkeletonBoneController_NoDistantController()
        {
            // GIVEN
            var targetPose = new PoseDto();

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
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_GetPose_NoDistantController()
        {
            // GIVEN
            var targetPose = new PoseDto();

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
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_GetPose_NoTrackedSkeletonBoneController()
        {
            // GIVEN
            var targetPose = new PoseDto();

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            foreach (var bone in trackedBones)
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
            }

            foreach (var controller in distantControllers)
            {
                controllers.Add(controller);
                targetPose.bones.Add(new BoneDto() { boneType = controller.boneType, rotation = controller.rotation.Dto()});
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_GetPose()
        {
            // GIVEN
            var targetPose = new PoseDto();

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

            foreach (var controller in distantControllers)
            {
                controllers.Add(controller);
                targetPose.bones.Add(new BoneDto() { boneType = controller.boneType, rotation = controller.rotation.Dto() });
            }

            trackedSkeleton.bones = bones;
            trackedSkeleton.controllers = controllers;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        #endregion GetPose

        #region UpdateFrame

        [Test]
        public void Test_UpdateFrame_NoTrackedBones()
        {
            // GIVEN


            // WHEN


            // THEN

        }

        [Test]
        public void Test_UpdateFrame()
        {
            // GIVEN


            // WHEN


            // THEN

        }

        #endregion UpdateFrame

        #region WriteFrame



        #endregion WriteFrame

        #region GetBonePosition



        #endregion GetBonePosition

        #region GetBoneRotation


        #endregion GetBoneRotation
    }
}
