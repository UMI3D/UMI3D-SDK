/*
Copyright 2019 - 2021 Inetum

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



using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Client form of a user's avatar, the virtual representation of the usr in the environment.
    /// </summary>
    public class UserAvatar : MonoBehaviour
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        #region Struct definition

        /// <summary>
        /// Saved state of an object properties before being bound to a user's bone.
        /// </summary>
        protected struct SavedTransform
        {
            public Transform obj;
            public Vector3 savedPosition;
            public Quaternion savedRotation;
            public Vector3 savedLocalScale;
            public Vector3 savedLossyScale;
        }

        /// <summary>
        /// Object that has been bound through a bone binding.
        /// </summary>
        protected struct BoundObject
        {
            public ulong objectId;
            public string rigname;
        }

        /// <summary>
        /// Represents a binding.
        /// </summary>
        /// Used mainly for computational purposes.
        protected struct Bound
        {
            public uint bonetype;
            public Transform obj;
            public Vector3 offsetPosition;
            public Quaternion offsetRotation;
            public Vector3 offsetScale;
            public bool syncPos;
            public bool syncRot;
            public bool freezeWorldScale;
            public Vector3 frozenLossyScale;
            public Quaternion anchorRelativeRot;
        }

        #endregion

        #region Fields

        public List<Transform> boundRigs = new List<Transform>();
        /// <summary>
        /// User's registered id
        /// </summary>
        public ulong userId { get; protected set; }
        /// <summary>
        /// User's size.
        /// </summary>
        public Vector3 userSize { get; protected set; }
        /// <summary>
        /// Has the user currently active bindings?
        /// </summary>
        public bool activeUserBindings { get; protected set; }
        /// <summary>
        /// List of currently applied <see cref="BoneBindingDto"/> to the user's skeleton.
        /// </summary>
        public List<BoneBindingDto> userBindings { get; protected set; }

        /// <summary>
        /// Saves of the transform of objects before they had been bound to a user's bone.
        /// </summary>
        protected Dictionary<BoundObject, SavedTransform> savedTransforms = new Dictionary<BoundObject, SavedTransform>();

        protected readonly List<Bound> bounds = new List<Bound>();

        protected Transform viewpointObject;

        protected UMI3DKalmanVector3Lerp nodePositionLerp;
        protected UMI3DKalmanQuaternionLerp nodeRotationLerp;

        protected float MeasuresPerSecond = 0;
        protected float lastFrameTime = 0;
        protected float lastMessageTime = 0;

        #endregion

        #region Methods

        private void OnTransformParentChanged()
        {
            nodePositionLerp = null;
            nodeRotationLerp = null;
        }

        private void Start()
        {
            viewpointObject = UMI3DClientUserTracking.Instance.viewpoint;
        }

        private void Update()
        {
            this.transform.position = UMI3DClientUserTracking.Instance.transform.position;
            this.transform.rotation = UMI3DClientUserTracking.Instance.transform.rotation;

            foreach (Bound item in bounds)
            {
                if (item.obj != null)
                {
                    if (item.bonetype.Equals(BoneType.CenterFeet))
                    {
                        if (item.syncPos)
                            item.obj.position = UMI3DClientUserTracking.Instance.skeletonContainer.TransformPoint(item.offsetPosition);
                        if (item.syncRot)
                            item.obj.rotation = UMI3DClientUserTracking.Instance.skeletonContainer.rotation * item.anchorRelativeRot * item.offsetRotation;
                    }
                    else if (item.bonetype.Equals(BoneType.Viewpoint))
                    {
                        if (item.syncPos)
                            item.obj.position = viewpointObject.TransformPoint(item.offsetPosition);
                        if (item.syncRot)
                            item.obj.rotation = viewpointObject.rotation * item.anchorRelativeRot * item.offsetRotation;
                    }
                    else
                    {
                        if (item.syncPos)
                            item.obj.position = UMI3DClientUserTracking.Instance.GetComponentInChildren<Animator>().GetBoneTransform(item.bonetype.ConvertToBoneType().GetValueOrDefault()).TransformPoint(item.offsetPosition);
                        if (item.syncRot)
                            item.obj.rotation = UMI3DClientUserTracking.Instance.GetComponentInChildren<Animator>().GetBoneTransform(item.bonetype.ConvertToBoneType().GetValueOrDefault()).rotation * item.anchorRelativeRot * item.offsetRotation;
                    }

                    if (item.freezeWorldScale)
                    {
                        Vector3 WscaleMemory = item.frozenLossyScale;
                        Vector3 WScaleParent = item.obj.parent.lossyScale;

                        item.obj.localScale = new Vector3(WscaleMemory.x / WScaleParent.x, WscaleMemory.y / WScaleParent.y, WscaleMemory.z / WScaleParent.z) + item.offsetScale;
                    }
                }
            }
        }

        /// <summary>
        /// Set a new UserAvatar from an UMI3DAvatarNodeDto.
        /// </summary>
        /// <param name="dto"></param>
        public void Set(UMI3DAvatarNodeDto dto)
        {
            userId = dto.userId;
            userSize = dto.userSize;
            activeUserBindings = dto.activeBindings;
            userBindings = dto.bindings;

            if (dto.handPoses != null)
                foreach (UMI3DHandPoseDto pose in dto.handPoses)
                    UMI3DEnvironmentLoader.RegisterEntityInstance(pose.id, pose, null).NotifyLoaded();

            if (dto.bodyPoses != null)
                foreach (UMI3DBodyPoseDto pose in dto.bodyPoses)
                    UMI3DEnvironmentLoader.RegisterEntityInstance(pose.id, pose, null).NotifyLoaded();

            if (dto.emotesConfigDto != null)
                UMI3DEmotesConfigLoader.Load(dto.emotesConfigDto);

            if (activeUserBindings && userBindings != null)
            {
                foreach (BoneBindingDto boneDto in userBindings)
                {
                    if (boneDto.active)
                        UpdateBindingPosition(boneDto);
                }
            }
        }

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
                    foreach (BoneBindingDto dto in userBindings)
                    {
                        if (dto.active)
                            UpdateBindingPosition(dto);
                    }
                }
                else
                {
                    foreach (BoneBindingDto dto in userBindings)
                    {
                        if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                            ResetObject(dto);
                    }
                }
            }
        }

        /// <summary>
        /// Set a new list of BoneBindingDto.
        /// </summary>
        /// <param name="newBindings">a list of dto containing binding data</param>
        public void SetBindings(List<BoneBindingDto> newBindings)
        {
            foreach (BoneBindingDto dto in userBindings)
            {
                if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                    ResetObject(dto);
            }

            userBindings = newBindings;

            if (activeUserBindings && userBindings != null)
            {
                foreach (BoneBindingDto dto in userBindings)
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
        public void AddBinding(int index, BoneBindingDto dto)
        {
            if (index <= userBindings.Count - 1)
            {
                BoneBindingDto dtoAtIndex = userBindings[index];

                if (!dto.bindingId.Contains(dtoAtIndex.bindingId) && !dtoAtIndex.bindingId.Contains(dto.bindingId))
                    AddBinding_(index, dto);
            }
            else
            {
                AddBinding_(index, dto);
            }
        }

        protected void AddBinding_(int index, BoneBindingDto dto)
        {
            userBindings.Insert(index, dto);
            if (activeUserBindings && dto.active)
                UpdateBindingPosition(dto);
        }

        /// <summary>
        /// Remove the BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void RemoveBinding(int index)
        {
            BoneBindingDto dto = userBindings[index];
            userBindings.RemoveAt(index);
            ResetObject(dto);
        }

        /// <summary>
        /// Update the BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void UpdateBinding(int index, BoneBindingDto dto)
        {
            userBindings[index] = dto;
            if (activeUserBindings)
            {
                if (dto.active)
                    UpdateBindingPosition(dto);
                else if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                    ResetObject(dto);
            }
        }

        protected void UpdateBindingPosition(BoneBindingDto dto)
        {
            if (userId == UMI3DClientServer.Instance.GetUserId())
            {
                if (UMI3DClientUserTrackingBone.instances.TryGetValue(dto.boneType, out UMI3DClientUserTrackingBone bone))
                {
                    WaitForRig(dto, bone);
                }
                else
                {
                    UMI3DLogger.LogWarning(dto.boneType + "not found in bones instances", scope);
                }
            }
            else
            {
                WaitForOtherRig(dto);
            }
        }

        protected Transform InspectBoundRigs(BoneBindingDto dto)
        {
            Transform obj = null;
            foreach (Transform rig in boundRigs)
            {
                if ((obj = rig.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName)) != null)
                    return obj;
            }
            return obj;
        }

        protected void WaitForOtherRig(BoneBindingDto dto)
        {
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.objectId, (e) =>
            {
                StartCoroutine(WaitingForOtherEntityRig(e as UMI3DNodeInstance, dto));
            });
        }

        protected IEnumerator WaitingForOtherEntityRig(UMI3DNodeInstance node, BoneBindingDto dto)
        {
            if (node == null)
                yield break;

            yield return null;

            Transform obj = null;
            if (dto.rigName != "")
            {
                obj = node.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName);
            }
            else
            {
                obj = node.transform;
            }

            bounds.Add(new Bound()
            {
                bonetype = dto.boneType,
                obj = obj,
                offsetPosition = dto.offsetPosition,
                offsetRotation = dto.offsetRotation,
                offsetScale = dto.offsetScale,
                syncPos = dto.syncPosition,
                syncRot = dto.syncRotation,
                freezeWorldScale = dto.freezeWorldScale,
                frozenLossyScale = obj.lossyScale,
                anchorRelativeRot = dto.rigName == "" ? Quaternion.identity : Quaternion.Inverse(node.transform.rotation) * obj.rotation
            });

            if (!savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
            {
                var savedTransform = new SavedTransform
                {
                    obj = obj,
                    savedPosition = obj.localPosition,
                    savedRotation = obj.localRotation,
                    savedLocalScale = obj.localScale,
                    savedLossyScale = obj.lossyScale,
                };

                savedTransforms.Add(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, savedTransform);
            }
        }

        protected void WaitForRig(BoneBindingDto dto, UMI3DClientUserTrackingBone bone)
        {
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.objectId, (e) =>
            {
                if (e is UMI3DNodeInstance node)
                {
                    StartCoroutine(WaitForRig(node, dto, bone));
                }
            }
            );
        }

        protected IEnumerator WaitForRig(UMI3DNodeInstance node, BoneBindingDto dto, UMI3DClientUserTrackingBone bone)
        {
            Transform obj = null;
            if (dto.rigName != "")
            {
                while ((obj = node.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName)) == null && (obj = InspectBoundRigs(dto)) == null)
                {
                    yield return null;
                }

                if (!boundRigs.Contains(obj))
                    boundRigs.Add(obj);
            }
            else
            {
                obj = node.transform;
            }

            if (!savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
            {
                var savedTransform = new SavedTransform
                {
                    obj = obj,
                    savedPosition = obj.localPosition,
                    savedRotation = obj.localRotation,
                    savedLocalScale = obj.localScale,
                    savedLossyScale = obj.lossyScale,
                };

                savedTransforms.Add(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, savedTransform);

                bounds.Add(new Bound()
                {
                    bonetype = dto.boneType,
                    obj = obj,
                    offsetPosition = dto.offsetPosition,
                    offsetRotation = dto.offsetRotation,
                    offsetScale = dto.offsetScale,
                    syncPos = dto.syncPosition,
                    syncRot = dto.syncRotation,
                    freezeWorldScale = dto.freezeWorldScale,
                    frozenLossyScale = obj.lossyScale,
                    anchorRelativeRot = dto.rigName == "" ? Quaternion.identity : Quaternion.Inverse(node.transform.rotation) * obj.rotation
                });

                if (dto.rigName == "")
                    node.updatePose = false;
            }
            else
            {
                if (savedTransforms.TryGetValue(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, out SavedTransform savedTransform))
                {
                    int index = bounds.FindIndex(b => b.obj == savedTransform.obj && b.bonetype == dto.boneType);

                    if (index >= 0)
                    {
                        Bound bound = bounds[index];
                        bound.offsetPosition = dto.offsetPosition;
                        bound.offsetRotation = dto.offsetRotation;
                        bound.offsetScale = dto.offsetScale;
                        bound.syncPos = dto.syncPosition;
                        bound.syncRot = dto.syncRotation;
                        bound.freezeWorldScale = dto.freezeWorldScale;
                        bounds[index] = bound;
                    }
                    else
                    {
                        bounds.Add(new Bound()
                        {
                            bonetype = dto.boneType,
                            obj = obj,
                            offsetPosition = dto.offsetPosition,
                            offsetRotation = dto.offsetRotation,
                            offsetScale = dto.offsetScale,
                            syncPos = dto.syncPosition,
                            syncRot = dto.syncRotation,
                            freezeWorldScale = dto.freezeWorldScale,
                            frozenLossyScale = obj.lossyScale,
                            anchorRelativeRot = dto.rigName == "" ? Quaternion.identity : Quaternion.Inverse(node.transform.rotation) * obj.rotation
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Replace an object associated with a binding to its saved state.
        /// </summary>
        /// <param name="dto"></param>
        protected void ResetObject(BoneBindingDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.objectId);

            if (node != null)
            {
                if (savedTransforms.TryGetValue(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, out SavedTransform savedTransform))
                {
                    if (savedTransform.obj != null)
                    {
                        int index = bounds.FindIndex(b => b.obj == savedTransform.obj);

                        Bound bd = bounds[index];
                        bounds.Remove(bd);

                        if (dto.rigName == "")
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

                    if (dto.rigName == "" && node != null)
                        node.updatePose = true;
                    else
                        boundRigs.Remove(savedTransform.obj);

                    savedTransforms.Remove(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName });
                }
                else
                {
                    Destroy(node.gameObject);
                }
            }

            if (!dto.active)
            {
                savedTransforms.Remove(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName });
            }
        }

        #endregion
    }
}
