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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using umi3d.common.utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static umi3d.cdk.userCapture.animation.AnimatedSubskeleton;


namespace PlayMode_Tests.UserCapture.Skeletons.CDK
{
    [TestFixture, TestOf(typeof(AbstractSkeleton))]
    public abstract class AbstractSkeleton_Test
    {
        protected AbstractSkeleton abstractSkeleton;
        protected GameObject skeletonGo;
        protected Mock<IUnityMainThreadDispatcher> unityMainThreadDispatcherMock;

        protected Mock<ITrackedSubskeleton> trackedSubskeletonMock;
        protected Mock<IPoseSubskeleton> poseSubskeletonMock;

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            ClearSingletons();

            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public virtual void SetUp()
        {
            skeletonGo = new GameObject("Susbkeleton");
            UnityEngine.Object.Instantiate(skeletonGo);

            unityMainThreadDispatcherMock = new Mock<IUnityMainThreadDispatcher>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Object.Destroy(abstractSkeleton);
            Object.Destroy(skeletonGo);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
        }

        #region Compute

        /// <summary>
        /// test that we can handle an empty list of sub skeleton in the compute method
        /// </summary>
        [Test]
        public virtual void Compute_EmptyList()
        {
            //Given
            // Empty by default

            //When
            ISkeleton results = abstractSkeleton.Compute();

            //Then
            Assert.AreEqual(1, results.Bones.Count);
            Assert.AreEqual(BoneType.Hips, results.Bones.Keys.ToList()[0]);
            Assert.AreSame(abstractSkeleton, results);
        }

        /// <summary>
        /// Test that we handle a subskeleton that has a list of animated skeletons composed of not initialized animated skeletons
        /// </summary>
        [Test]
        public virtual void Compute_ListWithEmpties()
        {
            var hierarchy = new UMI3DSkeletonHierarchy(null);
            //Given
            GameObject subskeletonGo = new GameObject("Subskeleton");
            UnityEngine.Object.Instantiate(subskeletonGo);
            var mapper = subskeletonGo.AddComponent<SkeletonMapper>();

            Mock<AnimatedSubskeleton> animatedSkeletonMock = new(mapper, new List<UMI3DAnimatorAnimation>(), 0, null, null, unityMainThreadDispatcherMock.Object);
            animatedSkeletonMock.Setup(x => x.SelfUpdatedAnimatorParameters).Returns(new List<SkeletonAnimationParameter>() { new(new()) });

            animatedSkeletonMock.Setup(x => x.GetPose(hierarchy)).Returns(new SubSkeletonPoseDto());

            List<AnimatedSubskeleton> animatedSubskeletons = new()
            {
                animatedSkeletonMock.Object,
                animatedSkeletonMock.Object
            };

            foreach (var animatedSubskeleton in animatedSubskeletons)
                abstractSkeleton.AddSubskeleton(animatedSubskeleton);

            //When
            ISkeleton results = abstractSkeleton.Compute();

            //Then
            Assert.AreEqual(1, results.Bones.Count);
            Assert.AreSame(abstractSkeleton, results);
        }

