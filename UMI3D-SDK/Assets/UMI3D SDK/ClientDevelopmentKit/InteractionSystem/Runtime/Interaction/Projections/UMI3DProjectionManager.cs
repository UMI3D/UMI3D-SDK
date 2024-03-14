/*
Copyright 2019 - 2021 Inetum

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
using System.Collections.Generic;
using System.Linq;
using umi3d.common.interaction;
using umi3d.debug;
using UnityEngine;
using UnityEngine.Windows;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Manage the links between projected tools and their associated inputs.<br/>
    /// <br/>
    /// This projection is based on a tree constituted of <see cref="ProjectionTreeNodeData"/>.
    /// </summary>
    public sealed class UMI3DProjectionManager
    {
        UMI3DLogger logger = new();

        public ProjectionTree_SO projectionTree_SO;
        public ProjectionEventSystem eventSystem;

        [HideInInspector] public UMI3DController controller;
        [HideInInspector] public UMI3DControlManager controlManager;
        [HideInInspector] public UMI3DToolManager toolManager;

        public IProjectionTreeNodeDelegate<ManipulationDto> ptManipulationNodeDelegate;
        public IProjectionTreeNodeDelegate<EventDto> ptEventNodeDelegate;
        public IProjectionTreeNodeDelegate<FormDto> ptFormNodeDelegate;
        public IProjectionTreeNodeDelegate<LinkDto> ptLinkNodeDelegate;
        public IProjectionTreeNodeDelegate<AbstractParameterDto> ptParameterNodeDelegate;

        string treeId;
        /// <summary>
        /// The root of the tree.
        /// </summary>
        ProjectionTreeNodeData treeRoot;
        ProjectionTreeModel treeModel;

        /// <summary>
        /// Init this projection manager.<br/>
        /// <br/>
        /// Warning: Delegate must be set before calling this method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controller"></param>
        /// <param name="controlManager"></param>
        /// <param name="toolManager"></param>
        public void Init(
            MonoBehaviour context,
            UMI3DController controller,
            UMI3DControlManager controlManager,
            UMI3DToolManager toolManager
        )
        {
            logger.MainContext = context;
            logger.MainTag = nameof(UMI3DProjectionManager);

            treeId = (context.gameObject.GetInstanceID() + UnityEngine.Random.Range(0, 1000)).ToString();
            treeRoot = new ProjectionTreeNodeData()
            {
                treeId = treeId,
                id = 0,
                children = new(),
                interactionData = null,
                control = null
            };
            treeModel = new(
                projectionTree_SO,
                treeId
            );
            treeModel.AddRoot(treeRoot);

            logger.Assert(ptManipulationNodeDelegate != null, $"{nameof(ptManipulationNodeDelegate)} is null");
            logger.Assert(ptEventNodeDelegate != null, $"{nameof(ptEventNodeDelegate)} is null");
            logger.Assert(ptFormNodeDelegate != null, $"{nameof(ptFormNodeDelegate)} is null");
            logger.Assert(ptLinkNodeDelegate != null, $"{nameof(ptLinkNodeDelegate)} is null");
            logger.Assert(ptParameterNodeDelegate != null, $"{nameof(ptParameterNodeDelegate)} is null");
            logger.Assert(eventSystem != null, $"{nameof(eventSystem)} is null");

            logger.Assert(controller != null, $"{nameof(controller)} is null");
            logger.Assert(controlManager != null, $"{nameof(controlManager)} is null");
            logger.Assert(toolManager != null, $"{nameof(toolManager)} is null");

            this.controller = controller;
            this.controlManager = controlManager;
            this.toolManager = toolManager;
        }

        /// <summary>
        /// Project an interaction and return associated control.
        /// </summary>
        /// <param name="controller">Controller to project on</param>
        /// <param name="evt">Event dto to project</param>
        List<AbstractControlEntity> Project<Dto>(
            Dto interaction,
            ulong environmentId,
            ulong toolId,
            ulong hoveredObjectId,
            DofGroupDto dof = null
        )
            where Dto : AbstractInteractionDto
        {
            ProjectionTreeNodeData currentNode = treeRoot;
            List<AbstractControlEntity> selectedControls = new();

            if (interaction is ManipulationDto manipulationDto)
            {
                DofGroupOptionDto[] options
                    = manipulationDto
                    .dofSeparationOptions
                    .ToArray();
                DofGroupOptionDto bestDofGroupOption
                    = controlManager
                    .manipulationDelegate
                    .FindBest(options);

                foreach (DofGroupDto sep in bestDofGroupOption.separations)
                {
                    currentNode = ProjectAndUpdateTree(
                        interaction,
                        currentNode,
                        environmentId,
                        toolId,
                        hoveredObjectId,
                        false,
                        sep
                    );
                    selectedControls.Add(currentNode.control);
                }
            }
            else
            {
                // TODO maybe check for hold Control.
                currentNode = ProjectAndUpdateTree(
                    interaction,
                    currentNode,
                    environmentId,
                    toolId,
                    hoveredObjectId,
                    false,
                    null
                );
                selectedControls.Add(currentNode.control);
            }

            return selectedControls;
        }

        /// <summary>
        /// Project a set of interactions and return associated controls.
        /// </summary>
        /// <param name="interactions">Interactions to project</param>
        /// <param name="environmentId"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        List<AbstractControlEntity> Project(
            IEnumerable<AbstractInteractionDto> interactions, 
            ulong environmentId, 
            ulong toolId, 
            ulong hoveredObjectId
        )
        {
            bool foundHoldableEvent = false;
            if (true)
            {
                var temp = new List<AbstractInteractionDto>();

                foreach (AbstractInteractionDto dto in interactions)
                {
                    if (dto is EventDto eventDto && eventDto.hold && !foundHoldableEvent)
                    {
                        temp.Insert(0, dto);
                        foundHoldableEvent = true;
                    }
                    else
                    {
                        temp.Add(dto);
                    }
                }

                if (foundHoldableEvent)
                {
                    interactions = temp.ToArray();
                }
            }

            ProjectionTreeNodeData currentNode = treeRoot;
            List<AbstractControlEntity> selectedControls = new();

            int depth = 0;
            foreach (AbstractInteractionDto interaction in interactions)
            {
                if (interaction is ManipulationDto manipulationDto)
                {
                    DofGroupOptionDto[] options 
                        = manipulationDto
                        .dofSeparationOptions
                        .ToArray();
                    DofGroupOptionDto bestDofGroupOption 
                        = controlManager
                        .manipulationDelegate
                        .FindBest(options);

                    foreach (DofGroupDto sep in bestDofGroupOption.separations)
                    {
                        currentNode = ProjectAndUpdateTree(
                            interaction,
                            currentNode,
                            environmentId,
                            toolId,
                            hoveredObjectId,
                            false,
                            sep
                        );
                        selectedControls.Add(currentNode.control);
                    }
                }
                else
                {
                    currentNode = ProjectAndUpdateTree(
                        interaction,
                        currentNode,
                        environmentId,
                        toolId,
                        hoveredObjectId,
                        depth == 0 && foundHoldableEvent,
                        null
                    );
                    selectedControls.Add(currentNode.control);
                }
                depth++;
            }

            return selectedControls;
        }

        /// <summary>
        /// Project a tool and its interaction.
        /// </summary>
        /// <param name="tool"> The ToolDto to be projected.</param>
        /// <see cref="Release(AbstractTool)"/>
        public void Project(
            AbstractTool tool,
            ulong hoveredObjectId
        )
        {
            if (!controlManager.IsCompatibleWith(tool))
            {
                throw new IncompatibleToolException($"For {tool.GetType().Name}: {tool.data.dto.name}");
            }

            if (toolManager.IsProjected(tool))
            {
                Release(tool);
            }

            List<AbstractControlEntity> controls = Project(
                tool.interactionsLoaded, 
                tool.data.environmentId, 
                tool.data.dto.id, 
                hoveredObjectId
            );
            toolManager.ProjectTool(tool);
            toolManager.AssociateControls(tool, controls.ToArray());
            eventSystem.OnProjected(tool);
        }

        /// <summary>
        /// Project the newly added tool's interaction when the server update the tool.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="newInteraction"></param>
        /// <param name="releasable"></param>
        /// <param name="reason"></param>
        public void Project(
            AbstractTool tool,
            AbstractInteractionDto newInteraction,
            bool releasable,
            InteractionMappingReason reason
        )
        {
            if (!toolManager.IsProjected(tool))
            {
                throw new System.Exception("This tool is not currently projected on this controller");
            }

            var controls = Project(
                newInteraction,
                tool.data.environmentId,
                tool.data.dto.id,
                toolManager.tool_SO.currentHoverId
            );
            toolManager.AssociateControls(tool, controls.ToArray());
        }

        /// <summary>
        /// Release a projected tool.
        /// </summary>
        /// <param name="tool">Tool to release</param>
        /// <see cref="Project(AbstractTool)"/>
        public void Release(AbstractTool tool)
        {
            if (!toolManager.IsProjected(tool))
            {
                logger.Error(nameof(Release), $"This tool is not currently projected on this controller");
                return;
            }

            toolManager.DissociateAllControls(tool);
            toolManager.ReleaseTool(tool);
            eventSystem.OnReleased(tool);
        }

        public void Release(
            AbstractTool tool, 
            AbstractInteractionDto oldInteraction
        )
        {
            var control = controlManager.Dissociate(oldInteraction);
            toolManager.DissociateControl(tool, control);
        }

        /// <summary>
        /// Updates the projection tree and project the interaction.<br/>
        /// 
        /// <b>Warning</b> interaction is associated with its control only if 
        /// <paramref name="environmentId"/>, <paramref name="hoveredObjectId"/> and <paramref name="toolId"/> are not null.<br/>
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="currentTreeNode"></param>
        /// <param name="environmentId"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="unused"></param>
        /// <param name="tryToFindInputForHoldableEvent"></param>
        /// <param name="dof"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        ProjectionTreeNodeData ProjectAndUpdateTree(
            AbstractInteractionDto interaction,
            ProjectionTreeNodeData currentTreeNode,
            ulong? environmentId = null,
            ulong? toolId = null,
            ulong? hoveredObjectId = null,
            bool tryToFindInputForHoldableEvent = false,
            DofGroupDto dof = null
        )
        {
            System.Predicate<ProjectionTreeNodeData> adequation;
            System.Func<ProjectionTreeNodeData> deepProjectionCreation;
            System.Action<ProjectionTreeNodeData> chooseProjection;

            switch (interaction)
            {
                case ManipulationDto manipulationDto:

                    if (ptManipulationNodeDelegate is ManipulationManager manipulationManager)
                    {
                        manipulationManager.dofGroup = dof;
                    }
                    adequation = ptManipulationNodeDelegate.IsNodeCompatible(manipulationDto);
                    deepProjectionCreation = ptManipulationNodeDelegate.CreateNodeForControl(
                        treeId,
                        manipulationDto,
                        () =>
                        {
                            return controlManager.GetControl(
                                manipulationDto,
                                dof: dof
                            );
                        }
                    );
                    chooseProjection = ptManipulationNodeDelegate.ChooseProjection(
                        controlManager,
                        environmentId, 
                        toolId, 
                        hoveredObjectId
                    );
                    break;

                case EventDto eventDto:

                    adequation = ptEventNodeDelegate.IsNodeCompatible(eventDto);
                    deepProjectionCreation = ptEventNodeDelegate.CreateNodeForControl(
                        treeId,
                        eventDto,
                        () =>
                        {
                            return controlManager.GetControl(
                                eventDto,
                                tryToFindInputForHoldableEvent
                            );
                        }
                    );
                    chooseProjection = ptEventNodeDelegate.ChooseProjection(
                        controlManager,
                        environmentId,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case FormDto formDto:

                    adequation = ptFormNodeDelegate.IsNodeCompatible(formDto);
                    deepProjectionCreation = ptFormNodeDelegate.CreateNodeForControl(
                        treeId,
                        formDto,
                        () =>
                        {
                            return controlManager.GetControl(
                                formDto
                            );
                        }
                    );
                    chooseProjection = ptFormNodeDelegate.ChooseProjection(
                        controlManager,
                        environmentId,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case LinkDto linkDto:

                    adequation = ptLinkNodeDelegate.IsNodeCompatible(linkDto);
                    deepProjectionCreation = ptLinkNodeDelegate.CreateNodeForControl(
                        treeId,
                        linkDto,
                        () =>
                        {
                            return controlManager.GetControl(
                                linkDto
                            );
                        }
                    );
                    chooseProjection = ptLinkNodeDelegate.ChooseProjection(
                        controlManager,
                        environmentId,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case AbstractParameterDto parameterDto:

                    adequation = ptParameterNodeDelegate.IsNodeCompatible(parameterDto);
                    deepProjectionCreation = ptParameterNodeDelegate.CreateNodeForControl(
                        treeId,
                        parameterDto,
                        () =>
                        {
                            return controlManager.GetControl(
                                parameterDto
                            );
                        }
                    );
                    chooseProjection = ptParameterNodeDelegate.ChooseProjection(
                        controlManager,
                        environmentId,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                default:
                    throw new System.Exception("Unknown interaction type, can't project !");
            }

            return ProjectAndUpdateTree(
                currentTreeNode,
                adequation,
                deepProjectionCreation,
                chooseProjection
            );
        }

        /// <summary>
        /// Updates the projection tree and project the interaction.<br/>
        /// 
        /// <b>Warning</b> interaction is not necessary associated with a control.
        /// </summary>
        /// <param name="isAdequate">Whether the given projection node is adequate for the interaction to project</param>
        /// <param name="projectionNodeCreation">Create a new projection node, should throw an <see cref="NoInputFoundException"/> if no input is available</param>
        /// <param name="chooseProjection">Project the interaction to the given node's control</param>
        /// <param name="currentTreeNode">Current node in tree projection</param>
        /// <param name="unusedInputsOnly">Project on unused controls only</param>
        /// <exception cref="NoInputFoundException"></exception>
        ProjectionTreeNodeData ProjectAndUpdateTree(
            ProjectionTreeNodeData currentTreeNode,
            Predicate<ProjectionTreeNodeData> isAdequate,
            Func<ProjectionTreeNodeData> projectionNodeCreation,
            Action<ProjectionTreeNodeData> chooseProjection
        )
        {
            ProjectionTreeNodeData? projection;
            ///<summary>
            /// Return 1 when projection has been found.<br/>
            /// Return 0 when projection has been found but the input is not available.<br/>
            /// Return -1 when projection has not been found.
            /// </summary>
            int _FindProjection(List<ProjectionTreeNodeData> children, out ProjectionTreeNodeData? projection)
            {
                IEnumerable<int> indexes = Enumerable
                    .Range(0, children.Count)
                    .Where(i =>
                    {
                        return isAdequate?.Invoke(children[i]) ?? false;
                    });

                foreach (var index in indexes)
                {
                    var tmp = children[index];
                    if (!tmp.control.controlData.isUsed)
                    {
                        projection = tmp;
                        return 1;
                    }
                }

                projection = null;
                return indexes.Count() == 0 ? -1 : 0;
            }

            int projectionStatus = _FindProjection(
                treeModel.GetAllSubNodes(currentTreeNode),
                out projection
            );
            if (projectionStatus == -1) 
            {
                projectionStatus = _FindProjection(
                    treeModel.GetAllSubNodes(treeRoot),
                    out projection
                );
                if (projectionStatus <= 0)
                {
                    projection = projectionNodeCreation();

                    treeModel.AddChild(currentTreeNode.id, projection.Value);
                }
                else
                {
                    treeModel.RemoveChild(projection.Value.parentId, projection.Value.id);
                    treeModel.AddChild(currentTreeNode.id, projection.Value);
                }
            }
            else if (projectionStatus == 1)
            {
                treeModel.RemoveChild(projection.Value.parentId, projection.Value.id);
                treeModel.AddChild(currentTreeNode.id, projection.Value);
            }

            chooseProjection(projection.Value);
            eventSystem.OnProjected(
                projection.Value.control
            );
            return projection.Value;
        }
    }
}