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

using umi3d.common.core;
using umi3d.common.userCapture.binding;
using UnityEngine;

namespace umi3d.cdk.userCapture.binding
{
    /// <summary>
    /// Client support for binding between a bone and a rig.
    /// </summary>
    public class RigBoneBinding : BoneBinding
    {
        public RigBoneBinding(RigBoneBindingDataDto dto, Transform rigBoundTransform, ISkeleton skeleton, Transform rootObject)
                    : base(dto, rigBoundTransform, skeleton)
        {
            this.rootObject = rootObject;
            this.autoComputedRotationOffset = UseAutoComputedRotationOffset ? Quaternion.Inverse(rootObject.rotation) * rigBoundTransform.rotation : Quaternion.identity;
            RigBoneBindingDataDto = (RigBoneBindingDataDto)SimpleBindingData;
        }

        #region DTO Access

        protected RigBoneBindingDataDto RigBoneBindingDataDto { get; }

        protected Transform rootObject;

        /// <summary>
        /// See <see cref="RigBoneBindingDataDto.rigName"/>.
        /// </summary>
        public string RigName => RigBoneBindingDataDto.rigName;

        /// <summary>
        /// See <see cref="RigBoneBindingDataDto.applyOriginalRotation"/>.
        /// </summary>
        public bool UseAutoComputedRotationOffset => RigBoneBindingDataDto.applyOriginalRotation;


        #endregion DTO Access
    }
}