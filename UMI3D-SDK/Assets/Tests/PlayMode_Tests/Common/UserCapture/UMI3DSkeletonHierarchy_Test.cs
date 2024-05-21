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
using System.Linq;
using TestUtils.UserCapture;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Description.Common
{
    [TestFixture, TestOf(nameof(UMI3DSkeletonHierarchy))]
    public class UMI3DSkeletonHierarchy_Test
    {
        private GameObject rootGo;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            rootGo = new GameObject("Root");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(rootGo);
        }

        #endregion Test SetUp

        // TODO: Replace the region name by the tested method name

        #region UMI3DSkeletonHierarchy

        [Test]
        public void UMI3DSkeletonHierarchy_Null()
        {
            // GIVEN
            IUMI3DSkeletonHierarchyDefinition definition = null;

            // WHEN
            UMI3DSkeletonHierarchy hierarchy = new(definition);

            // THEN
            Assert.AreEqual(1, hierarchy.Relations.Count);
            Assert.AreEqual(BoneType.Hips, hierarchy.Relations.Keys.First());
            Assert.AreEqual(BoneType.None, hierarchy.Relations.Values.First().boneTypeParent);
        }

        [Test]
        public void UMI3DSkeletonHierarchy()
        {
            // GIVEN

            List<IUMI3DSkeletonHierarchyDefinition.BoneRelation> boneRelations = new()
            {
                new() { parentBoneType = BoneType.None, boneType = BoneType.Hips, relativePosition = Vector3.zero.Dto() },
                new() { parentBoneType = BoneType.Hips, boneType = BoneType.Chest, relativePosition = Vector3.zero.Dto() },
                new() { parentBoneType = BoneType.Chest, boneType = BoneType.Spine, relativePosition = Vector3.zero.Dto() },
                new() { parentBoneType = BoneType.Chest, boneType = BoneType.LeftForearm, relativePosition = Vector3.zero.Dto() }
            };

            Mock<IUMI3DSkeletonHierarchyDefinition> definition = new Mock<IUMI3DSkeletonHierarchyDefinition>();
            definition.Setup(x => x.Relations).Returns(boneRelations);

            // WHEN
            UMI3DSkeletonHierarchy hierarchy = new(definition.Object);

            // THEN
            Assert.AreEqual(boneRelations.Select(x => x.boneType).Distinct().Count(), hierarchy.Relations.Count);
        }

        #endregion UMI3DSkeletonHierarchy

        #region Generate

        [Test]
        public void Generate()
        {
            // GIVEN
            Transform root = rootGo.transform;

            List<IUMI3DSkeletonHierarchyDefinition.BoneRelation> boneRelations = new()
            {
                new() { parentBoneType = BoneType.None, boneType = BoneType.Hips, relativePosition = Vector3.zero.Dto() },
                new() { parentBoneType = BoneType.Hips, boneType = BoneType.Chest, relativePosition = Vector3.zero.Dto() },
                new() { parentBoneType = BoneType.Chest, boneType = BoneType.Spine, relativePosition = Vector3.zero.Dto() },
                new() { parentBoneType = BoneType.Chest, boneType = BoneType.LeftForearm, relativePosition = Vector3.zero.Dto() }
            };
            Mock<IUMI3DSkeletonHierarchyDefinition> definition = new Mock<IUMI3DSkeletonHierarchyDefinition>();
            definition.Setup(x => x.Relations).Returns(boneRelations);

            UMI3DSkeletonHierarchy hierarchy = new(definition.Object);

            // WHEN
            var hierarchyGenerated = hierarchy.Generate(root);

            // THEN
            Assert.AreEqual(boneRelations.Count(), hierarchyGenerated.Count());

            foreach (var node in hierarchyGenerated)
            {
                Assert.AreEqual(BoneTypeHelper.GetBoneName(node.Key), node.Value.name);
            }
        }

        #endregion Generate

        #region Apply

        /// <summary>
        /// Given a hierarchy and an action to apply to each bone of the hierarchy strating from a bone StartBone,
        /// when the action is null,
        /// then an exception is thrown.
        /// </summary>
        [Test]
        public void Apply_Null()
        {
            // given
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            // when
            TestDelegate action = () => hierarchy.Apply(null);

            // then
            Assert.Throws<System.ArgumentNullException>(action);
        }

        /// <summary>
        /// Given a hierarchy and an action to apply to each bone of the hierarchy strating from a bone StartBone,
        /// when application is request from root bone,
        /// then the action is applied on all bones of the hierarchy.
        /// </summary>
        [Test]
        public void Apply()
        {
            // given
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            bool hasBeenCalled = false;
            List<uint> bonesCalled = new();

            void ToApply(uint bone)
            {
                hasBeenCalled = true;
                bonesCalled.Add(bone);
            }

            // when
            hierarchy.Apply(ToApply);

            // then
            Assert.IsTrue(hasBeenCalled);
            Assert.IsTrue(hierarchy.Relations.Keys.ToHashSet().SetEquals(bonesCalled));
        }

        /// <summary>
        /// Given a hierarchy and an action to apply to each bone of the hierarchy,
        /// when application on hierarchy is requested from a bone StartBone that is a lower bone than the root bone,
        /// then the action is applied only for the child bones of StartBone.
        /// </summary>
        [Test]
        public void Apply_StartBone_OtherThanRoot()
        {
            // given
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            bool hasBeenCalled = false;
            List<uint> bonesCalled = new();

            void ToApply(uint bone)
            {
                hasBeenCalled = true;
                bonesCalled.Add(bone);
            }

            // when
            hierarchy.Apply(ToApply, BoneType.Chest);

            // then
            Assert.IsTrue(hasBeenCalled);
            Assert.IsTrue(hierarchy.Relations.Keys.Except(new uint[] { BoneType.Hips }).ToHashSet().SetEquals(bonesCalled));
        }

        /// <summary>
        /// Given a hierarchy and an action to apply to each bone of the hierarchy strating from a bone StartBone,
        /// when StartBone is None,
        /// then the action is never called.
        /// </summary>
        [Test]
        public void Apply_StartBone_None()
        {
            // given
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            bool hasBeenCalled = false;
            void ToApply(uint bone)
            {
                hasBeenCalled = true;
            }

            // when
            hierarchy.Apply(ToApply, startBone: BoneType.None);

            // then
            Assert.IsFalse(hasBeenCalled);
        }

        /// <summary>
        /// Given a hierarchy and an action to apply to each bone of the hierarchy strating from a bone StartBone,
        /// when StartBone is not in the hierarchy,
        /// then the action is never called.
        /// </summary>
        [Test]
        public void Apply_StartBone_UndefinedInHierarchy()
        {
            // given
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            bool hasBeenCalled = false;
            void ToApply(uint bone)
            {
                hasBeenCalled = true;
            }

            // when
            hierarchy.Apply(ToApply, startBone: BoneType.RightMiddleProximal);

            // then
            Assert.IsFalse(hasBeenCalled);
        }

        #endregion Apply
    }
}