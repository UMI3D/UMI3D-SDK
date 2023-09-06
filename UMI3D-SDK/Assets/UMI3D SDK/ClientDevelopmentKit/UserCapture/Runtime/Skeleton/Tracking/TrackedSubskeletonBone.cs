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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Representation for each tracked bone.
    /// </summary>
    public class TrackedSubskeletonBone : MonoBehaviour
    {
        /// <summary>
        /// Bone type in UMI3D standards. See <see cref="BoneDto"/>.
        /// </summary>
        [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone type in UMI3D standards.")]
        public uint boneType;

        public bool positionComputed;

        /// <summary>
        /// Convert this bone to a dto.
        /// </summary>
        /// <param name="Anchor">Frame of reference</param>
        /// <returns></returns>
        public BoneDto ToBoneDto()
        {
            return boneType == BoneType.None ? null : new BoneDto { boneType = boneType, rotation = this.transform.rotation.Dto() };
        }

        public virtual ControllerDto ToControllerDto()
        {
            return boneType == BoneType.None ? null : new ControllerDto { boneType = boneType, position = this.transform.position.Dto(), rotation = this.transform.rotation.Dto(), isOverrider = false };
        }
    }
}