/*
Copyright 2019 - 2025 Inetum

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

using inetum.unityUtils.editor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.pose.editor;
using UnityEngine;

namespace TestUtils
{
    struct FakeFolder
    {
        public string name;
        public List<FakeFolder> children;
        public List<FakeFile> files;
    }
    struct FakeFile
    {
        public string name;
    }


    public class UpdateHelperTest
    {
        const string FOLDER_NAME = "Update_Helper_Test_Folder";

        private string GetTestFolderPath()
        {
            return System.IO.Path.Combine(Application.dataPath, "../"+ FOLDER_NAME);
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var folder = GetTestFolderPath();
            System.IO.Directory.CreateDirectory(folder);
            UnityEngine.Debug.Log(folder);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var folder = GetTestFolderPath();
            System.IO.Directory.Delete(folder, true);
        }


        [SetUp]
        public void Setup()
        {
            var folder = GetTestFolderPath();
            var folders = System.IO.Directory.GetDirectories(folder);
            var files = System.IO.Directory.GetFiles(folder);

            foreach(var f in folders)
                System.IO.Directory.Delete(f, true);
            foreach (var f in files)
                System.IO.File.Delete(f);
        }

        #region Test local utils
        [Test]
        public void TestGenerateFiles()
        {
            var folder = GetTestFolderPath();
            Assert.IsTrue(System.IO.Directory.Exists(folder));

            var fake = new FakeFolder()
            {
                name = "fake_A",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_A_1"},
                    new FakeFile(){name = "fake_A_2"},
                    new FakeFile(){name = "fake_A_3"},
                    new FakeFile(){name = "fake_A_4"},
                }
            };

            Generate(fake);
            TestFolder(fake);
        }
        [Test]
        public void TestGenerateFolders()
        {
            var folder = GetTestFolderPath();
            Assert.IsTrue(System.IO.Directory.Exists(folder));

            var fake = new FakeFolder()
            {
                name = "fake_A",
                children = new List<FakeFolder>()
                {
                    new FakeFolder()
                    {
                        name = "fake_B"
                    },
                    new FakeFolder()
                    {
                        name = "fake_C"
                    }
                }
            };

            Generate(fake);
            TestFolder(fake);
        }
        [Test]
        public void TestGenerateMulti()
        {
            var folder = GetTestFolderPath();
            Assert.IsTrue(System.IO.Directory.Exists(folder));

            var fake = new FakeFolder()
            {
                name = "fake_A",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_A_1"},
                    new FakeFile(){name = "fake_A_2"},
                    new FakeFile(){name = "fake_A_3"},
                    new FakeFile(){name = "fake_A_4"},
                },
                children = new List<FakeFolder>()
                {
                    new FakeFolder()
                    {
                        name = "fake_B",
                        files = new List<FakeFile>()
                        {
                            new FakeFile(){name = "fake_B_1"},
                            new FakeFile(){name = "fake_B_2"},
                            new FakeFile(){name = "fake_B_3"},
                            new FakeFile(){name = "fake_B_4"},
                        }
                    },
                    new FakeFolder()
                    {
                        name = "fake_C",
                        files = new List<FakeFile>()
                        {
                            new FakeFile(){name = "fake_C_1"},
                            new FakeFile(){name = "fake_C_2"},
                            new FakeFile(){name = "fake_C_3"},
                            new FakeFile(){name = "fake_C_4"},
                        },
                        children = new List<FakeFolder>()
                        {
                            new FakeFolder()
                            {
                                name = "fake_D",
                                files = new List<FakeFile>()
                                {
                                    new FakeFile(){name = "fake_D_1"},
                                    new FakeFile(){name = "fake_D_2"},
                                    new FakeFile(){name = "fake_D_3"},
                                    new FakeFile(){name = "fake_D_4"},
                                }
                            },
                            new FakeFolder()
                            {
                                name = "fake_E",
                                files = new List<FakeFile>()
                                {
                                    new FakeFile(){name = "fake_E_1"},
                                    new FakeFile(){name = "fake_E_2"},
                                    new FakeFile(){name = "fake_E_3"},
                                    new FakeFile(){name = "fake_E_4"},
                                }
                            }
                        }
                    }
                }
            };

            Generate(fake);
            TestFolder(fake);
        }

        void Generate(FakeFolder folder) => Generate(folder, GetTestFolderPath());

        void Generate(FakeFolder folder, string parent)
        {
            string folderPath = System.IO.Path.Combine(parent, folder.name);
            var d = System.IO.Directory.CreateDirectory(folderPath);

            if (folder.children != null)
                foreach (var child in folder.children)
                    Generate(child, folderPath);

            if (folder.files != null)
                foreach (var file in folder.files)
                {
                    string filePath = System.IO.Path.Combine(folderPath, file.name);
                    using (var f = System.IO.File.Create(filePath))
                        f.Dispose();
                }
        }

        void TestFolder(FakeFolder folder) => TestFolder(folder, GetTestFolderPath());

        void TestFolder(FakeFolder folder, string parent)
        {
            string folderPath = System.IO.Path.Combine(parent, folder.name);
            Assert.IsTrue(System.IO.Directory.Exists(folderPath),$"Folder {folderPath} do not exist");

            if (folder.children != null)
                foreach (var child in folder.children)
                    TestFolder(child, folderPath);

            if (folder.files != null)
                foreach (var file in folder.files)
                {
                    string filePath = System.IO.Path.Combine(folderPath, file.name);
                    Assert.IsTrue(System.IO.File.Exists(filePath), $"File {filePath} do not exist");
                }
        }
        #endregion Test local utils

        /// <summary>
        /// From no to
        /// no to from
        /// from to
        /// 
        /// </summary>
        [Test]
        public void CopyFolderFilesFiles()
        {
            var folder = GetTestFolderPath();
            Assert.IsTrue(System.IO.Directory.Exists(folder));

            var from = new FakeFolder()
            {
                name = "fake_A",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_A_1"},
                    new FakeFile(){name = "fake_A_2"},
                    new FakeFile(){name = "fake_A_3"},
                    new FakeFile(){name = "fake_A_4"},
                }
            };
            var to = new FakeFolder()
            {
                name = "fake_B",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_B_1"},
                    new FakeFile(){name = "fake_B_2"},
                    new FakeFile(){name = "fake_B_3"},
                    new FakeFile(){name = "fake_B_4"},
                }
            };

            CopyFolder(from, to);
        }

        [Test]
        public void CopyFolder()
        {
            var folder = GetTestFolderPath();
            Assert.IsTrue(System.IO.Directory.Exists(folder));

            var from = new FakeFolder()
            {
                name = "fake_A",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_A_1"},
                    new FakeFile(){name = "fake_A_2"},
                    new FakeFile(){name = "fake_A_3"},
                    new FakeFile(){name = "fake_A_4"},
                }
            };
            var to = new FakeFolder()
            {
                name = "fake_B",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_B_1"},
                    new FakeFile(){name = "fake_B_2"},
                    new FakeFile(){name = "fake_B_3"},
                    new FakeFile(){name = "fake_B_4"},
                }
            };

            CopyFolder(from, to);
        }

        [Test]
        public void CopyFolderToEmpty()
        {
            var folder = GetTestFolderPath();
            Assert.IsTrue(System.IO.Directory.Exists(folder));

            var from = new FakeFolder()
            {
                name = "fake_A",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_A_1"},
                    new FakeFile(){name = "fake_A_2"},
                    new FakeFile(){name = "fake_A_3"},
                    new FakeFile(){name = "fake_A_4"},
                }
            };
            Generate(from);

            string root = GetTestFolderPath();
            string fromPath = System.IO.Path.Combine(root, from.name);
            string toPath = System.IO.Path.Combine(root, "fake_B");

            CopyFolder(fromPath, toPath);
        }

        [Test]
        public void CopyFolderFromEmpty()
        {
            var folder = GetTestFolderPath();
            Assert.IsTrue(System.IO.Directory.Exists(folder));

            var to = new FakeFolder()
            {
                name = "fake_B",
                files = new List<FakeFile>()
                {
                    new FakeFile(){name = "fake_B_1"},
                    new FakeFile(){name = "fake_B_2"},
                    new FakeFile(){name = "fake_A_3"},
                    new FakeFile(){name = "fake_A_4"},
                }
            };
            Generate(to);

            string root = GetTestFolderPath();
            string fromPath = System.IO.Path.Combine(root, "fake_A");
            string toPath = System.IO.Path.Combine(root, to.name);

            TestDelegate action = () => UpdateHelperCopier.CopyFolder(fromPath, toPath);
            Assert.Throws<System.IO.DirectoryNotFoundException>(action);
        }

        void CopyFolder(FakeFolder from, FakeFolder to)
        {
            Generate(from);
            Generate(to);

            string root = GetTestFolderPath();
            string fromPath = System.IO.Path.Combine(root, from.name);
            string toPath = System.IO.Path.Combine(root, to.name);

            CopyFolder(fromPath, toPath);
        }

        public void CopyFolder(string from, string to)
        {
            TestDelegate action = () => UpdateHelperCopier.CopyFolder(from, to);
            Assert.DoesNotThrow(action);
        }
    }
}