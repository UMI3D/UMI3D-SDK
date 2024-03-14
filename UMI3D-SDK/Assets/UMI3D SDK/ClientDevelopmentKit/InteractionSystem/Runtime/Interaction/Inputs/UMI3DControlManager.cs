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

using MathNet.Numerics;
using System;
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public sealed class UMI3DControlManager 
    {
        debug.UMI3DLogger logger = new();

        public Controls_SO controls_SO;

        [HideInInspector] public ControlModel model = new();
        [HideInInspector] public UMI3DController controller;
        [HideInInspector] public UMI3DToolManager toolManager;
        public IControlManipulationDelegate manipulationDelegate;
        public IControlEventDelegate eventDelegate;
        public IControlDelegate<FormDto> formDelegate;
        public IControlDelegate<LinkDto> linkDelegate;
        public IControlDelegate<AbstractParameterDto> parameterDelegate;

        /// <summary>
        /// Init this control manager.<br/>
        /// <br/>
        /// Warning: Delegate must be set before calling this method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controller"></param>
        public void Init(
            MonoBehaviour context, 
            UMI3DController controller
        )
        {
            logger.MainContext = context;
            logger.MainTag = nameof(UMI3DControlManager);
            this.controller = controller;
            this.toolManager = controller.toolManager;
            model.Init(controls_SO);

            logger.Assert(manipulationDelegate != null, $"{nameof(manipulationDelegate)} is null");
            logger.Assert(eventDelegate != null, $"{nameof(eventDelegate)} is null");
            logger.Assert(formDelegate != null, $"{nameof(formDelegate)} is null");
            logger.Assert(linkDelegate != null, $"{nameof(linkDelegate)} is null");
            logger.Assert(parameterDelegate != null, $"{nameof(parameterDelegate)} is null");
        }

        /// <summary>
        /// Whether or not <paramref name="tool"/> can be projected now on this controller.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool IsAvailableFor(AbstractTool tool)
        {
            //TODO Allow multiple projection.
            return toolManager.CurrentProjectedTool == null || toolManager.CurrentProjectedTool == tool;
        }

        /// <summary>
        /// Whether or not <paramref name="tool"/> can be projected on this controller.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public bool IsCompatibleWith(AbstractTool tool)
        {
            return tool.interactionsLoaded.TrueForAll(
                inter =>
                {
                    if (inter is not ManipulationDto manipulation)
                    {
                        return true;
                    }

                    // Return true if control is compatible with each dof in a separation.
                    System.Predicate<DofGroupOptionDto> controlCompatibleWithSeparation(HasManipulationControlData control)
                    {
                        return dofSep =>
                        {
                            foreach (var dof in dofSep.separations)
                            {
                                if (!control.ManipulationControlData.compatibleDofGroup.Contains(dof.dofs))
                                {
                                    return false;
                                }
                            }
                            return true;
                        };
                    }

                    // Return true if there is one physical manipulation control that is
                    // compatible with one dof separation of this interaction.
                    var compatible = model.controls_SO.physicalManipulationControls.Exists(
                        control =>
                        {
                            return manipulation.dofSeparationOptions.Exists(
                                controlCompatibleWithSeparation(control)
                            );
                        }
                    );
                    if (compatible)
                    {
                        return true;
                    }

                    // Return true if there is one ui manipulation control that is
                    // compatible with one dof separation of this interaction.
                    return model.controls_SO.uIManipulationControlPrefabs.Exists(
                        control =>
                        {
                            return manipulation.dofSeparationOptions.Exists(
                                controlCompatibleWithSeparation(control)
                            );
                        }
                    );
                }
            );
        }

        /// <summary>
        /// Return a control compatible with <paramref name="interaction"/>.<br/> 
        /// Return null if no control is available.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="unused"></param>
        /// <param name="tryToFindInputForHoldableEvent"></param>
        /// <param name="dof"></param>
        /// <returns></returns>
        public AbstractControlEntity GetControl<Interaction>(
            Interaction interaction,
            bool tryToFindInputForHoldableEvent = false,
            DofGroupDto dof = null
        )
            where Interaction: AbstractInteractionDto
        {
            switch (interaction)
            {
                case ManipulationDto manipulation:
                    manipulationDelegate.Dof = dof;
                    return manipulationDelegate.GetControl(
                        controller,
                        manipulation
                    );
                case EventDto button:
                    eventDelegate.TryToFindInputForHoldableEvent = tryToFindInputForHoldableEvent;
                    return eventDelegate.GetControl(
                        controller,
                        button
                    );
                case FormDto form:
                    return formDelegate.GetControl(
                        controller,
                        form
                    );
                case LinkDto link:
                    return linkDelegate.GetControl(
                        controller,
                        link
                    );
                case AbstractParameterDto parameter:
                    return parameterDelegate.GetControl(
                        controller,
                        parameter
                    );
                default:
                    throw new System.Exception("Unknown interaction type, can't project !");
            }
        }

        public void Associate(
            AbstractControlEntity control,
            ulong environmentId,
            ulong toolId,
            AbstractInteractionDto interaction,
            ulong hoveredObjectId,
            DofGroupDto dof = null
        )
        {
            switch (interaction)
            {
                case ManipulationDto manipulation:

                    manipulationDelegate.Dof = dof;
                    manipulationDelegate.Associate(
                        controller,
                        control,
                        environmentId,
                        interaction,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case EventDto button:

                    eventDelegate.Associate(
                        controller,
                        control,
                        environmentId,
                        interaction,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case FormDto form:

                    formDelegate.Associate(
                        controller,
                        control,
                        environmentId,
                        interaction,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case LinkDto link:

                    linkDelegate.Associate(
                        controller,
                        control,
                        environmentId,
                        interaction,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case AbstractParameterDto parameter:

                    parameterDelegate.Associate(
                        controller,
                        control,
                        environmentId,
                        interaction,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                default:
                    throw new System.Exception("Unknown interaction type, can't project !");
            }

            controls_SO.controlByInteraction.Add(interaction, control);
        }

        public AbstractControlEntity Dissociate(
            AbstractInteractionDto interaction
        )
        {
            if (!controls_SO.controlByInteraction.ContainsKey(interaction))
            {
                logger.Exception(
                    nameof(Dissociate),
                    new InteractionNotFoundException()
                );
                return null;
            }

            AbstractControlEntity control = controls_SO.controlByInteraction[interaction];
            switch (interaction)
            {
                case ManipulationDto manipulation:

                    manipulationDelegate.Dissociate(control);
                    break;

                case EventDto button:

                    eventDelegate.Dissociate(control);
                    break;

                case FormDto form:

                    formDelegate.Dissociate(control);
                    break;

                case LinkDto link:

                    linkDelegate.Dissociate(control);
                    break;

                case AbstractParameterDto parameter:

                    parameterDelegate.Dissociate(control);
                    break;

                default:
                    throw new System.Exception("Unknown interaction type, can't project !");
            }

            return control;
        }
    }
}