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
using umi3d.common.userCapture;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using inetum.unityUtils;
using System;
using UnityEngine.Events;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// <see cref="UMI3DUser"/> with a UMI3D Avatar attached to it.
    /// </summary>
    public class UMI3DTrackedUser : UMI3DUser
    {
        /// <summary>
        /// User's avatar
        /// </summary>
        private UMI3DAvatarNode avatar;

        /// <summary>
        /// User's avatar
        /// </summary>
        public UMI3DAvatarNode Avatar
        {
            get => avatar;
            set
            {
                if (avatar == value)
                    return;
                if (avatar != null)
                    GameObject.Destroy(avatar.gameObject);
                if (value != null)
                    value.userId = Id();
                avatar = value;
                UMI3DServer.Instance.NotifyUserChanged(this);
            }
        }

        #region Bindings
        /// <summary>
        /// If true, the avatar could receive bindings.
        /// </summary>
        private bool activeAvatarBindings_ = true;

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

            var bindingDtoList = new List<BindingDto>();

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
            return base.ToBytes(user)
                + UMI3DSerializer.Write(userId)
                + UMI3DSerializer.Write(activeBindings.GetValue(user))
                + UMI3DSerializer.WriteIBytableCollection(bindings.GetValue(user), user);
        }

        #endregion
    }
}