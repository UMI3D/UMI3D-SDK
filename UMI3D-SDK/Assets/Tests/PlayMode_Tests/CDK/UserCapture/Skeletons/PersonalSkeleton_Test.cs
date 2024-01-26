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
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Skeletons.CDK
{
    [TestFixture, TestOf(typeof(PersonalSkeleton))]
    public class PersonalSkeleton_Test : AbstractSkeleton_Test
    {
        private GameObject trackedSusbskeletonGo;

        private PersonalSkeleton PersonalSkeleton => abstractSkeleton as PersonalSkeleton;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            abstractSkeleton = skeletonGo.AddComponent<PersonalSkeleton>();
            abstractSkeleton.SkeletonHierarchy = new(null);

            trackedSusbskeletonGo = new GameObject("tracked subskeleton");
            var camera = trackedSusbskeletonGo.AddComponent<Camera>();

            trackedSubskeletonMock = new Mock<ITrackedSubskeleton>();
            trackedSubskeletonMock.Setup(x => x.Hips).Returns(trackedSusbskeletonGo.transform);
            trackedSubskeletonMock.Setup(x => x.ViewPoint).Returns(camera);

            poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            abstractSkeleton.Init(trackedSubskeletonMock.Object, poseSubskeletonMock.Object, unityMainThreadDispatcherMock.Object);
        }

        public override void TearDown()
        {
            base.TearDown();

            Object.Destroy(trackedSusbskeletonGo);
        }

        #region GetFrame

        [Test]
        public void GetFrame()
        {
            // when
            trackedSubskeletonMock.Setup(x => x.WriteTrackingFrame(It.IsAny<UserTrackingFrameDto>(), It.IsAny<TrackingOption>()));
            poseSubskeletonMock.Setup(x => x.WriteTrackingFrame(It.IsAny<UserTrackingFrameDto>(), It.IsAny<TrackingOption>()));

            // given
            var frame = PersonalSkeleton.GetFrame(null);

            // then
            Assert.IsTrue(PersonalSkeleton.gameObject.transform.position == frame.position.Struct(), "Positions are not the same");
            Assert.IsTrue(PersonalSkeleton.gameObject.transform.rotation == frame.rotation.Quaternion(), "Rotations are not the same");
            Assert.AreEqual(frame, PersonalSkeleton.LastFrame);
            trackedSubskeletonMock.Verify(x => x.WriteTrackingFrame(It.IsAny<UserTrackingFrameDto>(), It.IsAny<TrackingOption>()), Times.Once);
            //poseSubskeletonMock.Verify(x => x.WriteTrackingFrame(It.IsAny<UserTrackingFrameDto>(), It.IsAny<TrackingOption>()), Times.Once);
        }

        #endregion GetFrame
    }
}