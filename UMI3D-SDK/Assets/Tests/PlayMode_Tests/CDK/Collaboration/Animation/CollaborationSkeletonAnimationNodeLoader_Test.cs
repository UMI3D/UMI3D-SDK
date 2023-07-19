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
using PlayMode_Tests.UserCapture.Animation.CDK;
using umi3d.cdk.collaboration.userCapture;
using umi3d.cdk.collaboration.userCapture.animation;

namespace PlayMode_Tests.Collaboration.UserCapture.Animation.CDK
{
    [TestFixture, TestOf(typeof(CollaborationSkeletonAnimationNodeLoader))]
    public class CollaborationSkeletonAnimationNodeLoader_Test : SkeletonAnimationNodeLoader_Test
    {
        protected Mock<ICollaborationSkeletonsManager> collaborativeSkeletonsManagerMock;

        #region Test SetUp

        [SetUp]
        public override void SetUp()
        {
            environmentManagerMock = new();
            loadingManagerMock = new();
            resourcesManagerMock = new();
            coroutineManagerMock = new();
            personnalSkeletonServiceMock = new();
            clientServerMock = new();
            collaborativeSkeletonsManagerMock = new();

            skeletonAnimationNodeLoader = new CollaborationSkeletonAnimationNodeLoader(environmentManagerMock.Object,
                                                                          loadingManagerMock.Object,
                                                                          resourcesManagerMock.Object,
                                                                          coroutineManagerMock.Object,
                                                                          personnalSkeletonServiceMock.Object,
                                                                          collaborativeSkeletonsManagerMock.Object,
                                                                          clientServerMock.Object);
        }

        #endregion Test SetUp
    }
}