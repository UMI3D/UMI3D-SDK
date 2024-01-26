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

#if UNITY_EDITOR
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using umi3d.common.userCapture.description;
using UnityEditor;
using UnityEngine;

namespace umi3d.common.userCapture.pose.editor
{
    /// <summary>
    /// Service to save a skeleton pose.
    /// </summary>
    public class PoseSaverService
    {
        private const string DEFAULT_PATH = "Assets";

        private static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            TypeNameHandling = TypeNameHandling.None
        };

        /// <summary>
        /// Saves a .umi3dpose asset at given path.
        /// </summary>
        /// --> if you got many root on your skeleton it wil generate a scriptable object per root
        /// --> keep in mind that its normal that is you add a parent root you delete all the children root a
        ///         and that when you add a children root it dosent touch the parent once, (this last feature has to be changed at somepoint)
        public void SavePose(IEnumerable<PoseSetterBoneComponent> roots, string filePath, out bool success)
        {
            if (roots is null)
                throw new System.ArgumentNullException(nameof(roots));

            if (filePath is null || filePath == string.Empty)
                filePath = Path.ChangeExtension(Path.Combine(DEFAULT_PATH, PoseEditorParameters.DEFAULT_POSE_NAME), PoseEditorParameters.POSE_FORMAT_EXTENSION);

            foreach (PoseSetterBoneComponent rootBoneComponent in roots)
            {
                Quaternion originalRootRotation = rootBoneComponent.transform.rotation;
                rootBoneComponent.transform.rotation = Quaternion.identity; // security to make sure that the positions and rotation are right

                PoseDto poseDto = new PoseDto
                {
                    anchor = new PoseAnchorDto()
                    {
                        bone = rootBoneComponent.BoneType,
                        position = rootBoneComponent.transform.position.Dto(),
                        rotation = rootBoneComponent.transform.rotation.Dto()
                    },

                    bones = rootBoneComponent.GetComponentsInChildren<PoseSetterBoneComponent>()
                                    .Where(bc => bc.BoneType != BoneType.None)
                                    .Skip(1) // skip root
                                    .Select(bc =>
                                    {
                                        Vector4Dto boneRotation = bc.transform.rotation.Dto();
                                        return new BoneDto()
                                        {
                                            boneType = bc.BoneType,
                                            rotation = boneRotation
                                        };
                                    }).ToList()
                };

                rootBoneComponent.transform.rotation = originalRootRotation; // put back rotation before saving

                string savePath = filePath;
                if (roots.Count() > 1)
                {
                    savePath = Path.Combine(Path.GetDirectoryName(savePath), Path.GetFileNameWithoutExtension(savePath) + $"_from_{BoneTypeHelper.GetBoneName(rootBoneComponent.BoneType)}");
                    savePath = Path.ChangeExtension(savePath, PoseEditorParameters.POSE_FORMAT_EXTENSION);
                }

                // write the string in a file IO
                try
                {
                    string jsonString = JsonConvert.SerializeObject(poseDto, Formatting.Indented, serializationSettings);
                    File.WriteAllText(savePath, jsonString);

                    AssetDatabase.SaveAssets(); // import the json as a text asset if within project
                }
                catch (System.Exception e) when (e is IOException or JsonSerializationException)
                {
                    Debug.LogException(e);
                    success = false;
                    return;
                }
            }

            success = true;
        }

        /// <summary>
        /// Load one pose asset and apply all bone pose to the current skeleton in scene view
        /// </summary>
        public PoseDto LoadPose(string path, out bool success)
        {
            success = false;

            if (path is null)
                throw new System.ArgumentNullException(nameof(path));

            try
            {
                string fileContent = File.ReadAllText(path);
                PoseDto pose = JsonConvert.DeserializeObject<PoseDto>(fileContent, serializationSettings);
                
                if (pose is null)
                    return null;

                success = true;
                return pose;
            }
            catch (System.Exception e) when (e is IOException or JsonSerializationException)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
#endif