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

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Class to associate a bone to a node.
    /// </summary>
    /// Bone bindings allow to make links between the user skeleton's bones and other objects in the scene.
    /// For example, a binding is necessary yo enable the user to equip a watch or to hold a hammer.
    [Serializable]
    public class BoneBindingDto : UMI3DDto, IBytable
    {
        /// <summary>
        /// An identifier defined by the designer.
        /// </summary>
        public string bindingId;

        /// <summary>
        /// Optional rig name. If null, the whole object is binded to the bone.
        /// </summary>
        public string rigName;

        /// <summary>
        /// Define if the binding is currently active or overrided by the media.
        /// </summary>
        public bool active;

        /// <summary>
        /// Define if the binding has to synchronize the object position with the bone position.
        /// </summary>
        public bool syncPosition;

        /// <summary>
        /// Define if the binding has to synchronize the object rotation with the bone rotation.
        /// </summary>
        public bool syncRotation;

        /// <summary>
        /// If true, rejects all operations on the scale of the object but keep them in the DTO.
        /// </summary>
        public bool freezeWorldScale;

        /// <summary>
        /// The binded BoneType.
        /// </summary>
        public uint boneType;

        /// <summary>
        /// The identifier of the 3D object.
        /// </summary>
        public ulong objectId;

        /// <summary>
        /// Position offset between the object center of mass and the point used for the bindings
        /// </summary>
        public SerializableVector3 offsetPosition;

        /// <summary>
        /// Rotation offset between the object center of mass and the point used for the bindings
        /// </summary>
        public SerializableVector4 offsetRotation;

        /// <summary>
        /// Scale offset between the object center of mass and the point used for the bindings
        /// </summary>
        public SerializableVector3 offsetScale;

        /// <inheritdoc/>
        public bool IsCountable()
        {
            return false;
        }

        /// <inheritdoc/>
        public Bytable ToBytableArray(params object[] parameters)
        {

            return UMI3DSerializer.Write(bindingId)
                + UMI3DSerializer.Write(rigName)
                + UMI3DSerializer.Write(active)
                + UMI3DSerializer.Write(boneType)
                + UMI3DSerializer.Write(objectId)
                + UMI3DSerializer.Write(offsetPosition)
                + UMI3DSerializer.Write(offsetRotation)
                + UMI3DSerializer.Write(offsetScale)
                + UMI3DSerializer.Write(syncPosition)
                + UMI3DSerializer.Write(syncRotation)
                + UMI3DSerializer.Write(freezeWorldScale);
        }
    }
}