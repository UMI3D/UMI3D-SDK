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

using inetum.unityUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TestUtils;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose.editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Editor.PoseEditor
{
    [TestFixture, TestOf(typeof(PoseSaverService))]
    public class PoseSaverService_Test : IOTest
    {
        private PoseSaverService poseSaverService;

        #region Test SetUp

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            poseSaverService = new ();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        #endregion Test SetUp

        #region SavePose


        [Test, TestOf(nameof(PoseSaverService.SavePose))]
        public void SavePose_NullRoots()
        {
            // GIVEN
            IEnumerable<PoseSetterBoneComponent> roots = null;
            string filePath = null;

            // WHEN
            TestDelegate action = () => poseSaverService.SavePose(roots, filePath, out _);

            // THEN
            Assert.Throws<ArgumentNullException>(action);
        }

        [Test, TestOf(nameof(PoseSaverService.SavePose))]
        public void SavePose_NoRoot()
        {
            // GIVEN
            IEnumerable<PoseSetterBoneComponent> roots = new List<PoseSetterBoneComponent>();
            string filePath = null;

            // WHEN
            poseSaverService.SavePose(roots, filePath, out bool success);

            // THEN
            Assert.IsTrue(success);
        }

        [Test, TestOf(nameof(PoseSaverService.SavePose))]
        public void SavePose()
        {
            // GIVEN
            
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(PoseEditorParameters.SKELETON_PREFAB_PATH));
            PoseSetterBoneComponent root = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.GetComponentInChildren<PoseSetterBoneComponent>();

            IEnumerable<PoseSetterBoneComponent> roots = new List<PoseSetterBoneComponent>() { root };
            string filePath = System.IO.Path.ChangeExtension(System.IO.Path.Combine(TEST_TMP_FOLDER_PATH, "TestPose"), PoseEditorParameters.POSE_FORMAT_EXTENSION);
            string completePath = System.IO.Path.Combine(Application.dataPath, filePath);

            // WHEN
            poseSaverService.SavePose(roots, completePath, out bool success);

            // THEN
            Assert.IsTrue(success);
            Assert.IsTrue(System.IO.File.Exists(completePath), $"No file created at {completePath}"); ;

            // cleaning
            System.IO.File.Delete(filePath);
        }

        [Test, TestOf(nameof(PoseSaverService.SavePose))]
        public void SavePose_SeveralRoots()
        {
            // GIVEN

            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(PoseEditorParameters.SKELETON_PREFAB_PATH));
            PoseSetterBoneComponent root = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.GetComponentInChildren<PoseSetterBoneComponent>();

            IEnumerable<PoseSetterBoneComponent> roots = root.GetComponentsInChildren<PoseSetterBoneComponent>().Take(3);
            string folderPath = System.IO.Path.Combine(Application.dataPath, TEST_TMP_FOLDER_PATH);
            string filePath = System.IO.Path.ChangeExtension(System.IO.Path.Combine(folderPath, "TestPose"), PoseEditorParameters.POSE_FORMAT_EXTENSION);

            // WHEN
            poseSaverService.SavePose(roots, filePath, out bool success);

            // THEN
            Assert.IsTrue(success);
            string[] foundFiles = System.IO.Directory.GetFiles(folderPath, $"*.{PoseEditorParameters.POSE_FORMAT_EXTENSION}", System.IO.SearchOption.AllDirectories);
            string errorMessage = $"Files are missing in {folderPath}.\n" + $"Found files : {foundFiles.Aggregate("", (x, y) => x + "\n" + y)}";
            Assert.AreEqual(roots.Count(), foundFiles.Length, errorMessage);

            // cleaning
            foreach(string foundFilePath in foundFiles)
                System.IO.File.Delete(foundFilePath);
        }

        #endregion SavePose

        #region LoadPose

        [Test, TestOf(nameof(PoseSaverService.LoadPose))]
        public void LoadPose_NullPath()
        {
            // GIVEN
            string filePath = null;

            // WHEN
            TestDelegate action = () => poseSaverService.LoadPose(filePath, out _);

            // THEN
            Assert.Throws<ArgumentNullException>(action);
        }

        [Test, TestOf(nameof(PoseSaverService.LoadPose))]
        public void LoadPose_IncorrectFile()
        {
            // GIVEN
            string filePath = System.IO.Path.ChangeExtension(System.IO.Path.Combine(TEST_TMP_FOLDER_PATH, "EmptyTestPose"), PoseEditorParameters.POSE_FORMAT_EXTENSION);
            string completePath = System.IO.Path.Combine(Application.dataPath, filePath);
            using (var f = System.IO.File.Create(completePath))
            {
                f.Dispose();
            }

            // WHEN
            PoseDto pose = poseSaverService.LoadPose(completePath, out bool success);

            // THEN
            Assert.IsFalse(success);
            Assert.IsNull(pose);

            // cleaning
            System.IO.File.Delete(completePath);
        }

        [Test, TestOf(nameof(PoseSaverService.LoadPose))]
        public void LoadPose()
        {
            // GIVEN
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(PoseEditorParameters.SKELETON_PREFAB_PATH));
            PoseSetterBoneComponent root = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.GetComponentInChildren<PoseSetterBoneComponent>();

            string filePath = System.IO.Path.ChangeExtension(System.IO.Path.Combine(TEST_TMP_FOLDER_PATH, "TestPose"), PoseEditorParameters.POSE_FORMAT_EXTENSION);
            string completePath = System.IO.Path.Combine(Application.dataPath, filePath);
            poseSaverService.SavePose(new List<PoseSetterBoneComponent> { root }, completePath, out _);

            // WHEN
            PoseDto pose = poseSaverService.LoadPose(completePath, out bool success);

            // THEN
            Assert.IsTrue(success);
            Assert.IsNotNull(pose);
            Assert.IsNotNull(pose.bones);
            Assert.Greater(pose.bones.Count, 0);
            foreach(var bone in pose.bones)
            {
                Assert.IsNotNull(bone);
                Assert.IsNotNull(bone.rotation);
                Assert.AreNotEqual(BoneType.None, bone.boneType);
            }
            Assert.IsNotNull(pose.anchor);
            Assert.IsNotNull(pose.anchor.position);
            Assert.AreNotEqual(BoneType.None, pose.anchor.bone);
        }

        #endregion LoadPose
    }
}