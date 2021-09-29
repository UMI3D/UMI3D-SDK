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
        /// The binded BoneType.
        /// </summary>
        public uint boneType;

        /// <summary>
        /// The identifier of the 3D object.
        /// </summary>
        public ulong objectId;

        public SerializableVector3 offsetPosition;

        public SerializableVector4 offsetRotation;

        public bool IsCountable()
        {
            return false;
        }

        public Bytable ToBytableArray(params object[] parameters)
        {

            return UMI3DNetworkingHelper.Write(bindingId)
                + UMI3DNetworkingHelper.Write(rigName)
                + UMI3DNetworkingHelper.Write(active)
                + UMI3DNetworkingHelper.Write(boneType)
                + UMI3DNetworkingHelper.Write(objectId)
                + UMI3DNetworkingHelper.Write(offsetPosition)
                + UMI3DNetworkingHelper.Write(offsetRotation)
                + UMI3DNetworkingHelper.Write(syncPosition);
        }
    }
}