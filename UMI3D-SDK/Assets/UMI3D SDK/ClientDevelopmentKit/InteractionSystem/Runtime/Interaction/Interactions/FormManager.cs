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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace umi3d.cdk.interaction
{
    public class FormManager : IProjectionTreeNodeDelegate<FormDto>, IControlDelegate<FormDto>
    {
        // IProjectionTreeNodeDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public Predicate<ProjectionTreeNodeData> IsNodeCompatible(FormDto interaction)
        {
            return node =>
            {
                var interactionDto = node.interactionData.Interaction;
                return interactionDto is FormDto && interactionDto.name.Equals(interaction.name);
            };
        }

        public Func<ProjectionTreeNodeData> CreateNodeForControl(
            string treeId,
            FormDto interaction,
            Func<AbstractControlEntity> getControl
        )
        {
            return () =>
            {
                AbstractControlEntity control = getControl?.Invoke();

                if (control == null)
                {
                    throw new NoInputFoundException($"For {nameof(FormDto)}: {interaction.name}");
                }

                return new ProjectionTreeNodeData()
                {
                    treeId = treeId,
                    id = interaction.id,
                    children = new(),
                    interactionData = new ProjectionTreeFormNodeData()
                    {
                        interaction = interaction
                    },
                    control = control,
                };
            };
        }

        public Action<ProjectionTreeNodeData> ChooseProjection(
            UMI3DControlManager controlManager,
            ulong? environmentId = null,
            ulong? toolId = null,
            ulong? hoveredObjectId = null
        )
        {
            return node =>
            {
                if (environmentId.HasValue && toolId.HasValue && hoveredObjectId.HasValue)
                {
                    controlManager.Associate(
                        node.control,
                        environmentId.Value,
                        toolId.Value,
                        node.interactionData.Interaction,
                        hoveredObjectId.Value
                    );
                }
            };
        }

        // IControlDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public bool CanPerform(System.Object value)
        {
            throw new System.NotImplementedException();
        }

        public void Associate(
            UMI3DController controller,
            AbstractControlEntity control,
            ulong environmentId,
            AbstractInteractionDto interaction,
            ulong toolId,
            ulong hoveredObjectId
        )
        {
            throw new System.NotImplementedException();
        }

        public void Dissociate(AbstractControlEntity control)
        {
            (this as IControlDelegate<EventDto>).BaseDissociate(control);
        }

        public AbstractControlEntity GetControl(UMI3DController controller, FormDto interaction)
        {
            throw new System.NotImplementedException();
        }
    }
}