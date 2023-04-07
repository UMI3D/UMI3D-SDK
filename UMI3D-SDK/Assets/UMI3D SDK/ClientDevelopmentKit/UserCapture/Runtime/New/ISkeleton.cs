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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using System;
using umi3d.cdk.utils.extrapolation;
using System.Threading.Tasks;
using System.Collections;
using umi3d.cdk;
using inetum.unityUtils;
using UnityEngine.UIElements;

namespace umi3d.cdk.userCapture
{
    public interface ISkeleton
    {
        protected const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;
        Dictionary<uint, s_Transform> Bones { get; set; }
        List<ISubSkeleton> Skeletons { get; set; }

        Dictionary<uint, (uint, Vector3)> SkeletonHierarchy { get; set; }

        protected Transform HipsAnchor { get; set; }
        
        #region Data struture
        public class s_Transform
        {
            public Vector3 s_Position;
            public Vector4 s_Rotation;
        }
        #endregion

        public abstract void UpdateFrame(UserTrackingFrameDto frame);

        /// <summary>
        /// Only Call once
        /// </summary>
        public void Init()
        {
            UMI3DSkeletonHierarchy.SetSkeletonHierarchy.AddListener(() => this.SkeletonHierarchy = UMI3DSkeletonHierarchy.SkeletonHierarchy);

            if (Bones == null) Bones = new Dictionary<uint, s_Transform>();
            if (Skeletons == null) Skeletons = new List<ISubSkeleton>();
            if (savedTransforms == null) savedTransforms = new Dictionary<ulong, SavedTransform>();
            if (bounds == null) bounds = new List<Bound>();

            //if (SkeletonHierarchy == null) SetSkeletonHierarchy();
        }

        #region Compute current skeleton
        public ISkeleton Compute()
        {
            if (CheckNulls())
            {
                return this;
            }

            foreach (ISubWritableSkeleton skeleton in Skeletons.OfType<ISubWritableSkeleton>().Reverse())
            {
                List<BoneDto> bones = new List<BoneDto>();

                try
                {
                    bones = skeleton.GetPose().bones.ToList();
                }
                catch (Exception e)
                {
                    Debug.Log($"<color=red> _{e} </color>");
                    return this;
                }

                bones.ForEach(b =>
                {
                    if (b.rotation != null)
                    {
                        Bones.TryGetValue(b.boneType, out var pose);
                        if (pose != null)
                        {
                            Bones[b.boneType].s_Rotation = b.rotation;
                        }
                        else
                        {
                            Bones.TryAdd(b.boneType, new s_Transform()
                            {
                                s_Rotation = b.rotation
                            });
                        }
                    }
                });

            }

            //very naïve
            Bones.Add(BoneType.Hips, new s_Transform());
            Bones[BoneType.Hips].s_Position = HipsAnchor != null ? HipsAnchor.position : Vector3.zero;

            foreach (uint boneType in Bones.Keys)
            {
                ComputeBonePosition(boneType);
            }

            return this;
        }

        private void ComputeBonePosition(uint boneType)
        {
            if (Bones[boneType].s_Position == null && SkeletonHierarchy.TryGetValue(boneType, out var pose))
            {
                if (Bones[pose.Item1].s_Position == null)
                    ComputeBonePosition(pose.Item1);

                Bones[boneType].s_Position = Bones[pose.Item1].s_Position + Bones[pose.Item1].s_Rotation.ToQuaternion() * pose.Item2;
            }
        }

