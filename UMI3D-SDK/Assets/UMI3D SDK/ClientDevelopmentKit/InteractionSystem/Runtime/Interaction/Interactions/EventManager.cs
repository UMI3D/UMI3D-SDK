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
    public sealed class EventManager : IProjectionTreeNodeDelegate<EventDto>, IControlEventDelegate
    {
        // IProjectionTreeNodeDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public Predicate<ProjectionTreeNodeData> IsNodeCompatible(EventDto interaction)
        {
            return node =>
            {
                var interactionDto = node.interactionData.Interaction;
                return interactionDto is EventDto && interactionDto.name.Equals(interaction.name);
            };
        }

        public Func<ProjectionTreeNodeData> CreateNodeForControl(
            string treeId,
            EventDto interaction,
            Func<AbstractControlEntity> getControl
        )
        {
            return () =>
            {
                AbstractControlEntity control = getControl?.Invoke();

                if (control == null)
                {
                    throw new NoInputFoundException($"For {nameof(EventDto)}: {interaction.name}");
                }

                return new ProjectionTreeNodeData()
                {
                    treeId = treeId,
                    id = interaction.id,
                    children = new(),
                    interactionData = new ProjectionTreeEventNodeData()
                    {
                        interaction = interaction
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
                        hoveredObjectId.Value
                    );
                }
            };
        }


        // IControlEventDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public bool TryToFindInputForHoldableEvent { get; set; }

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
            if (interaction is not EventDto evtInteraction)
            {
                throw new Exception($"[UMI3D] Control: Interaction is not an {nameof(EventDto)}.");
            }
            if (control is not HasButtonControlData)
            {
                throw new Exception($"[UMI3D] Control: control is not a {nameof(HasButtonControlData)}.");
            }

            (this as IControlDelegate<EventDto>).BaseAssociate(
                controller,
                control,
                environmentId,
                interaction,
                toolId,
                hoveredObjectId
            );

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

            control.controlData.actionPerformedHandler = phase =>
            {
                InteractionRequestDto request = null;
                switch (phase)
                {
                    case InputActionPhase.Started:
                        if (evtInteraction.hold)
                        {
                            request = new EventStateChangedDto()
                            {
                                active = true
                            };
                        }
                        else
                        {
                            request = new EventTriggeredDto();
                        }
                        request.boneType = boneType;
                        request.toolId = toolId;
                        request.id = interaction.id;
                        request.hoveredObjectId = hoveredObjectId;
                        request.bonePosition = bonePosition;
                        request.boneRotation = boneRotation;

                        UMI3DClientServer.SendRequest(request, true);
                        if (evtInteraction.TriggerAnimationId != 0)
                        {
                            //TODO
                            StartAnim(evtInteraction.TriggerAnimationId);
                        }
                        break;
                    case InputActionPhase.Canceled:
                        if (evtInteraction.hold)
                        {
                            request = new EventStateChangedDto()
                            {
                                active = false,
                                boneType = boneType,
                                id = interaction.id,
                                toolId = toolId,
                                hoveredObjectId = hoveredObjectId,
                                bonePosition = bonePosition,
                                boneRotation = boneRotation,
                            };

                            UMI3DClientServer.SendRequest(request, true);
                            if (evtInteraction.ReleaseAnimationId != 0)
                            {
                                //TODO
                                StartAnim(evtInteraction.ReleaseAnimationId);
                            }
                        }
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

        public AbstractControlEntity GetControl(UMI3DController controller, EventDto interaction)
        {
            var model = controller.controlManager.model;
            var physicalButton = model.GetPhysicalButton();
            if (physicalButton != null)
            {
                return physicalButton;
            }

            return model.GetUIButton();
        }

        async void StartAnim(ulong id)
        {
            UMI3DEntityInstance entity = UMI3DEnvironmentLoader
                .Instance
                .TryGetEntityInstance(
                UMI3DGlobalID.EnvironmentId,
                id
            );

            if (entity == null)
            {
                return;
            }

            UMI3DAbstractAnimation animation = entity.Object as UMI3DAbstractAnimation;
            if (animation == null)
            {
                return;
            }

            await animation.SetUMI3DProperty(
            new SetUMI3DPropertyData(
                UMI3DGlobalID.EnvironmentId,
                new SetEntityPropertyDto()
                {
                    entityId = id,
                    property = UMI3DPropertyKeys.AnimationPlaying,
                    value = true
                },
                entity)
            );
            animation.Start();
        }
    }
}