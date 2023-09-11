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

using umi3d.common;
using umi3d.common.dto.binding;
using UnityEngine;

namespace umi3d.cdk.binding
{
    /// <summary>
    /// Client support for node binding.
    /// </summary>
    public class RigNodeBinding : NodeBinding
    {

        public RigNodeBinding(RigNodeBindingDataDto dto, Transform boundTransform, UMI3DNodeInstance parentNode, Transform rootObject) : base(dto, boundTransform, parentNode)
        {
            this.rootObject = rootObject;
            this.originalRotationOffset = Quaternion.Inverse(rootObject.rotation) * boundTransform.rotation;
        }

        protected RigNodeBindingDataDto RigNodeBindingDataDto => SimpleBindingData as RigNodeBindingDataDto;

        protected Transform rootObject;

        #region DTO Access

        /// <summary>
        /// See <see cref="NodeBindingDataDto.parentNodeId"/>.
        /// </summary>
        public virtual string RigName => RigNodeBindingDataDto.rigName;

        #endregion DTO Access

        /// <inheritdoc/>
        public override void Apply(out bool success)
        {
            if (boundTransform == null || parentNode is null || parentNode.transform == null || RigName == null)
            {
                if (parentNode is null || parentNode.transform == null)
                    UMI3DLogger.LogError($"Node {NodeBindingDataDto.parentNodeId} is null. It may have been deleted without removing the binding first.", DebugScope.CDK | DebugScope.Core);
                if (RigName == null)
                    UMI3DLogger.LogError($"Rig is null. It may have been deleted without removing the binding first.", DebugScope.CDK | DebugScope.Core);

                success = false;
                return;
            }

            Compute((parentNode.transform.position, parentNode.transform.rotation, parentNode.transform.localScale));
            success = true;
        }
    }
}