        private bool CheckNulls()
        {
            if (Bones == null)
            {
                Bones = new Dictionary<uint, s_Transform>();
            }

            if (Skeletons == null || Skeletons.Count == 0)
            {
                return true;
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Has the user currently active bindings?
        /// </summary>
        public bool activeUserBindings { get; protected set; }
        /// <summary>
        /// User's registered id
        /// </summary>
        public ulong userId { get; protected set; }

        /// <summary>
        /// Extrapolator for the avatar position.
        /// </summary>
        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator { get; set; }

        /// <summary>
        /// Extrapolator for the avatar rotation.
        /// </summary>
        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator { get; set; }

        protected List<Bound> bounds { get; set; }
        public List<Bound> Bounds { get => bounds; }

        public List<Transform> boundRigs { get; set; }
        /// <summary>
        /// List of currently applied <see cref="BindingDto"/> to the user's skeleton.
        /// </summary>
        public List<BindingDto> userBindings { get; protected set; }
        /// <summary>
        /// Saves of the transform of objects before they had been bound to a user's bone.
        /// </summary>
        protected Dictionary<ulong, SavedTransform> savedTransforms { get; set; }
        public Dictionary<ulong, SavedTransform> SavedTransforms { get => savedTransforms; }

        #region Data Structure
        /// <summary>
        /// Represents a binding.
        /// </summary>
        /// Used mainly for computational purposes.
        public struct Bound
        {
            public uint bonetype;
            public Transform obj;
            public Vector3 offsetPosition;
            public Vector4 offsetRotation;
            public Vector3 offsetScale;
            public bool syncPos;
            public bool syncRot;
            public Quaternion anchorRelativeRot;
        }

        public struct SavedTransform
        {
            public Transform obj;
            public Vector3 savedPosition;
            public Quaternion savedRotation;
            public Vector3 savedLocalScale;
            public Vector3 savedLossyScale;
        }
        #endregion

        #region bindings
        #region Add/Remove
        /// <summary>
        /// Set the binding activation from the given bool.
        /// </summary>
        /// <param name="b">the activation value</param>
        public void SetActiveBindings(bool b)
        {
            activeUserBindings = b;

            if (userBindings != null)
            {
                if (activeUserBindings)
                {
                    foreach (BindingDto dto in userBindings)
                    {
                        if (dto.active)
                            UpdateBindingPosition(dto);
                    }
                }
                else
                {
                    foreach (BindingDto dto in userBindings)
                    {
                        if (savedTransforms.ContainsKey(dto.bindingId))
                            ResetObjectBindings(dto);
                    }
                }
            }
        }

        /// <summary>
        /// Set a new list of BoneBindingDto.
        /// </summary>
        /// <param name="newBindings">a list of dto containing binding data</param>
        public void SetBindings(List<BindingDto> newBindings)
        {
            foreach (BindingDto dto in userBindings)
            {
                if (savedTransforms.ContainsKey(dto.bindingId))
                    ResetObjectBindings(dto);
            }

            userBindings = newBindings;

            if (activeUserBindings && userBindings != null)
            {
                foreach (BindingDto dto in userBindings)
                {
                    if (dto.active)
                        UpdateBindingPosition(dto);
                }
            }
        }

        /// <summary>
        /// Add a new BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void AddBinding(int index, BindingDto boneBinding)
        {
            if (boneBinding == null) return;

            if (userBindings == null) userBindings = new List<BindingDto>();

            if (index <= userBindings.Count - 1)
            {
                BindingDto dtoAtIndex = userBindings[index];

                if (boneBinding.bindingId != dtoAtIndex.bindingId)
                {
                    AddBinding_(index, boneBinding);
                }
            }
            else
            {
                AddBinding_(index, boneBinding);
            }
        }

        private void AddBinding_(int index, BindingDto boneBinding)
        {
            userBindings.Insert(index, boneBinding);
            if (activeUserBindings && boneBinding.active)
            {
                UpdateBindingPosition(boneBinding);
            }
        }

        /// <summary>
        /// Remove the BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void RemoveBinding(int index)
        {
            if (userBindings == null) return;
            BindingDto dto = userBindings[index];
            userBindings.RemoveAt(index);
            ResetObjectBindings(dto);
        }

        #endregion

        #region Update/Await
        /// <summary>
        /// Update the BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void UpdateBinding(int index, BindingDto dto)
        {
            userBindings[index] = dto;
            if (activeUserBindings)
            {
                if (dto.active)
                    UpdateBindingPosition(dto);
                else if (savedTransforms.ContainsKey(dto.bindingId))
                    ResetObjectBindings(dto);
            }
        }

        protected void UpdateBindingPosition(BindingDto dto)
        {
            if (userId == UMI3DClientServer.Instance.GetUserId())
            {
                if (BoneSkeleton.bonSkeletonInstances.TryGetValue((dto.data as RigBindingDataDto).boneType, out BoneSkeleton bone))
                {
                    WaitForRig(dto, bone);
                }
                else
                {
                    UMI3DLogger.LogWarning((dto.data as RigBindingDataDto).boneType + "not found in bones instances", scope);
                }
            }
            else
            {
                WaitForOtherRig(dto);
            }
        }

        protected Transform InspectBoundRigs(BindingDto dto)
        {
            Transform obj = null;
            foreach (Transform rig in boundRigs)
            {
                if ((obj = rig.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == (dto.data as RigBindingDataDto).rigName)) != null)
                    return obj;
            }
            return obj;
        }

        /// <summary>
        /// Replace an object associated with a binding to its saved state.
        /// </summary>
        /// <param name="dto"></param>
        protected void ResetObjectBindings(BindingDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.bindingId);


            if (node != null)
            {
                if (savedTransforms.TryGetValue((dto.data as RigBindingDataDto).boneType, out SavedTransform savedTransform))
                {

                    ResetSavedTansform(dto, savedTransform, node);

                    if ((dto.data as RigBindingDataDto).rigName == "" && node != null)
                        node.updatePose = true;
                    else
                        boundRigs.Remove(savedTransform.obj);

                    savedTransforms.Remove(dto.bindingId);
                }
                else
                {
                    GameObject.Destroy(node.gameObject);
                }
            }

            if (!dto.active)
            {
                if (savedTransforms == null) return;

                savedTransforms.Remove(dto.bindingId);
            }
        }

