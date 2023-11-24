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
        /// <summary>
        /// Create a new pose.
        /// </summary>
        /// <param name="skeleton"></param>
        public void CreatePose(PoseEditorSkeleton skeleton)
        {
            skeleton.currentPose = (UMI3DPose_so)ScriptableObject.CreateInstance(typeof(UMI3DPose_so));
            skeleton.currentPose.name = "Unnamed pose";
        }

        /// <summary>
        /// Saves a scriptable object at given path
        /// --> if you got many root on your skeleton it wil generate a scriptable object per root
        /// --> keep in mind that its normal that is you add a parent root you delete all the children root a
        ///         and that when you add a children root it dosent touch the parent once, (this last feature has to be changed at somepoint)
        /// </summary>
        public void SavePose(PoseEditorSkeleton skeleton, string path, string name, out bool success)
        {
            if (path == string.Empty)
                path = "Assets";

            var roots = skeleton.boneComponents.Where(bc => bc.isRoot);

            foreach (PoseSetterBoneComponent r in roots)
            {
                r.transform.rotation = Quaternion.identity; // security to make sure that the positions and rotation are right
                List<BoneDto> bonsPoseSos = new();
                var pose_So = (UMI3DPose_so)ScriptableObject.CreateInstance(typeof(UMI3DPose_so));
                pose_So.name = name;

                string fileName = roots.Count() == 1 ? name : $"{name}_from_{BoneTypeHelper.GetBoneName(r.BoneType)}";
                string completeName = System.IO.Path.ChangeExtension(fileName, ".asset");
                AssetDatabase.CreateAsset(pose_So, System.IO.Path.Combine(path, completeName));

                List<PoseSetterBoneComponent> bonesToSave = r.GetComponentsInChildren<PoseSetterBoneComponent>()
                                                             .Where(bc => bc.BoneType != BoneType.None)
                                                             .ToList();

                Vector4Dto rootRotation = r.transform.rotation.Dto();
                PoseAnchorDto bonePoseDto = CreateBoneAnchor(rootRotation, r);
                bonesToSave.RemoveAt(0);

                try
                {
                    bonesToSave.ForEach(bc =>
                    {
                        Vector4Dto boneRotation = bc.transform.rotation.Dto();
                        var bonePose_So = new BoneDto()
                        {
                            boneType = bc.BoneType,
                            rotation = boneRotation
                        };

                        AssetDatabase.SaveAssets();

                        bonsPoseSos.Add(bonePose_So);
                    });

                    pose_So.Init(bonsPoseSos, bonePoseDto);
                    EditorUtility.SetDirty(pose_So);
                    AssetDatabase.SaveAssets();
                }
                catch (IOException e)
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
        public void LoadPose(PoseEditorSkeleton skeleton, UMI3DPose_so pose, out bool success)
        {
            if (skeleton.boneComponents?.Count == 0)
            {
                Debug.Log($"<color=red>A skeleton with rigs is expected.</color>");
                success = false;
                return;
            }

            if (pose == null)
            {
                Debug.Log($"<color=red>An UMI3DPose_so is expected.</color>");
                success = false;
                return;
            }

            skeleton.currentPose = pose;
            success = true;
        }


        private PoseAnchorDto CreateBoneAnchor(Vector4Dto rootRotation, PoseSetterBoneComponent r)
        {
            var bonePoseDto = new PoseAnchorDto()
            {
                bone = r.BoneType,
                position = r.transform.position.Dto(),
                rotation = rootRotation
            };
            return new NodePoseAnchorDto()
            {
                bone = bonePoseDto.bone,
                position = bonePoseDto.position,
                rotation = bonePoseDto.rotation
            };
        }
    }
}
#endif
