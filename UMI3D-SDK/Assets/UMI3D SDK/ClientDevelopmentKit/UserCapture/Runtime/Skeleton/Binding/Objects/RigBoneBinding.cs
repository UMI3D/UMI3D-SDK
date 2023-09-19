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

using System.Linq;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.binding;
using UnityEngine;

namespace umi3d.cdk.userCapture.binding
{
    /// <summary>
    /// Client support for binding between a bone and a rig.
    /// </summary>
    public class RigBoneBinding : BoneBinding
    {
        public RigBoneBinding(RigBoneBindingDataDto dto, Transform rigBoundTransform, ISkeleton skeleton, Transform rootObject) : base(dto, rigBoundTransform, skeleton)
        {
            this.rootObject = rootObject;
            this.originalRotationOffset = Quaternion.Inverse(rootObject.rotation) * boundTransform.rotation;
        }

        #region DTO Access

        protected RigBoneBindingDataDto RigBoneBindingDataDto => SimpleBindingData as RigBoneBindingDataDto;

        protected Transform rootObject;

        /// <summary>
        /// See <see cref="RigBoneBindingDataDto.rigName"/>.
        /// </summary>
        public string RigName => RigBoneBindingDataDto.rigName;

        #endregion DTO Access

        /// <inheritdoc/>
        public override void Apply(out bool success)
        {
            if (boundTransform == null) // node is destroyed
            {
                success = false;
                return;
            }

            ISkeleton.Transformation parentBone = null;

            if (!RigBoneBindingDataDto.bindToController)
                parentBone = skeleton.Bones[BoneType];
            else
            {
                var controller = ((skeleton.TrackedSubskeleton as TrackedSubskeleton).controllers.Find(c => c.boneType == BoneType) as DistantController);

                if (controller != null)
                    parentBone = new()
                    {
                        Position = controller.position,
                        Rotation = controller.rotation,
                    };
            }
            
            if (parentBone is null)
            {
                UMI3DLogger.LogError($"Bone transform from bone {BoneType} is null. It may have been deleted without removing the binding first.", DebugScope.CDK | DebugScope.Core);
                success = false;
                return;
            }

            Compute((parentBone.Position, parentBone.Rotation, Vector3.one));
            success = true;
        }
    }
}