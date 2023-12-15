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
        public const string ROOT_NAME = "SkeletonRoot";

        #region Skeleton

        /// <summary>
        /// This should be called each time you change the skeleton prefab
        /// Reads all of the PoseSetterBineComponent which are not None Bones and add them in the treeView
        /// </summary>
        /// <param name="value"></param>
        public void UpdateSkeletonGameObject(PoseEditorSkeleton skeleton, HandClosureSkeleton handClosureSkeleton, GameObject value)
        {
            skeleton.root = value.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name == ROOT_NAME)?.gameObject;

            if (skeleton.root == null)
            {
                UnityEngine.Debug.LogWarning("Invalid root. Check naming.");
                return;
            }

            skeleton.boneComponents = skeleton.root.GetComponentsInChildren<PoseSetterBoneComponent>()
                                                            .Where(bc => bc.BoneType != BoneType.None)
                                                            .ToList();

            handClosureSkeleton.handClosureAnimator = value.GetComponentsInChildren<Animator>().First(x => x.runtimeAnimatorController != null);
            handClosureSkeleton.root = handClosureSkeleton.handClosureAnimator.gameObject;
            handClosureSkeleton.boneComponents = handClosureSkeleton.root.GetComponentsInChildren<PoseSetterBoneComponent>()
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

        #endregion Skeleton

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

        #endregion Rotations

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
            var (originRoot, targetRoot) = GetSymRoots(skeleton, isFromLeft, symmetryTarget);

            ComputeSymmetry(originRoot, targetRoot);
        }

        private void ComputeSymmetry(Transform originRoot, Transform targetRoot)
        {
            PoseSetterBoneComponent[] originBC = originRoot.gameObject.GetComponentsInChildren<PoseSetterBoneComponent>();
            PoseSetterBoneComponent[] targetBC = targetRoot.GetComponentsInChildren<PoseSetterBoneComponent>();

            for (int i = 0; i < originBC.Length; i++)
            {
                Transform ot = originBC[i].transform;
                targetBC[i].transform.rotation = new Quaternion(ot.rotation.x * -1.0f,
                                            ot.rotation.y,
                                            ot.rotation.z,
                                            ot.rotation.w * -1.0f);
            }
        }

        private (Transform originRoot, Transform targetRoot) GetSymRoots(PoseEditorSkeleton skeleton, bool isFromLeft, SymmetryTarget symmetryTarget)
        {
            (Transform originRoot, Transform targetRoot) result = (null, null);

            (uint origin, uint target) symParameters;

            if (symmetryTarget == SymmetryTarget.Hands)
                symParameters = isFromLeft ? (BoneType.LeftHand, BoneType.RightHand) : (BoneType.RightHand, BoneType.LeftHand);
            else
                symParameters = isFromLeft ? (BoneType.LeftUpperArm, BoneType.RightUpperArm) : (BoneType.RightUpperArm, BoneType.LeftUpperArm);

            result.originRoot = skeleton.boneComponents.Find(bc => bc.BoneType == symParameters.origin).transform;
            result.targetRoot = skeleton.boneComponents.Find(bc => bc.BoneType == symParameters.target).transform;

            return result;
        }

        #endregion Symmetry

        #region CloseHand

        /// <summary>
        ///  Close a fingers group on the pose skeleton using a hand closure skeleton equipped with an animator
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="handClosureSkeleton"></param>
        /// <param name="handBoneType">Hand that is closed</param>
        /// <param name="fingerGroup">Fingers group closed</param>
        /// <param name="closureRate">Closure rate between 0 and 1</param>
        public void CloseFinger(PoseEditorSkeleton skeleton, HandClosureSkeleton handClosureSkeleton, uint handBoneType, HandClosureGroup fingerGroup, float closureRate)
        {
            int chosenLayer = (int)fingerGroup + 1; // skip default layer
            int[] otherLayers = Enumerable.Range(0, handClosureSkeleton.handClosureAnimator.layerCount).Where(x => x != chosenLayer).ToArray(); // others layers on animator. 4 in total (default + 3 groups)
            foreach (int layer in otherLayers)
            {
                handClosureSkeleton.handClosureAnimator.SetLayerWeight(layer, 0);
            }
            handClosureSkeleton.handClosureAnimator.SetLayerWeight(chosenLayer, 1);
            handClosureSkeleton.handClosureAnimator.Play(stateName: fingerGroup.ToString(), layer: chosenLayer, normalizedTime: closureRate);
            handClosureSkeleton.handClosureAnimator.Update(Time.deltaTime);
            handClosureSkeleton.handClosureAnimator.speed = 0;

            bool isOnLeftHand = handBoneType == BoneType.LeftHand;

            switch (fingerGroup)
            {
                case HandClosureGroup.THUMB or HandClosureGroup.INDEX:
                    {
                        uint rootBoneType = fingerGroup == HandClosureGroup.THUMB ? BoneType.RightThumbProximal : BoneType.RightIndexProximal;
                        CopyFingerMovement(skeleton, handClosureSkeleton, rootBoneType, isOnLeftHand);
                        break;
                    }
                case HandClosureGroup.MEDIAL_GROUP:
                    {
                        uint[] rootsBoneType = new uint[3] { BoneType.RightMiddleProximal, BoneType.RightRingProximal, BoneType.RightLittleProximal };
                        foreach (uint rootBoneType in rootsBoneType)
                        {
                            CopyFingerMovement(skeleton, handClosureSkeleton, rootBoneType, isOnLeftHand);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Copy the finger movement from the handClosureSkeleton to the edited pose skeleton.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="handClosureSkeleton"></param>
        /// <param name="rootRightFingerBoneType"></param>
        /// <param name="isOnLeftHand"></param>
        private void CopyFingerMovement(PoseEditorSkeleton skeleton, HandClosureSkeleton handClosureSkeleton, uint rootRightFingerBoneType, bool isOnLeftHand)
        {
            PoseSetterBoneComponent handRoot, handClosureRoot;

            if (!isOnLeftHand)
            {
                handRoot = skeleton.boneComponents.First(x => x.BoneType == rootRightFingerBoneType);
                handClosureRoot = handClosureSkeleton.boneComponents.First(x => x.BoneType == rootRightFingerBoneType);
            }
            else
            {
                uint symmetricalLeftBoneType = BoneTypeHelper.GetSymmetricBoneType(rootRightFingerBoneType);
                handRoot = skeleton.boneComponents.First(x => x.BoneType == symmetricalLeftBoneType);
                PoseSetterBoneComponent handClosureRootRight = handClosureSkeleton.boneComponents.First(x => x.BoneType == rootRightFingerBoneType);
                handClosureRoot = handClosureSkeleton.boneComponents.First(x => x.BoneType == symmetricalLeftBoneType);
                ComputeSymmetry(handClosureRootRight.transform, handClosureRoot.transform);
            }
            CopyLocalRotationHierarchy(handRoot.transform, handClosureRoot.transform);
        }

        /// <summary>
        /// Copy the hierarchy of local rotations.
        /// </summary>
        /// <param name="HandNode"></param>
        /// <param name="HandClosureNode"></param>
        private void CopyLocalRotationHierarchy(Transform HandNode, Transform HandClosureNode)
        {
            HandNode.localRotation = HandClosureNode.localRotation;
            foreach (Transform child in HandNode)
            {
                Transform correspondingClosureChild = HandClosureNode.Find(child.name);
                CopyLocalRotationHierarchy(child, correspondingClosureChild);
            }
        }

        #endregion CloseHand
    }
}
#endif