        protected void ResetSavedTansform(BindingDto dto, SavedTransform savedTransform, UMI3DNodeInstance node)
        {
            if (savedTransform.obj != null)
            {
                int index = bounds.FindIndex(b => b.obj == savedTransform.obj);

                Bound bd = bounds[index];
                bounds.Remove(bd);

                if ((dto.data as RigBindingDataDto).rigName == "")
                {
                    if (node.dto is GlTFNodeDto)
                    {
                        savedTransform.obj.localPosition = (node.dto as GlTFNodeDto).position;
                        savedTransform.obj.localRotation = (node.dto as GlTFNodeDto).rotation;
                        savedTransform.obj.localScale = (node.dto as GlTFNodeDto).scale;
                    }
                    else if (node.dto is GlTFSceneDto)
                    {
                        savedTransform.obj.localPosition = (node.dto as GlTFSceneDto).extensions.umi3d.position;
                        savedTransform.obj.localRotation = (node.dto as GlTFSceneDto).extensions.umi3d.rotation;
                        savedTransform.obj.localScale = (node.dto as GlTFSceneDto).extensions.umi3d.scale;
                    }
                }
                else
                {
                    savedTransform.obj.localPosition = savedTransform.savedPosition;
                    savedTransform.obj.localRotation = savedTransform.savedRotation;
                    savedTransform.obj.localScale = savedTransform.savedLocalScale;
                }
            }
        }

        /// <summary>
        /// Called to reset the extrapolators. 
        /// </summary>
        /// E.g. when entering in a vehicle.
        public void ResetExtrapolators()
        {
            nodePositionExtrapolator = null;
            nodeRotationExtrapolator = null;
        }
        #endregion
        #region Waiting
        protected void WaitForRig(BindingDto dto, BoneSkeleton bone)
        {
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.bindingId, (entityInstance) =>
            {
                if (entityInstance is UMI3DNodeInstance node)
                {
                    UMI3DEnvironmentLoader.StartCoroutine(WaitForRig(node, dto, bone));
                }
            }
            );
        }

