/*
Copyright 2019 - 2024 Inetum

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
using TestUtils;
using umi3d.common.userCapture;
using umi3d.common.userCapture.pose.editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Editor.PoseEditor
{
    [TestFixture, TestOf(typeof(PoseEditionService))]
    public class PoseEditorService_Test
    {
        private const int NB_BONES = 54;
        private PoseEditionService poseEditionService;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            poseEditionService = new();
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion Test SetUp

        [Test, TestOf(nameof(PoseEditionService.InitSkeleton))]
        public void InitSkeleton()
        {
            // GIVEN
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(PoseEditorParameters.SKELETON_PREFAB_PATH));
            GameObject skeletonGo = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

            PoseEditorSkeleton skeleton = new PoseEditorSkeleton()
            {
                root = null,
                anchor = BoneType.Hips
            };

            HandClosureSkeleton handClosureSkeleton = new HandClosureSkeleton()
            {
                root = null
            };

            // WHEN
            poseEditionService.InitSkeleton(skeleton, handClosureSkeleton, skeletonGo);

            // THEN
            Assert.IsNotNull(skeleton.root);
            Assert.IsNotNull(skeleton.boneComponents);
            Assert.AreEqual(NB_BONES, skeleton.boneComponents.Count);

            Assert.IsNotNull(handClosureSkeleton.root);
            Assert.IsNotNull(handClosureSkeleton.handClosureAnimator);
            Assert.AreEqual(NB_BONES, handClosureSkeleton.boneComponents.Count);
            Assert.AreEqual(skeleton.boneComponents.Count, handClosureSkeleton.boneComponents.Count);
        }

        private (PoseEditorSkeleton, HandClosureSkeleton) GetInitedSkeletons()
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(PoseEditorParameters.SKELETON_PREFAB_PATH));
            GameObject skeletonGo = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

            PoseEditorSkeleton skeleton = new PoseEditorSkeleton()
            {
                root = null,
                anchor = BoneType.Hips
            };

            HandClosureSkeleton handClosureSkeleton = new HandClosureSkeleton()
            {
                root = null
            };

            // WHEN
            poseEditionService.InitSkeleton(skeleton, handClosureSkeleton, skeletonGo);
            return (skeleton, handClosureSkeleton);
        }

        [Test, TestOf(nameof(PoseEditionService.ResetSkeleton))]
        public void ResetSkeleton()
        {
            // GIVEN
            var (skeleton, _) = GetInitedSkeletons();

            foreach (var bc in skeleton.boneComponents)
            {
                bc.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
            }

            // WHEN
            poseEditionService.ResetSkeleton(skeleton);

            // THEN
            foreach (var bc in skeleton.boneComponents)
            {
                AssertUnityStruct.AreEqual(Quaternion.identity, bc.transform.rotation);
            }
        }

        [Test, TestOf(nameof(PoseEditionService.UpdateBoneRotation))]
        public void UpdateBoneRotation()
        {
            // GIVEN
            var (skeleton, _) = GetInitedSkeletons();

            Quaternion targetRotation = Quaternion.Euler(15, 27, 19);
            uint boneType = BoneType.Neck;
            GameObject bone = skeleton.boneComponents.Find(x => x.BoneType == boneType).gameObject;

            // WHEN
            poseEditionService.UpdateBoneRotation(skeleton, boneType, targetRotation);

            // THEN
            AssertUnityStruct.AreEqual(targetRotation, bone.transform.rotation);
        }

        [Test, TestOf(nameof(PoseEditionService.ChangeIsRoot))]
        public void ChangeIsRoot_ToTrue()
        {
            // GIVEN
            var (skeleton, _) = GetInitedSkeletons();

            uint boneType = BoneType.Neck;
            PoseSetterBoneComponent bone = skeleton.boneComponents.Find(x => x.BoneType == boneType);
            PoseSetterBoneComponent boneHead = skeleton.boneComponents.Find(x => x.BoneType == BoneType.Head);
            boneHead.isRoot = true;

            // WHEN
            poseEditionService.ChangeIsRoot(skeleton, boneType, true);

            // THEN
            Assert.IsTrue(bone.isRoot);
            Assert.IsFalse(boneHead.isRoot);
        }

        [Test]
        public void ChangeIsRoot_ToFalse()
        {
            // GIVEN
            var (skeleton, _) = GetInitedSkeletons();

            uint boneType = BoneType.Neck;
            PoseSetterBoneComponent bone = skeleton.boneComponents.Find(x => x.BoneType == boneType);
            PoseSetterBoneComponent boneHead = skeleton.boneComponents.Find(x => x.BoneType == BoneType.Head);
            boneHead.isRoot = true;
            bone.isRoot = false;

            // WHEN
            poseEditionService.ChangeIsRoot(skeleton, boneType, false);

            // THEN
            Assert.IsFalse(bone.isRoot);
            Assert.IsTrue(boneHead.isRoot);
        }

        [Test, TestOf(nameof(PoseEditionService.RemoveAllRoots))]
        public void RemoveAllRoots()
        {
            // GIVEN
            var (skeleton, _) = GetInitedSkeletons();

            foreach (var bc in skeleton.boneComponents)
                bc.isRoot = true;

            // WHEN
            poseEditionService.RemoveAllRoots(skeleton);

            // THEN
            foreach (var bc in skeleton.boneComponents)
                Assert.IsFalse(bc.isRoot);
        }

        [Test, TestOf(nameof(PoseEditionService.ResetRoot))]
        public void ResetRoot()
        {
            // GIVEN
            var (skeleton, _) = GetInitedSkeletons();

            foreach (var bc in skeleton.boneComponents)
                bc.isRoot = true;

            // WHEN
            poseEditionService.ResetRoot(skeleton);

            // THEN
            foreach (var bc in skeleton.boneComponents)
            {
                if (bc.BoneType == BoneType.Hips)
                    Assert.IsTrue(bc.isRoot);
                else
                    Assert.IsFalse(bc.isRoot);
            } 
        }
    }
}