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

using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.userCapture
{
    public class UMI3DAvatarNode : UMI3DNode
    {
        [SerializeField]
        public ulong userId;

        [SerializeField, EditorReadOnly]
        bool activeAvatarBindings_ = true;

        public Dictionary<string, UMI3DUserEmbodimentBone> dicoBones = new Dictionary<string, UMI3DUserEmbodimentBone>();

        public UserCameraPropertiesDto userCameraPropertiesDto;

        public UMI3DAsyncListProperty<UMI3DBinding> bindings { get { Register(); return _bindings; } protected set => _bindings = value; }
        public UMI3DAsyncProperty<bool> activeBindings { get { Register(); return _activeBindings; } protected set => _activeBindings = value; }

        public class OnActivationValueChanged : UnityEvent<ulong, bool> { };

        public static OnActivationValueChanged onActivationValueChanged = new OnActivationValueChanged();
        private UMI3DAsyncListProperty<UMI3DBinding> _bindings;
        private UMI3DAsyncProperty<bool> _activeBindings;

        ///<inheritdoc/>
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
        /// Update an embodiment from the given dto.
        /// </summary>
        public void UpdateEmbodiment(UserTrackingFrameDto dto)
        {
            List<BoneDto> newBoneList = dto.bones;

            List<UMI3DUserEmbodimentBone> oldBoneList = new List<UMI3DUserEmbodimentBone>();

            foreach (KeyValuePair<string, UMI3DUserEmbodimentBone> pair in dicoBones)
                oldBoneList.Add(pair.Value);

            List<BoneDto> bonesToCreate = new List<BoneDto>();
            List<BoneDto> bonesToUpdate = new List<BoneDto>();
            List<UMI3DUserEmbodimentBone> bonesToDelete = new List<UMI3DUserEmbodimentBone>();

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

        private UMI3DUserEmbodimentBone InstanciateBone(BoneDto dto)
        {
            if (dto.boneType != BoneType.None)
            {
                UMI3DUserEmbodimentBone embodimentBone = new UMI3DUserEmbodimentBone(userId, dto.boneType);
                return embodimentBone;
            }
            return null;
        }

        private void UpdateBone(UMI3DUserEmbodimentBone embodimentBone, BoneDto dto)
        {
            embodimentBone.spatialPosition = new UMI3DUserEmbodimentBone.SpatialPosition()
            {
                localPosition = dto.position,
                localRotation = dto.rotation,
                localScale = Vector3.Scale(dto.scale, UMI3DEmbodimentManager.Instance.embodimentSize[this.userId])
            };
            embodimentBone.isTracked = UMI3DEmbodimentManager.Instance.BoneTrackedInformation(userId, dto.boneType);
        }

        #endregion

        ///<inheritdoc/>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DAvatarNodeDto();
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            UMI3DAvatarNodeDto avatarNodeDto = dto as UMI3DAvatarNodeDto;

            avatarNodeDto.userId = userId;
            avatarNodeDto.activeBindings = activeBindings.GetValue(user);

            List<BoneBindingDto> bindingDtoList = new List<BoneBindingDto>();

            foreach (var item in bindings.GetValue(user))
            {
                bindingDtoList.Add(item.ToDto(user));
            }

            avatarNodeDto.bindings = bindingDtoList;
        }

        public override (int, Func<byte[], int, int>) ToBytes(UMI3DUser user)
        {
            var fp = base.ToBytes(user);

            int size = 2 * sizeof(bool)
                + UMI3DNetworkingHelper.GetSize(idGenerator)
                + fm.Item1
                + fp.Item1;
            Func<byte[], int, int> func = (b, i) => {
                i += UMI3DNetworkingHelper.Write(areSubobjectsTracked, b, i);
                i += UMI3DNetworkingHelper.Write(areSubobjectsTracked ? isRightHanded : true, b, i);
                i += UMI3DNetworkingHelper.Write(idGenerator, b, i);
                i += UMI3DNetworkingHelper.Write(isPartOfNavmesh, b, i);
                i += UMI3DNetworkingHelper.Write(isTraversable, b, i);
                i += fm.Item2(b, i);
                i += fp.Item2(b, i);
                return size;
            };


        }
    }
