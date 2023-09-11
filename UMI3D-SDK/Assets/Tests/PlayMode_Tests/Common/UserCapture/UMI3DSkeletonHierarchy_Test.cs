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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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

            List<UMI3DSkeletonHierarchyDefinition.BoneRelation> boneRelations = new()
            {
                new(BoneType.Hips, BoneType.None, Vector3.one),
                new(BoneType.Spine, BoneType.Hips, Vector3.one),
                new(BoneType.Head, BoneType.Hips, Vector3.one),
                new(BoneType.LeftAnkle, BoneType.Head, Vector3.one),
                new(BoneType.LeftAnkle, BoneType.RightAnkle, Vector3.one),
            };


            Mock<IUMI3DSkeletonHierarchyDefinition> definition = new Mock<IUMI3DSkeletonHierarchyDefinition>();
            definition.Setup(x => x.Relations).Returns(boneRelations);

            // WHEN
            UMI3DSkeletonHierarchy hierarchy = new(definition.Object);

            // THEN
            Assert.AreEqual(boneRelations.Select(x=>x.Bonetype).Distinct().Count(), hierarchy.Relations.Count);
        }

        #endregion UMI3DSkeletonHierarchy

        #region Generate

        [Test]
        public void Generate()
        {
            // GIVEN
            Transform root = rootGo.transform;

            List<UMI3DSkeletonHierarchyDefinition.BoneRelation> boneRelations = new()
            {
                new(BoneType.Hips, BoneType.None, Vector3.one),
                new(BoneType.Spine, BoneType.Hips, Vector3.one),
                new(BoneType.Head, BoneType.Hips, Vector3.one),
                new(BoneType.LeftAnkle, BoneType.Head, Vector3.one),
            };
            Mock<IUMI3DSkeletonHierarchyDefinition> definition = new Mock<IUMI3DSkeletonHierarchyDefinition>();
            definition.Setup(x => x.Relations).Returns(boneRelations);

            UMI3DSkeletonHierarchy hierarchy = new(definition.Object);

            // WHEN
            var hierarchyGenerated = hierarchy.Generate(root);

            // THEN
            Assert.AreEqual(boneRelations.Count, hierarchyGenerated.Count());
            
            foreach (var node in hierarchyGenerated)
            {
                Assert.AreEqual(BoneTypeHelper.GetBoneName(node.umi3dBoneType), node.boneTransform.name);
            }
        }

        #endregion Generate
    }
}