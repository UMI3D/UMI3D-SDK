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



using System.Collections.Generic;
using umi3d.cdk.collaboration;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class UserAvatar : MonoBehaviour
    {
        private struct OldPosition
        {
            public Transform oldParent;
            public Vector3 oldPosition;
            public Quaternion oldRotation;
        }

        public string userId { get; protected set; }
        public bool activeUserBindings { get; protected set; }
        public List<BoneBindingDto> userBindings { get; protected set; }

        Dictionary<string, OldPosition> oldPositions = new Dictionary<string, OldPosition>();

        /// <summary>
        /// Set a new UserAvatar from an UMI3DAvatarNodeDto.
        /// </summary>
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
                        if (oldPositions.ContainsKey(dto.objectId))
                            ResetParent(dto);
                }
            }

        }

        /// <summary>
        /// Set a new list of BoneBindingDto.
        /// </summary>
        public void SetBindings(List<BoneBindingDto> newBindings)
        {
            foreach (BoneBindingDto dto in userBindings)
                if (oldPositions.ContainsKey(dto.objectId))
                    ResetParent(dto);

            userBindings = newBindings;

            if (activeUserBindings && userBindings != null)
                foreach (BoneBindingDto dto in userBindings)
                    if (dto.active)
                        UpdateBindingPosition(dto);
        }

        /// <summary>
        /// Add a new BoneBindingDto at the given index.
        /// </summary>
        public void AddBinding(int index, BoneBindingDto dto)
        {
            userBindings.Insert(index, dto);
            if (activeUserBindings && dto.active)
                UpdateBindingPosition(dto);

            Debug.Log("Client Binding Created");
        }

        /// <summary>
        /// Remove the BoneBindingDto at the given index.
        /// </summary>
        public void RemoveBinding(int index, BoneBindingDto dto)
        {
            userBindings.RemoveAt(index);
            ResetParent(dto);

            Debug.Log("Client Binding Removed");
        }

        /// <summary>
        /// Update the BoneBindingDto at the given index.
        /// </summary>
        public void UpdateBinding(int index, BoneBindingDto dto)
        {
            userBindings[index] = dto;
            if (activeUserBindings)
            {
                if (dto.active)
                    UpdateBindingPosition(dto);
                else if (oldPositions.ContainsKey(dto.objectId))
                    ResetParent(dto);
            }

            Debug.Log("Client Binding Updated");
        }

        void UpdateBindingPosition(BoneBindingDto dto)
        {
            if (userId == UMI3DCollaborationClientServer.Identity.userId)
            {
                UMI3DClientUserTrackingBone bone = UMI3DClientUserTrackingBone.instances[dto.boneType];
                UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.objectId);

                if (node != null)
                {
                    if (!oldPositions.ContainsKey(dto.objectId))
                    {
                        OldPosition oldPosition = new OldPosition
                        {
                            oldParent = node.transform.parent,
                            oldPosition = node.transform.localPosition,
                            oldRotation = node.transform.localRotation
                        };

                        oldPositions.Add(dto.objectId, oldPosition);
                        node.transform.SetParent(bone.transform);
                    }

                    node.transform.localPosition = dto.position;
                    node.transform.localRotation = dto.rotation;
                }
            }
        }

        void ResetParent(BoneBindingDto dto)
        {
            if (userId == UMI3DCollaborationClientServer.Identity.userId)
            {
                UMI3DClientUserTrackingBone bone = UMI3DClientUserTrackingBone.instances[dto.boneType];
                UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.objectId);

                if (oldPositions.TryGetValue(dto.objectId, out OldPosition oldPosition))
                {
                    node.transform.SetParent(oldPosition.oldParent);
                    node.transform.localPosition = oldPosition.oldPosition;
                    node.transform.localRotation = oldPosition.oldRotation;
                }
                else
                    Destroy(node.gameObject);

                oldPositions.Remove(dto.objectId);
            }
        }

        /// <summary>
        /// Update the a UserAvatar directly sent by another client.
        /// </summary>
        public void UpdateBonePosition(UserTrackingFrameDto dto)
        {
            foreach (BoneDto boneDto in dto.bones)
            {
                BoneBindingDto b = userBindings.Find(binding => binding.boneType == boneDto.boneType);
                if (b != null)
                {
                    UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(b.objectId);
                    node.transform.localPosition = boneDto.position;
                    node.transform.localRotation = boneDto.rotation;
                    node.transform.localScale = boneDto.scale;
                }
            }
        }
    }
}
