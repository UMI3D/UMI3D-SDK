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

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            trackedSkeletonGo = new GameObject("Tracked Skeleton");
            Object.Instantiate(trackedSkeletonGo);
            trackedSkeletonGo.transform.position = Vector3.zero;
            trackedSkeletonGo.transform.rotation = Quaternion.identity;
            trackedSkeletonGo.transform.localScale = Vector3.one;
            trackedSkeleton = trackedSkeletonGo.AddComponent<TrackedSkeleton>();
        }

        #endregion Test SetUp

        #region GetCameraDto

        [Test]
        public void Test_GetCameraDto()
        {
            // GIVEN
            TrackedSkeleton emptyTrackedSkeleton = null;

            // WHEN
            var cameraDto = emptyTrackedSkeleton.GetCameraDto();

            // THEN
            Assert.IsNull(cameraDto);
        }

        #endregion GetCameraDto

        #region GetPose

        [Test]
        public void Test_GetPose_NoTrackedSkeletonBone_NoDistantController()
        {
            // GIVEN
            TrackedSkeleton trackedSkeleton = new TrackedSkeleton();

            var targetPose = new PoseDto();

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();

            trackedSkeleton.bones = bones;

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
            TrackedSkeleton trackedSkeleton = new TrackedSkeleton();

            var targetPose = new PoseDto();

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>()
            {
                { BoneType.LeftKnee, new TrackedSkeletonBone() { boneType = BoneType.LeftKnee} },
                { BoneType.RightToeBase, new TrackedSkeletonBone() { boneType = BoneType.RightToeBase} },
                { BoneType.Spine, new TrackedSkeletonBone() { boneType = BoneType.Spine} }
            };

            trackedSkeleton.bones = bones;

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
            TrackedSkeleton trackedSkeleton = new TrackedSkeleton();

            var targetPose = new PoseDto();

            List<IController> controllers = new List<IController>()
            {
                new DistantController() {boneType = BoneType.Head, isActif = true,}
            };

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>()
            {
                { BoneType.LeftKnee, new TrackedSkeletonBone() { boneType = BoneType.LeftKnee} },
                { BoneType.RightToeBase, new TrackedSkeletonBone() { boneType = BoneType.RightToeBase} },
                { BoneType.Spine, new TrackedSkeletonBone() { boneType = BoneType.Spine} },
                { BoneType.Head, new TrackedSkeletonBoneController() {boneType = BoneType.Head} },
            };

            trackedSkeleton.bones = bones;

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
            TrackedSkeleton trackedSkeleton = new TrackedSkeleton();

            var targetPose = new PoseDto();

            List<IController> controllers = new List<IController>();

            Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>()
            {
                { BoneType.LeftKnee, new TrackedSkeletonBone() { boneType = BoneType.LeftKnee} },
                { BoneType.RightToeBase, new TrackedSkeletonBone() { boneType = BoneType.RightToeBase} },
                { BoneType.Spine, new TrackedSkeletonBone() { boneType = BoneType.Spine} }
            };

            trackedSkeleton.bones = bones;

            // WHEN
            var pose = trackedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        #endregion GetPose
    }
}
