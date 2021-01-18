﻿/*
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



using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class UserAvatar : MonoBehaviour
    {
        protected struct SavedTransform
        {
            public Transform obj;
            public Transform savedParent;
            public Vector3 savedPosition;
            public Quaternion savedRotation;
        }

        protected struct BoundObject
        {
            public string objectId;
            public string rigname;
        }

        public List<Transform> boundRigs = new List<Transform>();

        public string userId { get; protected set; }
        public bool activeUserBindings { get; protected set; }
        public List<BoneBindingDto> userBindings { get; protected set; }

        protected Dictionary<BoundObject, SavedTransform> savedTransforms = new Dictionary<BoundObject, SavedTransform>();

        /// <summary>
        /// Set a new UserAvatar from an UMI3DAvatarNodeDto.
        /// </summary>
        /// <param name="dto"></param>
        public void Set(UMI3DAvatarNodeDto dto)
        {
            userId = dto.userId;
            activeUserBindings = dto.activeBindings;
            userBindings = dto.bindings;

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
                        if (dto.active)
                            UpdateBindingPosition(dto);
                }
                else
                {
                    foreach (BoneBindingDto dto in userBindings)
                        if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                            ResetObject(dto);
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
                if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                    ResetObject(dto);

            userBindings = newBindings;

            if (activeUserBindings && userBindings != null)
                foreach (BoneBindingDto dto in userBindings)
                    if (dto.active)
                        UpdateBindingPosition(dto);
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
                AddBinding_(index, dto);
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
        public void RemoveBinding(int index, BoneBindingDto dto)
        {
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
            if (userId == UMI3DClientServer.Instance.GetId())
            {
                if (UMI3DClientUserTrackingBone.instances.TryGetValue(dto.boneType, out UMI3DClientUserTrackingBone bone))
                {

                    StartCoroutine(WaitForRig(dto, bone));

                }
                else
                    UnityEngine.Debug.LogWarning(dto.boneType + "not found in bones instances");
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

        protected IEnumerator WaitForRig(BoneBindingDto dto, UMI3DClientUserTrackingBone bone)
        {
            UMI3DNodeInstance node;
            var wait = new WaitForFixedUpdate();

            while ((node = UMI3DEnvironmentLoader.GetNode(dto.objectId)) == null)
            {
                yield return wait;
            }

            if (node != null)
            {
                Transform obj = null;
                if (dto.rigName != "")
                {
                    while ((obj = UMI3DEnvironmentLoader.GetNode(dto.objectId).transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName)) == null && (obj = InspectBoundRigs(dto)) == null)
                    {
                        yield return wait;
                    }

                    if (!boundRigs.Contains(obj))
                        boundRigs.Add(obj);
                }
                else
                    obj = node.transform;

                if (!savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                {
                    SavedTransform savedTransform = new SavedTransform
                    {
                        obj = obj,
                        savedParent = obj.parent,
                        savedPosition = obj.localPosition,
                        savedRotation = obj.localRotation
                    };

                    savedTransforms.Add(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, savedTransform);

                    obj.transform.SetParent(bone.transform);

                    if (dto.rigName == "")
                        node.updatePose = false;
                }

                obj.localPosition = dto.position;
                obj.localRotation = dto.rotation;
            }
        }

        protected void ResetObject(BoneBindingDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.objectId);

            if (node != null)
            {
                if (savedTransforms.TryGetValue(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, out SavedTransform savedTransform))
                {
                    if (savedTransform.obj != null)
                    {
                        savedTransform.obj.SetParent(savedTransform.savedParent);

                        if (dto.rigName == "")
                        {
                            if (node.dto is GlTFNodeDto)
                            {
                                savedTransform.obj.localPosition = (node.dto as GlTFNodeDto).position;
                                savedTransform.obj.localRotation = (node.dto as GlTFNodeDto).rotation;
                            }
                            else if (node.dto is GlTFSceneDto)
                            {
                                savedTransform.obj.localPosition = (node.dto as GlTFSceneDto).extensions.umi3d.position;
                                savedTransform.obj.localRotation = (node.dto as GlTFSceneDto).extensions.umi3d.rotation;
                            }
                        }
                        else
                        {
                            savedTransform.obj.localPosition = savedTransform.savedPosition;
                            savedTransform.obj.localRotation = savedTransform.savedRotation;
                        }
                    }

                    if (dto.rigName == "" && node != null)
                        node.updatePose = true;
                    else
                        boundRigs.Remove(savedTransform.obj);
                }
                else
                    Destroy(node.gameObject);
            }

            savedTransforms.Remove(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName });
        }

        /// <summary>
        /// Update the a UserAvatar directly sent by another client.
        /// </summary>
        /// <param name="trackingFrameDto">a dto containing the tracking data</param>
        public IEnumerator UpdateBonePosition(UserTrackingFrameDto trackingFrameDto)
        {

            foreach (BoneDto boneDto in trackingFrameDto.bones)
            {
                List<BoneBindingDto> bindings = userBindings.FindAll(binding => binding.boneType == boneDto.boneType);
                foreach (BoneBindingDto boneBindingDto in bindings)
                {

                    if (boneBindingDto.active)
                    {
                        UMI3DNodeInstance node;
                        var wait = new WaitForFixedUpdate();

                        while ((node = UMI3DEnvironmentLoader.GetNode(boneBindingDto.objectId)) == null)
                        {
                            yield return wait;
                        }

                        Transform obj = null;
                        if (boneBindingDto.rigName != "")
                        {
                            while ((obj = UMI3DEnvironmentLoader.GetNode(boneBindingDto.objectId).transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == boneBindingDto.rigName)) == null && (obj = InspectBoundRigs(boneBindingDto)) == null)
                            {
                                yield return wait;
                            }

                            if (!boundRigs.Contains(obj))
                                boundRigs.Add(obj);
                        }
                        else
                            obj = node.transform;

                        if (!savedTransforms.ContainsKey(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }))
                        {
                            SavedTransform savedTransform = new SavedTransform
                            {
                                obj = obj,
                                savedParent = obj.parent,
                                savedPosition = obj.localPosition,
                                savedRotation = obj.localRotation
                            };

                            savedTransforms.Add(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }, savedTransform);

                            obj.transform.SetParent(UMI3DEnvironmentLoader.Instance.transform);

                            if (boneBindingDto.rigName == "")
                                node.updatePose = false;

                        }

                        obj.position = Matrix4x4.TRS(trackingFrameDto.position, trackingFrameDto.rotation, trackingFrameDto.scale).MultiplyPoint3x4((Vector3)boneDto.position + (Vector3)boneBindingDto.position * boneDto.scale.X);
                        obj.rotation = (Quaternion)trackingFrameDto.rotation * (Quaternion)boneDto.rotation * (Quaternion)boneBindingDto.rotation;

                        if (boneBindingDto.rigName == "")
                        {
                            node.updatePose = false;
                        }

                    }
                }
            }
        }
    }
}
