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

#if UNITY_EDITOR

using umi3d.common.userCapture.description;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;


namespace umi3d.common.userCapture.pose.editor
{
    [ScriptedImporter(1, "umi3dpose")]
    public class PoseResourceAssetImporter : ScriptedImporter
    {
        public const string POSE_RESOURCES_PATH = "Poses";

        public override void OnImportAsset(AssetImportContext context)
        {
            PoseSaverService poseSaverService = new PoseSaverService();

            PoseDto pose = poseSaverService.LoadPose(context.assetPath, out bool success);
            
            AssetDatabase.DeleteAsset(context.assetPath); // we need our own scriptable not the imported file asset

            if (!success)
                return;
            UMI3DPose_so pose_so = ScriptableObject.CreateInstance<UMI3DPose_so>();
            pose_so.Init(pose);

            string projectPath = System.IO.Path.GetDirectoryName(context.assetPath); // use the asset path of the dragged file
            Debug.Log(context.assetPath);


            string poseName = System.IO.Path.GetFileNameWithoutExtension(context.assetPath);
            string assetPath = System.IO.Path.Combine(projectPath, poseName + ".asset");

            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(pose_so, uniqueAssetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = pose_so;
        }
    }
}

#endif
