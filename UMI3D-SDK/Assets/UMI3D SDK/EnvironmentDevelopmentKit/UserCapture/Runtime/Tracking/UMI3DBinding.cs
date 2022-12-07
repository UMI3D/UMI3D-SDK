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

using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Environment representation of a binding, linking two objects.
    /// </summary>
    public class UMI3DBinding : IBytable
    {
        /// <summary>
        /// UMI3D bone type in <see cref="BoneType"/>
        /// </summary>
        public uint boneType;
        public bool isBinded = true;
        public bool syncPosition = true;
        public bool syncRotation = true;
        public bool freezeWorldScale = true;
        public UMI3DAbstractNode node;
        public string rigName = "";
        public Vector3 offsetPosition = Vector3.zero;
        public Quaternion offsetRotation = Quaternion.identity;
        public Vector3 offsetScale = Vector3.zero;

        public UMI3DBinding() { }

        public UMI3DBinding(UMI3DBinding b)
        {
            boneType = b.boneType;
            isBinded = b.isBinded;
            syncPosition = b.syncPosition;
            syncRotation = b.syncRotation;
            freezeWorldScale = b.freezeWorldScale;
            node = b.node;
            rigName = b.rigName;
            offsetPosition = b.offsetPosition;
            offsetRotation = b.offsetRotation;
            offsetScale = b.offsetScale;
        }

        /// <inheritdoc/>
        public Bytable ToByte(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(boneType)
                    + UMI3DNetworkingHelper.Write(isBinded)
                    + UMI3DNetworkingHelper.Write(this.node?.Id() ?? 0)
                    + UMI3DNetworkingHelper.Write(rigName)
                    + UMI3DNetworkingHelper.Write(offsetPosition)
                    + UMI3DNetworkingHelper.Write(offsetRotation)
                    + UMI3DNetworkingHelper.Write(offsetScale)
                    + UMI3DNetworkingHelper.Write(syncPosition)
                    + UMI3DNetworkingHelper.Write(syncRotation)
                    + UMI3DNetworkingHelper.Write(freezeWorldScale);
        }

        /// <inheritdoc/>
        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            if (parameters.Length < 1)
                return ToByte(null);
            return ToByte(parameters[0] as UMI3DUser);
        }

        /// <inheritdoc/>
        bool IBytable.IsCountable()
        {
            return true;
        }
        /// <inheritdoc/>
        public BoneBindingDto ToDto(UMI3DUser user)
        {
            var dto = new BoneBindingDto()
            {
                rigName = rigName,
                active = isBinded,
                syncPosition = syncPosition,
                syncRotation = syncRotation,
                freezeWorldScale = freezeWorldScale,
                boneType = boneType,
                offsetPosition = offsetPosition,
                offsetRotation = offsetRotation,
                offsetScale = offsetScale,
            };

            if (node != null)
                dto.objectId = node.Id();
            else
                dto.objectId = 0;

            if (user != null)
                dto.bindingId = user.Id() + "binding_" + boneType + "_" + rigName + "_" + dto.objectId;
            else
                dto.bindingId = "binding_" + boneType + "_" + rigName + "_" + dto.objectId;

            return dto;
        }
    }
}
