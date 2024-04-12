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

using umi3d.cdk.binding;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.binding;
using UnityEngine;

namespace umi3d.cdk.userCapture.binding
{
    /// <summary>
    /// Client support for bone binding.
    /// </summary>
    public class BoneBinding : AbstractSimpleBinding
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        public BoneBinding(BoneBindingDataDto dto, Transform boundTransform, ISkeleton skeleton) : base(dto, boundTransform)
        {
            this.skeleton = skeleton;
        }

        #region DTO Access

        protected BoneBindingDataDto BoneBindingDataDto => SimpleBindingData as BoneBindingDataDto;

        /// <summary>
        /// See <see cref="BoneBindingDataDto.userId"/>.
        /// </summary>
        public ulong UserId => BoneBindingDataDto.userId;

        /// <summary>
        /// See <see cref="BoneBindingDataDto.boneType"/>.
        /// </summary>
        public uint BoneType => BoneBindingDataDto.boneType;

        public bool BindToController => BoneBindingDataDto.bindToController;

        #endregion DTO Access

        /// <summary>
        /// Skeleton on which the binding is applied.
        /// </summary>
        protected ISkeleton skeleton;

        /// <inheritdoc/>
        public override void Apply(out bool success)
        {
            if (boundTransform == null) // node is destroyed
            {
                UMI3DLogger.LogWarning($"Bound transform is null. It may have been deleted without removing the binding first.", DEBUG_SCOPE);
                success = false;
                return;
            }

            ISkeleton.Transformation parentBone;

            if (!BindToController)
            {
                parentBone = skeleton.Bones[BoneType];
            }
            else if (skeleton.TrackedSubskeleton.Controllers.TryGetValue(BoneType, out IController controller) && controller is DistantController)
            {
                parentBone = new()
                {
                    Position = controller.position,
                    Rotation = controller.rotation,
                    Scale = controller.scale
                };
            }
            else
            {
                UMI3DLogger.LogWarning($"No existing controller for {BoneType}. It may have been deleted without removing the binding first.", DEBUG_SCOPE);
                success = false;
                return;
            }

            if (parentBone is null)
            {
                UMI3DLogger.LogWarning($"Bone transform from bone {BoneType} is null. It may have been deleted without removing the binding first.", DEBUG_SCOPE);
                success = false;
                return;
            }

            Compute((parentBone.Position, parentBone.Rotation, parentBone.Scale));
            success = true;
        }
    }
}