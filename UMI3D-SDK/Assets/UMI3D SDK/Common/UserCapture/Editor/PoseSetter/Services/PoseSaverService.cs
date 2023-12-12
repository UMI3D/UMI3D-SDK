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
        /// Saves a scriptable object at given path
        /// --> if you got many root on your skeleton it wil generate a scriptable object per root
        /// --> keep in mind that its normal that is you add a parent root you delete all the children root a
        ///         and that when you add a children root it dosent touch the parent once, (this last feature has to be changed at somepoint)
        /// </summary>
        public void SavePose(IEnumerable<PoseSetterBoneComponent> roots, string filePath, out bool success)
        {
            if (filePath == string.Empty)
                filePath = Path.ChangeExtension(Path.Combine(DEFAULT_PATH, PoseEditorParameters.DEFAULT_POSE_NAME), PoseEditorParameters.POSE_FORMAT_EXTENSION);

            foreach (PoseSetterBoneComponent rootBoneComponent in roots)
            {
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

                string savePath = filePath;
                if (roots.Count() > 1)
                {
                    savePath = Path.ChangeExtension(savePath, "") + $"_from_{rootBoneComponent.BoneType}";
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
        /// Load one scriptable object and apply all bone pose to the current skeleton in scene view
        /// </summary>
        public PoseDto LoadPose(string path, out bool success)
        {
            try
            {
                string fileContent = File.ReadAllText(path);
                PoseDto pose = JsonConvert.DeserializeObject<PoseDto>(fileContent, serializationSettings);
                success = true;
                return pose;
            }
            catch (System.Exception e) when (e is IOException or JsonSerializationException)
            {
                Debug.LogException(e);
                success = false;
                return null;
            }
        }
    }
}
#endif