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

using UnityEngine;

namespace TestUtils
{
    public class IOTest
    {
        public static readonly string TEST_TMP_FOLDER_PATH = System.IO.Path.Combine(Application.dataPath, "Tests", "TEST_TMP");

        public virtual void SetUp()
        {
            System.IO.Directory.CreateDirectory(TEST_TMP_FOLDER_PATH);
        }

        public virtual void TearDown()
        {
            System.IO.Directory.Delete(TEST_TMP_FOLDER_PATH, true);

            // in case a .meta unity file has been generated
            string metaFilePath = System.IO.Path.ChangeExtension(TEST_TMP_FOLDER_PATH, ".meta");
            if (System.IO.File.Exists(metaFilePath))
                System.IO.File.Delete(metaFilePath);
        }
    }
}