        /// <summary>
        /// Test that we can compute correctly With one animated skeleton with bones
        /// </summary>
        [Test]
        public virtual void Compute_OneAnimatedSkeletonWithBones()
        {
            var hierarchy = new UMI3DSkeletonHierarchy(null);
            //Given
            GameObject subskeletonGo = new GameObject("Subskeleton");
            UnityEngine.Object.Instantiate(subskeletonGo);
            var mapper = subskeletonGo.AddComponent<SkeletonMapper>();

            Mock<AnimatedSubskeleton> animatedSkeletonMock = new(mapper, new List<UMI3DAnimatorAnimation>(), 0, null, null, unityMainThreadDispatcherMock.Object);
            animatedSkeletonMock.Setup(x => x.SelfUpdatedAnimatorParameters).Returns(new List<SkeletonAnimationParameter>() { new(new()) });
          
            var poseDto = new SubSkeletonPoseDto()

            {
                bones = new List<SubSkeletonBoneDto>
                {
                    new SubSkeletonBoneDto()
                    {
                        boneType = BoneType.CenterFeet,
                        rotation = Vector4.one.Dto()
                    },
                }
            };

            animatedSkeletonMock.Setup(x => x.GetPose(hierarchy)).Returns(poseDto);

            List<AnimatedSubskeleton> animatedSubskeletons = new()
            {
                animatedSkeletonMock.Object,
                animatedSkeletonMock.Object
            };

            foreach (var animatedSubskeleton in animatedSubskeletons)
                abstractSkeleton.AddSubskeleton(animatedSubskeleton);

            //When
            ISkeleton results = abstractSkeleton.Compute();

            //Then
            Assert.AreSame(abstractSkeleton, results);
        }

        #endregion Compute

        #region UpdateBones

        [Test]
        public void UpdateBones()
        {
            // given
            var frameDto = new UserTrackingFrameDto();
            trackedSubskeletonMock.Setup(x => x.UpdateBones(It.IsAny<UserTrackingFrameDto>()));
            poseSubskeletonMock.Setup(x => x.UpdateBones(It.IsAny<UserTrackingFrameDto>()));

            // when
            abstractSkeleton.UpdateBones(frameDto);

            // then
            Assert.AreEqual(frameDto, abstractSkeleton.LastFrame);
            trackedSubskeletonMock.Verify(x => x.UpdateBones(It.IsAny<UserTrackingFrameDto>()), Times.Once);
            //poseSubskeletonMock.Verify(x => x.UpdateBones(It.IsAny<UserTrackingFrameDto>()), Times.Once);
        }

        #endregion UpdateBones

        #region GetCameraDto

        [Test]
        public void GetCameraDto()
        {
            // given

            // when
            var dto = abstractSkeleton.GetCameraDto();

            // then
            Assert.AreEqual(1f, dto.scale);
            Assert.AreEqual(abstractSkeleton.TrackedSubskeleton.ViewPoint.projectionMatrix, dto.projectionMatrix.Struct());
            Assert.AreEqual(BoneType.Viewpoint, dto.boneType);
        }

        #endregion GetCameraDto

        #region AddSusbskeleton

        [Test]
        public void AddSusbskeleton_Null()
        {
            // given
            var subskeletons = abstractSkeleton.Subskeletons.ToList();

            // when
            abstractSkeleton.AddSubskeleton(null);

            // then
            Assert.AreEqual(subskeletons.Count, abstractSkeleton.Subskeletons.Count);
            for (int i = 0; i < subskeletons.Count; i++)
            {
                Assert.AreEqual(subskeletons[i], abstractSkeleton.Subskeletons[i]);
            }
        }

        [UnityTest]
        public IEnumerator AddSusbskeleton_AnimatedSusbskeleton()
        {
            // given
            var subskeletons = abstractSkeleton.Subskeletons.ToList();

            var newSubskeletonMock = new Mock<IAnimatedSubskeleton>();
              
            newSubskeletonMock.Setup(x => x.SelfUpdatedAnimatorParameters).Returns(new List<SkeletonAnimationParameter>() { new(new()) });

            newSubskeletonMock.Setup(x => x.StartParameterSelfUpdate(abstractSkeleton));

            unityMainThreadDispatcherMock.Setup(x => x.Enqueue(It.IsAny<System.Action>())).Callback<System.Action>(r => r()); // callback allow the nested code to be run also

            // when
            abstractSkeleton.AddSubskeleton(newSubskeletonMock.Object);

            yield return null;

            // then
            Assert.AreEqual(subskeletons.Count + 1, abstractSkeleton.Subskeletons.Count);
            newSubskeletonMock.Verify(x => x.StartParameterSelfUpdate(abstractSkeleton), Times.Once);
        }

