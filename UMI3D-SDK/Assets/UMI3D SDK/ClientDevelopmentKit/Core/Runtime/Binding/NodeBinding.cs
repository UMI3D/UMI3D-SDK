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
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Client support for node binding.
    /// </summary>
    public class NodeBinding : AbstractSimpleBinding
    {
        public NodeBinding(AbstractSimpleBindingDataDto dto, Transform boundTransform) : base(dto, boundTransform)
        { }

        protected NodeBindingDataDto NodeBindingDataDto => dto as NodeBindingDataDto;

        public override void Apply(out bool success)
        {
            var parentNode = UMI3DEnvironmentLoader.Instance.GetNodeInstance(NodeBindingDataDto.nodeId);

            if (boundTransform is null || parentNode is null)
            {
                if (parentNode is null)
                    UMI3DLogger.LogError($"Node {NodeBindingDataDto.nodeId} is null. It may have been deleted without removing the binding first.", DebugScope.CDK | DebugScope.Core);
                success = false;
                return;
            }

            Compute((parentNode.transform.position, parentNode.transform.rotation, parentNode.transform.localScale));
            success = true;
        }
    }
}