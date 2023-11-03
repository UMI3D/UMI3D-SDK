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
using inetum.unityUtils;
using System.Linq;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.Assertions;

namespace umi3d.common.userCapture.pose.editor
{
    /// <summary>
    /// Service for editing a skeleton pose.
    /// </summary>
    public class PoseEditionService
    {

        #region Skeleton

        /// <summary>
        /// This should be called each time you change the skeleton prefab
        /// Reads all of the PoseSetterBineComponent which are not None Bones and add them in the treeView
        /// </summary>
        /// <param name="value"></param>
        public void UpdateSkeletonGameObject(PoseEditorSkeleton skeleton, GameObject value)
        {
            skeleton.boneComponents = value.GetComponentsInChildren<PoseSetterBoneComponent>()
                                                            .Where(bc => bc.BoneType != BoneType.None)
                                                            .ToList();
        }

        public void ResetSkeleton(PoseEditorSkeleton skeleton, GameObject skeletonRoot)
        {
            skeletonRoot.GetComponentsInChildren<PoseSetterBoneComponent>()
                        .ForEach(bc =>
                        {
                            bc.transform.rotation = Quaternion.identity;
                        });
        }

        #endregion

        #region Rotations

        /// <summary>
        /// Change the rotation of a bone.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="boneDto"></param>
        public void UpdateBoneRotation(PoseEditorSkeleton skeleton, BoneDto boneDto)
        {
            PoseSetterBoneComponent bone_component = skeleton.boneComponents.Find(bc => bc.BoneType == boneDto.boneType);
            if (bone_component == null)
                return;
            bone_component.transform.rotation = boneDto.rotation.Quaternion();
        }

        /// <summary>
        /// Change the rotation of a bone that is an anchor.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="boneDto"></param>
        public void UpdateBoneRotation(PoseEditorSkeleton skeleton, PoseAnchorDto bonePoseDto)
        {
            PoseSetterBoneComponent bone_component = skeleton.boneComponents.Find(bc => bc.BoneType == bonePoseDto.bone);
            if (bone_component == null)
                return;
            bone_component.transform.rotation = bonePoseDto.rotation.Quaternion();
        }

        #endregion

        #region RootManagement

        /// <summary>
        /// Change if a bone is a root or not and update other bones accordingly. 
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="boneType"></param>
        /// <param name="isRoot"></param>
        public void ChangeIsRoot(PoseEditorSkeleton skeleton, uint boneType, bool isRoot)
        {
            if (skeleton.boneComponents.Count == 0)
                return;

            PoseSetterBoneComponent boneComponent = skeleton.boneComponents.Find(bc => bc.BoneType == boneType);
            UpdateRootOnSkeleton(boneComponent, isRoot);

            if (skeleton.boneComponents.All(bc => !bc.isRoot))
                ResetRoot(skeleton);
        }

        /// <summary>
        /// Set all bones isRoot to false
        /// </summary>
        public void ResetAllBones(PoseEditorSkeleton skeleton)
        {
            skeleton.boneComponents.ForEach(bc =>
            {
                if (bc.isRoot != false)
                    bc.isRoot = false;
            });
        }

        /// <summary>
        /// Put Hips as the only root.
        /// </summary>
        /// <param name="skeleton"></param>
        public void ResetRoot(PoseEditorSkeleton skeleton)
        {
            var hipsBoneComponent = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.Hips);
            UpdateRootOnSkeleton(hipsBoneComponent, true);
        }


        /// <summary>
        /// Change a root in the skeleton and the isRoot value of all child bones.
        /// </summary>
        /// <param name="boneComponent"></param>
        /// <param name="value"></param>
        public void UpdateRootOnSkeleton(PoseSetterBoneComponent boneComponent, bool value)
        {
            Assert.IsNotNull(boneComponent);

            // Cleans children roots
            boneComponent.GetComponentsInChildren<PoseSetterBoneComponent>()
                         .Where(bc => bc.BoneType != BoneType.None)
                         .ForEach(bc =>
                         {
                             if (value == true)
                             {
                                 if (bc.isRoot) // cannot be root if a parent is root
                                     bc.isRoot = false;
                             }
                         });

            boneComponent.isRoot = value;
        }

        #endregion RootManagement

        #region Symmetry

        /// <summary>
        /// Apply a symmetry to the skeleton.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="isFromLeft"></param>
        /// <param name="symmetryTarget"></param>
        public void ApplySymmetry(PoseEditorSkeleton skeleton, bool isFromLeft, SymmetryTarget symmetryTarget)
        {
            Transform[] origin_target = GetSymRoot(skeleton, isFromLeft, symmetryTarget);
            PoseSetterBoneComponent[] originBC = origin_target[0].GetComponentsInChildren<PoseSetterBoneComponent>();
            PoseSetterBoneComponent[] targetBC = origin_target[1].GetComponentsInChildren<PoseSetterBoneComponent>();

            for (int i = 0; i < originBC.Length; i++)
            {
                Transform ot = originBC[i].transform;
                targetBC[i].transform.rotation = new Quaternion(ot.rotation.x * -1.0f,
                                            ot.rotation.y,
                                            ot.rotation.z,
                                            ot.rotation.w * -1.0f);
            }
        }

        private Transform[] GetSymRoot(PoseEditorSkeleton skeleton, bool isFromLeft, SymmetryTarget symmetryTarget)
        {
            var origin_target = new Transform[2];
            switch (symmetryTarget)
            {
                case SymmetryTarget.Hands:
                    if (isFromLeft)
                    {
                        origin_target[0] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.LeftHand).transform;
                        origin_target[1] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.RightHand).transform;
                    }
                    else
                    {
                        origin_target[0] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.RightHand).transform;
                        origin_target[1] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.LeftHand).transform;
                    }
                    break;

                case SymmetryTarget.Arms:
                    if (isFromLeft)
                    {
                        origin_target[0] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.LeftUpperArm).transform;
                        origin_target[1] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.RightUpperArm).transform;
                    }
                    else
                    {
                        origin_target[0] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.RightUpperArm).transform;
                        origin_target[1] = skeleton.boneComponents.Find(bc => bc.BoneType == BoneType.LeftUpperArm).transform;
                    }
                    break;
            }

            return origin_target;
        }

        #endregion Symmetry
    }
}
#endif