        [UnityTest]
        public IEnumerator AddSusbskeleton_SeveralAnimatedSubskeletonsOrderedInsert()
        {
            // given
            var subskeletons = abstractSkeleton.Subskeletons.Where(x=>x is IAnimatedSubskeleton).ToList();

            unityMainThreadDispatcherMock.Setup(x => x.Enqueue(It.IsAny<System.Action>())).Callback<System.Action>(r => r()); // callback allow the nested code to be run also

            // nb : mocking AnimatedSkeleton conflicts with the IComparable interface default implementation in ISubskeleton
            var newSubskeleton1 = new AnimatedSubskeleton(null, new List<UMI3DAnimatorAnimation>(), priority: 2, null, null, null);
            var newSubskeleton2 = new AnimatedSubskeleton(null, new List<UMI3DAnimatorAnimation>(), priority: 1, null, null, null);
            var newSubskeleton3 = new AnimatedSubskeleton(null, new List<UMI3DAnimatorAnimation>(), priority: 3, null, null, null);
            abstractSkeleton.AddSubskeleton(newSubskeleton1);
            abstractSkeleton.AddSubskeleton(newSubskeleton2);

            // when
            abstractSkeleton.AddSubskeleton(newSubskeleton3);

            yield return null;

            // then
            var sortedSubskeletons = abstractSkeleton.Subskeletons.Where(x => x is IAnimatedSubskeleton).OrderBy(x => x.Priority).ToList();
            var resultSubskeletons = abstractSkeleton.Subskeletons.Where(x => x is IAnimatedSubskeleton).ToList();

            Assert.AreEqual(subskeletons.Count + 3, resultSubskeletons.Count);
            for (int i = 0; i < sortedSubskeletons.Count; i++)
            {
                Assert.AreEqual(sortedSubskeletons[i], resultSubskeletons[i]);
            }
        }

        #endregion AddSusbskeleton

        #region RemoveSusbskeleton

        [Test]
        public void RemoveSubskeleton_Null()
        {
            // given
            var subskeletons = abstractSkeleton.Subskeletons.ToList();

            // when
            abstractSkeleton.RemoveSubskeleton(null);

            // then
            Assert.AreEqual(subskeletons.Count, abstractSkeleton.Subskeletons.Count);
            for (int i = 0; i < subskeletons.Count; i++)
            {
                Assert.AreEqual(subskeletons[i], abstractSkeleton.Subskeletons[i]);
            }
        }

        [UnityTest]
        public IEnumerator RemoveSubskeleton_AnimatedSubskeleton()
        {
            // given
            var newSubskeletonMock = new Mock<AnimatedSubskeleton>(null,
                new List<UMI3DAnimatorAnimation>(0),
                0,
                new umi3d.common.userCapture.animation.SkeletonAnimationParameterDto[1] { new() },
                null,
                null
                );
            newSubskeletonMock.Setup(x => x.SelfUpdatedAnimatorParameters).Returns(new List<SkeletonAnimationParameter>() { new(new()) });
            unityMainThreadDispatcherMock.Setup(x => x.Enqueue(It.IsAny<System.Action>())).Callback<System.Action>(r => r()); // callback allow the nested code to be run also

            abstractSkeleton.AddSubskeleton(newSubskeletonMock.Object);

            yield return null;

            var subskeletons = abstractSkeleton.Subskeletons.ToList();

            newSubskeletonMock.Setup(x => x.StartParameterSelfUpdate(abstractSkeleton));
            newSubskeletonMock.Setup(x => x.StopParameterSelfUpdate());

            // when
            abstractSkeleton.RemoveSubskeleton(newSubskeletonMock.Object);

            yield return null;

            // then
            Assert.AreEqual(subskeletons.Count - 1, abstractSkeleton.Subskeletons.Count);
            newSubskeletonMock.Verify(x => x.StopParameterSelfUpdate(), Times.Once);
        }

        #endregion RemoveSusbskeleton
    }
}