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

using inetum.unityUtils;
using System;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace umi3d.cdk.interaction
{
    public class ManipulationManager : IProjectionTreeNodeDelegate<ManipulationDto>, IControlManipulationDelegate
    {
        // IProjectionTreeNodeDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public DofGroupDto dofGroup;

        public Predicate<ProjectionTreeNodeData> IsNodeCompatible(ManipulationDto interaction)
        {
            return node =>
            {
                return node.interactionData is ProjectionTreeManipulationNodeData nodeData
                && nodeData.interaction is ManipulationDto
                && nodeData.dofGroup.dofs == dofGroup.dofs;
            };
        }

        public Func<ProjectionTreeNodeData> CreateNodeForControl(
            string treeId,
            ManipulationDto interaction,
            Func<AbstractControlEntity> getControl
        )
        {
            return () =>
            {
                AbstractControlEntity control = getControl?.Invoke();

                if (control == null)
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
                        dofGroup = dofGroup
                    },
                    control = control
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
                        hoveredObjectId.Value,
                        dofGroup
                    );
                }
            };
        }

        // IControlManipulationDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public DofGroupDto Dof { get; set; }

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
            if (interaction is not ManipulationDto manipInteraction)
            {
                throw new Exception($"[UMI3D] Control: Interaction is not an {nameof(ManipulationDto)}.");
            }
            if (control is not HasManipulationControlData manipControl)
            {
                throw new Exception($"[UMI3D] Control: control is not a {nameof(HasManipulationControlData)}.");
            }

            (this as IControlDelegate<EventDto>).BaseAssociate(
                controller,
                control,
                environmentId,
                interaction,
                toolId,
                hoveredObjectId
            );

            manipControl.ManipulationControlData.frameOfReference = UMI3DEnvironmentLoader
                .GetNode(
                    environmentId,
                    manipInteraction.frameOfReference
                )
                .gameObject
                .transform;

            uint boneType = control
                .controlData
                .controller
                .BoneType;
            Vector3Dto bonePosition = control
                .controlData
                .controller
                .BoneTransform
                .position
                .Dto();
            Vector4Dto boneRotation = control
                .controlData
                .controller
                .BoneTransform
                .rotation
                .Dto();

            manipControl
                .ManipulationControlData
                .messageSender
                .messageHandler = () =>
                {
                    ManipulationRequestDto request = new();
                    manipControl.ManipulationControlData.SetRequestTranslationAndRotation(
                        Dof.dofs,
                        request,
                        control
                            .controlData
                            .controller
                            .ManipulationTransform
                    );
                    request.boneType = boneType;
                    request.bonePosition = bonePosition;
                    request.boneRotation = boneRotation;
                    request.id = interaction.id;
                    request.toolId = toolId;
                    request.hoveredObjectId = hoveredObjectId;
                    UMI3DClientServer.SendRequest(request, true);
                };

            control.controlData.actionPerformedHandler += phase =>
            {
                switch (phase)
                {
                    case InputActionPhase.Started:
                        manipControl.ManipulationControlData.initialPosition =
                            control
                                .controlData
                                .controller
                                .ManipulationTransform
                                .position;
                        manipControl.ManipulationControlData.initialRotation =
                           control
                               .controlData
                               .controller
                               .ManipulationTransform
                               .rotation;

                        manipControl.ManipulationControlData.messageSender.networkMessage
                            = CoroutineManager
                                .Instance
                                .AttachCoroutine(
                                    manipControl.ManipulationControlData
                                    .messageSender
                                    .NetworkMessageSender()
                                );
                        break;
                    case InputActionPhase.Canceled:
                        CoroutineManager
                            .Instance
                            .DetachCoroutine(
                                manipControl
                                .ManipulationControlData
                                .messageSender
                                .networkMessage
                            );
                        break;
                    default:
                        break;
                }
            };
        }

        public void Dissociate(AbstractControlEntity control)
        {
            (this as IControlDelegate<EventDto>).BaseDissociate(control);
        }

        public AbstractControlEntity GetControl(UMI3DController controller, ManipulationDto interaction)
        {
            var model = controller.controlManager.model;
            var physicalManipulation = model.GetPhysicalManipulation(Dof);
            if (physicalManipulation != null)
            {
                return physicalManipulation;
            }

            return model.GetUIManipulation(Dof);
        }

        /// <summary>
        /// Find the best <see cref="DofGroupOptionDto"/>> for this controller.
        /// </summary>
        /// <param name="options">Options to search in</param>
        /// <returns></returns>
        public DofGroupOptionDto FindBest(DofGroupOptionDto[] options)
        {
            throw new System.NotImplementedException();
        }
    }
}