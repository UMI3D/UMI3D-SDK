/*
Copyright 2019 - 2024 Inetum

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
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    [CreateAssetMenu(fileName = "UMI3D PT Parameter Node Delegate", menuName = "UMI3D/Interactions/Projection Delegate/PT Parameter Node Delegate")]
    public class ProjectionTreeParameterNodeDelegate : AbstractProjectionTreeNodeDelegate<AbstractParameterDto>
    {
        public override Predicate<ProjectionTreeNodeData> IsNodeCompatible(AbstractParameterDto interaction)
        {
            return node =>
            {
                var interactionDto = node.interactionData.Interaction;
                return interactionDto is AbstractParameterDto
                && (interactionDto as AbstractParameterDto).GetType().Equals(interaction.GetType());
            };
        }

        public override Func<ProjectionTreeNodeData> CreateNodeForInput(
            AbstractParameterDto interaction,
            Func<AbstractUMI3DInput> findInput
        )
        {
            return () =>
            {
                AbstractUMI3DInput projection = findInput?.Invoke();

                if (projection == null)
                {
                    throw new NoInputFoundException($"For {nameof(AbstractParameterDto)}: {interaction.name}");
                }

                return new ProjectionTreeNodeData()
                {
                    treeId = treeId,
                    id = interaction.id,
                    children = new(),
                    interactionData = new ProjectionTreeParameterNodeData()
                    {
                        interaction = interaction
                    },
                    input = projection
                };
            };
        }

        public override Action<ProjectionTreeNodeData> ChooseProjection(
            ulong? environmentId = null,
            ulong? toolId = null,
            ulong? hoveredObjectId = null
        )
        {
            return node =>
            {
                if (environmentId.HasValue && toolId.HasValue && hoveredObjectId.HasValue)
                {
                    var interactionDto = node.interactionData.Interaction;
                    node.input.Associate(
                        environmentId.Value,
                        interactionDto,
                        toolId.Value,
                        hoveredObjectId.Value
                    );
                }
            };
        }
    }
}