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
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Client support for bone binding.
    /// </summary>
    public class BoneBinding : AbstractSimpleBinding
    {
        public BoneBinding(AbstractSimpleBindingDataDto dto, Transform boundTransform, ISkeleton skeleton) : base(dto, boundTransform)
        {
            this.skeleton = skeleton;
        }

        protected BoneBindingDataDto BoneBindingDataDto => SimpleBindingData as BoneBindingDataDto;

        protected ISkeleton skeleton;

        public override void Apply(out bool success)
        {
            if (boundTransform is null) // node is destroyed
            {
                success = false;
                return;
            }

            var parentBone = skeleton.Bones[BoneBindingDataDto.boneType];
            if (parentBone is null)
            {
                UMI3DLogger.LogError($"Bone transform from bone {BoneBindingDataDto.boneType} is null. It may have been deleted without removing the binding first.", DebugScope.CDK | DebugScope.Core);
                success = false;
                return;
            }

            Compute((parentBone.s_Position, parentBone.s_Rotation, Vector3.one));
            success = true;
        }
    }
}