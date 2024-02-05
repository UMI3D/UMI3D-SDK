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
using PlayMode_Tests.UserCapture.Skeletons.CDK;
using umi3d.cdk.collaboration.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using UnityEngine;

namespace PlayMode_Tests.Collaboration.UserCapture.CDK
{
    public class CollaborativeSkeleton_Test : AbstractSkeleton_Test
    {
        protected GameObject trackedSusbskeletonGo;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            abstractSkeleton = skeletonGo.AddComponent<CollaborativeSkeleton>();
            abstractSkeleton.SkeletonHierarchy = new(null);

            trackedSusbskeletonGo = new GameObject("tracked subskeleton");
            var camera = trackedSusbskeletonGo.AddComponent<Camera>();

            trackedSubskeletonMock = new Mock<ITrackedSubskeleton>();
            trackedSubskeletonMock.Setup(x => x.Hips).Returns(trackedSusbskeletonGo.transform);
            trackedSubskeletonMock.Setup(x => x.ViewPoint).Returns(camera);

            poseSubskeletonMock = new Mock<IPoseSubskeleton>();

            abstractSkeleton.Init(trackedSubskeletonMock.Object, poseSubskeletonMock.Object);
        }

        public override void TearDown()
        {
            base.TearDown();

            Object.Destroy(trackedSusbskeletonGo);
        }
    }
}