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

using inetum.unityUtils;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Base node for an avatar.
    /// </summary>
    public class UMI3DAvatarNode : UMI3DNode
    {
        /// <summary>
        /// Avatar's user id.
        /// </summary>
        [SerializeField, Tooltip("Avatar's user id.")]
        public ulong userId;

        /// <summary>
        /// If true, the avatar could receive bindings.
        /// </summary>
        [SerializeField, EditorReadOnly,
            Tooltip("If true, the avatar could receive bindings.")]
        private bool activeAvatarBindings_ = true;

        /// <summary>
        /// Dictionnary of every <see cref="UMI3DUserEmbodimentBone"/> indexed by their standard id in <see cref="BoneType"/>.
        /// </summary>
         [Tooltip("Dictionnary of every bone indexed by their standard id.")]
        public Dictionary<uint, UMI3DUserEmbodimentBone> dicoBones = new Dictionary<uint, UMI3DUserEmbodimentBone>();

        /// <summary>
        /// Animator of the skeleton of the avatar.
        /// </summary>
        public Animator skeletonAnimator;

        public UserCameraPropertiesDto userCameraPropertiesDto;

        public UMI3DAsyncListProperty<UMI3DBinding> bindings { get { Register(); return _bindings; } protected set => _bindings = value; }
        public UMI3DAsyncProperty<bool> activeBindings { get { Register(); return _activeBindings; } protected set => _activeBindings = value; }

        public class OnActivationValueChanged : UnityEvent<ulong, bool> { };

        public static OnActivationValueChanged onActivationValueChanged = new OnActivationValueChanged();
        private UMI3DAsyncListProperty<UMI3DBinding> _bindings;
        private UMI3DAsyncProperty<bool> _activeBindings;

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            activeBindings = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.ActiveBindings, activeAvatarBindings_);
            activeBindings.OnValueChanged += (b) =>
            {
                activeAvatarBindings_ = b;
            };

            bindings = new UMI3DAsyncListProperty<UMI3DBinding>(Id(), UMI3DPropertyKeys.UserBindings, new List<UMI3DBinding>(), (b, u) => b.ToDto(u));
        }

        #region UserTracking

        /// <summary>
        /// Update an embodiment from the given DTO.
        /// </summary>
        public void UpdateEmbodiment(UserTrackingFrameDto dto)
        {
            List<BoneDto> newBoneList = dto.bones;

            var oldBoneList = new List<UMI3DUserEmbodimentBone>();

            foreach (KeyValuePair<uint, UMI3DUserEmbodimentBone> pair in dicoBones)
                oldBoneList.Add(pair.Value);

            var bonesToCreate = new List<BoneDto>();
            var bonesToUpdate = new List<BoneDto>();
            var bonesToDelete = new List<UMI3DUserEmbodimentBone>();

            bonesToCreate = newBoneList.FindAll(newBoneDto => !dicoBones.ContainsKey(newBoneDto.boneType));
            bonesToUpdate = newBoneList.FindAll(newBoneDto => dicoBones.ContainsKey(newBoneDto.boneType));
            bonesToDelete = oldBoneList.FindAll(embodimentBone => !newBoneList.Exists(newBoneDto => newBoneDto.boneType.Equals(embodimentBone.boneType)));

            foreach (BoneDto boneDto in bonesToCreate)
            {
                RegisterBone(boneDto);
            }

            foreach (BoneDto boneDto in bonesToUpdate)
            {
                UMI3DUserEmbodimentBone embodimentBone = dicoBones[boneDto.boneType];
                UpdateBone(embodimentBone, boneDto);
                UMI3DEmbodimentManager.Instance.UpdateEvent.Invoke(embodimentBone);
            }

            foreach (UMI3DUserEmbodimentBone embodimentBone in bonesToDelete)
            {
                UnregisterBone(embodimentBone);
            }
        }

        private void RegisterBone(BoneDto dto)
        {
            UMI3DUserEmbodimentBone uMI3DUserEmbodimentBone = InstanciateBone(dto);
            dicoBones.Add(dto.boneType, uMI3DUserEmbodimentBone);
            UpdateBone(uMI3DUserEmbodimentBone, dto);
            UMI3DEmbodimentManager.Instance.CreationEvent.Invoke(uMI3DUserEmbodimentBone);
        }

        private void UnregisterBone(UMI3DUserEmbodimentBone embodimentBone)
        {
            UMI3DEmbodimentManager.Instance.DeletionEvent.Invoke(embodimentBone);
            dicoBones.Remove(embodimentBone.boneType);
        }

        /// <summary>
        /// Instanciate a new bone from a DTO.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private UMI3DUserEmbodimentBone InstanciateBone(BoneDto dto)
        {
            if (dto.boneType != BoneType.None)
            {
                var embodimentBone = new UMI3DUserEmbodimentBone(userId, dto.boneType);
                return embodimentBone;
            }
            return null;
        }

        /// <summary>
        /// Update a bone from a received DTO.
        /// </summary>
        /// <param name="embodimentBone"></param>
        /// <param name="dto"></param>
        private void UpdateBone(UMI3DUserEmbodimentBone embodimentBone, BoneDto dto)
        {
            embodimentBone.spatialPosition = new UMI3DUserEmbodimentBone.SpatialPosition()
            {
                localRotation = dto.rotation,
            };

            embodimentBone.isTracked = UMI3DEmbodimentManager.Instance.BoneTrackedInformation(userId, dto.boneType);
            Transform transform;

            if (dto.boneType != BoneType.Viewpoint)
                transform = skeletonAnimator.GetBoneTransform(dto.boneType.ConvertToBoneType().GetValueOrDefault());

            else
                transform = GetComponentInChildren<UMI3DAvatarViewpointHelper>()?.transform;

            if (transform != null)
                transform.localRotation = dto.rotation;
        }

        #endregion

        /// <inheritdoc/>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DAvatarNodeDto();
        }

        /// <inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var avatarNodeDto = dto as UMI3DAvatarNodeDto;

            avatarNodeDto.userId = userId;
            avatarNodeDto.activeBindings = activeBindings.GetValue(user);

            if (UMI3DEmbodimentManager.Instance.embodimentSize.ContainsKey(userId))
                avatarNodeDto.userSize = UMI3DEmbodimentManager.Instance.embodimentSize[userId];
            else
            {
                UMI3DLogger.LogError("EmbodimentSize does not contain key " + user.Id(), DebugScope.EDK);
            }

            var bindingDtoList = new List<BoneBindingDto>();

            foreach (UMI3DBinding item in bindings.GetValue(user))
            {
                bindingDtoList.Add(item.ToDto(user));
            }

            avatarNodeDto.bindings = bindingDtoList;

            UMI3DEmbodimentManager.Instance.WriteNodeCollections(avatarNodeDto, user);
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return
                base.ToBytes(user)
                + UMI3DSerializer.Write(userId)
                + UMI3DSerializer.Write(activeBindings.GetValue(user))
                + UMI3DSerializer.WriteIBytableCollection(bindings.GetValue(user), user);
        }
    }
}
