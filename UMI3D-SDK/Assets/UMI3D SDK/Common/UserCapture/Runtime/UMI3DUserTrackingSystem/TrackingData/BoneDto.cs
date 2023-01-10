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
    /// A bone is a part of the skeleton associated to a user. It represents a part of the human body. 
    /// They are defined like a tree with rotations relation from children to parents bone.
    [Serializable]
    public class BoneDto : UMI3DDto, IBytable
    {
        /// <summary>
        /// Defines the type of the bone.
        /// </summary>
        public uint boneType;

        /// <summary>
        /// Rotation of the bone relative to the parent
        /// </summary>
        public SerializableVector4 rotation;

        /// <inheritdoc/>
        bool IBytable.IsCountable()
        {
            return true;
        }

        /// <inheritdoc/>
        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            return
                UMI3DSerializer.Write(boneType)
                + UMI3DSerializer.Write(rotation);
        }
    }
}