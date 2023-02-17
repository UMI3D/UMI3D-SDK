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
        public bool syncScale = true;
        public bool freezeWorldScale = true;
        public UMI3DAbstractNode node;
        public string rigName = "";
        public Vector3 offsetPosition = Vector3.zero;
        public Vector4 offsetRotation = Vector4.zero;
        public Vector3 offsetScale = Vector3.zero;
        public bool partialFit = true;
        public int priotity = 0;

        public UMI3DBinding() { }

        public UMI3DBinding(UMI3DBinding b)
        {
            boneType = b.boneType;
            isBinded = b.isBinded;
            syncPosition = b.syncPosition;
            syncRotation = b.syncRotation;
            syncScale = b.syncScale;
            freezeWorldScale = b.freezeWorldScale;
            node = b.node;
            rigName = b.rigName;
            offsetPosition = b.offsetPosition;
            offsetRotation = b.offsetRotation;
            offsetScale = b.offsetScale;
            partialFit = b.partialFit; 
            priotity= b.priotity;
        }

        /// <inheritdoc/>
        public Bytable ToByte(UMI3DUser user)
        {
            return UMI3DSerializer.Write(boneType)
                    + UMI3DSerializer.Write(isBinded)
                    + UMI3DSerializer.Write(this.node?.Id() ?? 0)
                    + UMI3DSerializer.Write(rigName)
                    + UMI3DSerializer.Write(offsetPosition)
                    + UMI3DSerializer.Write(offsetRotation)
                    + UMI3DSerializer.Write(offsetScale)
                    + UMI3DSerializer.Write(syncPosition)
                    + UMI3DSerializer.Write(syncRotation)
                    + UMI3DSerializer.Write(syncScale)
                    + UMI3DSerializer.Write(freezeWorldScale)
                    + UMI3DSerializer.Write(partialFit)
                    + UMI3DSerializer.Write(priotity);
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
        public BindingDto ToDto(UMI3DUser user)
        {
            RigBindingDataDto rigBindingDataDto = new RigBindingDataDto(
                rigName : rigName, 
                boneType : boneType,
                userId : user.Id(),
                offSetScale : offsetScale,
                offSetRotation : offsetRotation,
                offSetPosition : offsetPosition,
                syncScale : syncScale,
                syncRotation : syncRotation,
                syncPosition : syncPosition,
                partialFit : partialFit,
                priority : priotity
            );

            BindingDto bindingDto = new BindingDto(
                objectId: NodeCheckForId(),
                active: true,
                data : rigBindingDataDto
            ) ;

            //if (user != null)
            //    dto.bindingId = user.Id() + "binding_" + boneType + "_" + rigName + "_" + dto.objectId;
            //else
            //    dto.bindingId = "binding_" + boneType + "_" + rigName + "_" + dto.objectId;

            return bindingDto;
        }

        private ulong NodeCheckForId()
        {
            if (node != null)
            {
                return node.Id();
            }
            else
            {
                return 0;
            }
        }
    }
}
