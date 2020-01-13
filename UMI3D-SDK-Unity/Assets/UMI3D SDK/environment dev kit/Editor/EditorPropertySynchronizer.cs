/*
Copyright 2019 Gfi Informatique

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
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using umi3d.edk;

namespace umi3d.edk.editor {

    [InitializeOnLoad]
    public class EditorPropertySynchronizer
    {
        static UMI3D umi3D;
        static UMI3DScene umi3DScene;

        static EditorPropertySynchronizer()
        {
            CheckSynchronize();
        }

        static void CheckSynchronize()
        {
            if(umi3D == null)
            {
                umi3D = GameObject.FindObjectOfType<UMI3D>();
            }
            if (umi3DScene == null)
            {
                umi3DScene = GameObject.FindObjectOfType<UMI3DScene>();
            }
            if(umi3DScene != null)
                umi3DScene.SyncProperties();
        }
    }
}
#endif