        protected void WaitForOtherRig(BindingDto dto)
        {
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.bindingId, (entityInstance) =>
            {
                if (entityInstance is UMI3DNodeInstance node)
                {
                    UMI3DEnvironmentLoader.StartCoroutine(WaitingForOtherEntityRig(node, dto));
                }
            });
        }

        protected IEnumerator WaitingForOtherEntityRig(UMI3DNodeInstance node, BindingDto dto)
        {
            yield return null;

            RigBindingDataDto rigDto = (dto.data as RigBindingDataDto);
            Transform obj = null;
            if (rigDto.rigName != "")
            {
                obj = node.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == (dto.data as RigBindingDataDto).rigName);
            }
            else
            {
                obj = node.transform;
            }

            AddANewBound(dto, obj, node);

            if (!savedTransforms.ContainsKey(dto.bindingId))
            {
                SaveTransform(dto, obj);
            }
        }

        protected IEnumerator WaitForRig(UMI3DNodeInstance node, BindingDto dto, BoneSkeleton bone)
        {
            RigBindingDataDto rigDto = (dto.data as RigBindingDataDto);
            Transform obj = null;
            if (rigDto.rigName != "")
            {
                while ((obj = node.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == (dto.data as RigBindingDataDto).rigName)) == null && (obj = InspectBoundRigs(dto)) == null)
                {
                    yield return null;
                }

                if (!boundRigs.Contains(obj))
                    boundRigs.Add(obj);

                SaveTransform(dto, obj, node);
            }
            else
            {
                obj = node.transform;
                SaveTransform(dto, obj, node);
            }
        }

        private void AddANewBound(BindingDto dto, Transform obj, UMI3DNodeInstance node)
        {
            if (dto == null || obj == null || node == null) return;
            RigBindingDataDto rigDto = dto.data as RigBindingDataDto;
            bounds.Add(new Bound()
            {
                bonetype = rigDto.boneType,
                obj = obj,
                offsetPosition = rigDto.offSetPosition,
                offsetRotation = rigDto.offSetRotation,
                offsetScale = rigDto.offSetScale,
                syncPos = rigDto.syncPosition,
                syncRot = rigDto.syncRotation,
                anchorRelativeRot = rigDto.rigName == "" ? Quaternion.identity : Quaternion.Inverse(node.transform.rotation) * obj.rotation
            });
        }

        private void SaveTransform(BindingDto dto, Transform obj, UMI3DNodeInstance node)
        {
            RigBindingDataDto rigDto = (dto.data as RigBindingDataDto);
            if (rigDto == null) return;
            if (savedTransforms.TryGetValue(dto.bindingId, out SavedTransform savedTransform))
            {

                int index = bounds.FindIndex(b => b.obj == savedTransform.obj && b.bonetype == rigDto.boneType);

                if (index >= 0)
                {
                    Bound bound = bounds[index];
                    bound.offsetPosition = rigDto.offSetPosition;
                    bound.offsetRotation = rigDto.offSetRotation;
                    bound.offsetScale = rigDto.offSetScale;
                    bound.syncPos = rigDto.syncPosition;
                    bound.syncRot = rigDto.syncRotation;
                    bounds[index] = bound;
                }
                else
                {
                    AddANewBound(dto, obj, node);
                }
            }
            else
            {
                SaveTransform(dto, obj);
                AddANewBound(dto, obj, node);

                if (rigDto.rigName == "")
                    node.updatePose = false;
            }
        }

        private void SaveTransform(BindingDto dto, Transform obj)
        {
            if (dto == null || obj == null) return;

            var savedTransform = new SavedTransform
            {
                obj = obj,
                savedPosition = obj.localPosition,
                savedRotation = obj.localRotation,
                savedLocalScale = obj.localScale,
                savedLossyScale = obj.lossyScale,
            };

            savedTransforms.Add(dto.bindingId, savedTransform);
        }

        #endregion
        #endregion
    }
}