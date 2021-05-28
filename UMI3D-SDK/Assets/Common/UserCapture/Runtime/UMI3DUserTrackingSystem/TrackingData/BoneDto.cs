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
    /// Class to describe a bone's 6-D pose in the frame of reference of a user.
    /// </summary>
    [Serializable]
    public class BoneDto : UMI3DDto, IByte
    {
        /// <summary>
        /// Defines the type of the bone.
        /// </summary>
        public string boneType;

        public SerializableVector3 position;

        public SerializableVector4 rotation;

        public SerializableVector3 scale;

        (int, Func<byte[], int, int>) IByte.ToByteArray(params object[] parameters)
        {
            int size = UMI3DNetworkingHelper.GetSize(boneType) + UMI3DNetworkingHelper.GetSize(position) + UMI3DNetworkingHelper.GetSize(rotation) + UMI3DNetworkingHelper.GetSize(scale);
            Func<byte[], int, int> func = (b, i) =>
            {
                i += UMI3DNetworkingHelper.Write(boneType, b, i);
                i += UMI3DNetworkingHelper.Write(position, b, i);
                i += UMI3DNetworkingHelper.Write(rotation, b, i);
                i += UMI3DNetworkingHelper.Write(scale, b, i);
                return size;
            };
            return (size, func);
        }
    }
}