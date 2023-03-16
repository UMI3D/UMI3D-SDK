/*
Copyright 2019 - 2023 Inetum

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
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Scriptable object to contains data for BonePoseDto
    /// </summary>
    public class UMI3DBonePose_so : ScriptableObject
    {
        [SerializeField, ConstEnum(typeof(BoneType), typeof(uint))] public uint bone;
        public Vector3 position;
        public Vector4 rotation;

        public void Init(uint bone, Vector3 position, Vector4 rotation)
        {
            this.bone = bone;
            this.position = position;
            this.rotation = rotation;
        }

        /// <summary>
        /// Transforms the Scriptable Object to its DTO counterpart
        /// </summary>
        /// <returns></returns>
        public BonePoseDto ToDTO()
        {
            return new BonePoseDto(bone, position, rotation);
        }
    }
}
