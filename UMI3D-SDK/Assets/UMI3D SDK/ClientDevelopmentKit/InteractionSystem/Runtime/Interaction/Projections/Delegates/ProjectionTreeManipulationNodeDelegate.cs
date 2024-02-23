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
    [CreateAssetMenu(fileName = "UMI3D PT Manipulation Node Delegate", menuName = "UMI3D/Interactions/Projection Delegate/PT Manipulation Node Delegate")]
    public class ProjectionTreeManipulationNodeDelegate : AbstractProjectionTreeNodeDelegate<ManipulationDto>
    {
        public DofGroupDto sep;

        public override Predicate<ProjectionTreeNodeData> IsNodeCompatible(ManipulationDto interaction)
        {
            return  node =>
            {
                var nodeData = (ProjectionTreeManipulationNodeData)node.interactionData;
                return nodeData.interaction is ManipulationDto
                && nodeData.manipulationDofGroupDto.dofs == sep.dofs;
            };
        }

        public override Func<ProjectionTreeNodeData> CreateNodeForInput(
            ManipulationDto interaction,
            Func<AbstractUMI3DInput> findInput
        )
        {
            return () =>
            {
                AbstractUMI3DInput projection = findInput?.Invoke();

                if (projection == null)
                {
                    throw new NoInputFoundException($"For {nameof(ManipulationDto)}: {interaction.name}");
                }

                return new ProjectionTreeNodeData()
                {
                    treeId = treeId,
                    id = interaction.id,
                    children = new(),
                    interactionData = new ProjectionTreeManipulationNodeData()
                    {
                        interaction = interaction,
                        manipulationDofGroupDto = sep
                    },
                    input = projection,
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
                        interactionDto as ManipulationDto,
                        sep.dofs,
                        toolId.Value,
                        hoveredObjectId.Value
                    );
                }
            };
        }
    }
}