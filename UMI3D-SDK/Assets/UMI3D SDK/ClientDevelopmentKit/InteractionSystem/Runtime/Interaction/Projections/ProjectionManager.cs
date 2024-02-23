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
    /// This projection is based on a tree <see cref="ProjectionTreeData"/> constituted of <see cref="ProjectionTreeNodeData"/>.
    /// </summary>
    [Serializable]
    public sealed class ProjectionManager
    {
        [SerializeField]
        UMI3DLogger logger = new();
        [Header("Inspector Dependency Injection")]
        public ProjectionTree_SO projectionTree_SO;
        public ProjectionTreeManipulationNodeDelegate ptManipulationNodeDelegate;
        public ProjectionTreeEventNodeDelegate ptEventNodeDelegate;
        public ProjectionTreeFormNodeDelegate ptFormNodeDelegate;
        public ProjectionTreeLinkNodeDelegate ptLinkNodeDelegate;
        public ProjectionTreeParameterNodeDelegate ptParameterNodeDelegate;
        public ProjectionEventDelegate eventDelegate;

        [HideInInspector]
        public UMI3DInputManager inputManager;
        [HideInInspector]
        public UMI3DToolManager toolManager;

        /// <summary>
        /// The root of the tree.
        /// </summary>
        ProjectionTreeNodeData treeRoot;
        ProjectionTreeModel treeModel;

        public void Init(
            MonoBehaviour context,
            UMI3DInputManager inputManager,
            UMI3DToolManager toolManager
        )
        {
            logger.MainContext = context;
            logger.MainTag = nameof(ProjectionManager);

            var treeId = (context.gameObject.GetInstanceID() + UnityEngine.Random.Range(0, 1000)).ToString();
            treeRoot = new ProjectionTreeNodeData()
            {
                treeId = treeId,
                id = 0,
                children = new(),
                interactionData = null,
                input = null
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
            logger.Assert(eventDelegate != null, $"{nameof(eventDelegate)} is null");

            ptManipulationNodeDelegate.Init(projectionTree_SO, treeId);
            ptEventNodeDelegate.Init(projectionTree_SO, treeId);
            ptFormNodeDelegate.Init(projectionTree_SO, treeId);
            ptLinkNodeDelegate.Init(projectionTree_SO, treeId);
            ptParameterNodeDelegate.Init(projectionTree_SO, treeId);

            logger.Assert(inputManager != null, $"{nameof(inputManager)} is null");
            logger.Assert(toolManager != null, $"{nameof(toolManager)} is null");

            this.inputManager = inputManager;
            this.toolManager = toolManager;
        }

        /// <summary>
        /// Get the inputs associated with this <paramref name="interactions"/>.
        /// </summary>
        /// <param name="interactions">the array of interaction for which an input is seeked</param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public AbstractUMI3DInput[] GetInputs(
            AbstractInteractionDto[] interactions,
            bool unused = true
        )
        {
            ProjectionTreeNodeData currentMemoryTreeState = treeRoot;

            List<AbstractUMI3DInput> selectedInputs = new();

            for (int depth = 0; depth < interactions.Length; depth++)
            {
                AbstractInteractionDto interaction = interactions[depth];

                if (interaction is ManipulationDto manipulationDto)
                {
                    DofGroupOptionDto[] options = manipulationDto.dofSeparationOptions.ToArray();
                    DofGroupOptionDto bestDofGroupOption = inputManager.manipulationDelegate.FindBest(options);

                    foreach (DofGroupDto sep in bestDofGroupOption.separations)
                    {
                        currentMemoryTreeState = ProjectAndUpdateTree(
                            interaction,
                            currentMemoryTreeState,
                            null,
                            null,
                            null,
                            unused,
                            false,
                            sep
                        );
                        selectedInputs.Add(currentMemoryTreeState.input);
                    }
                }
                else
                {
                    currentMemoryTreeState = ProjectAndUpdateTree(
                            interaction,
                            currentMemoryTreeState,
                            null,
                            null,
                            null,
                            unused,
                            false,
                            null
                        );
                    selectedInputs.Add(currentMemoryTreeState.input);
                }
            }

            return selectedInputs.ToArray();
        }

        /// <summary>
        /// Project an interaction and return associated input.
        /// </summary>
        /// <param name="controller">Controller to project on</param>
        /// <param name="evt">Event dto to project</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        public AbstractUMI3DInput Project<Dto>(
            Dto interaction,
            ulong environmentId,
            ulong toolId,
            ulong hoveredObjectId,
            bool unusedInputsOnly = false,
            DofGroupDto dof = null
        )
            where Dto : AbstractInteractionDto
        {
            return ProjectAndUpdateTree(
                interaction,
                treeRoot,
                environmentId,
                toolId,
                hoveredObjectId,
                unusedInputsOnly,
                false,
                dof
            ).input;
        }

        /// <summary>
        /// Project a set of interactions and return associated inputs.
        /// </summary>
        /// <param name="interactions">Interactions to project</param>
        /// <param name="environmentId"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        public AbstractUMI3DInput[] Project(
            AbstractInteractionDto[] interactions, 
            ulong environmentId, 
            ulong toolId, 
            ulong hoveredObjectId
        )
        {
            bool foundHoldableEvent = false;
            if (InteractionMapper.Instance.shouldProjectHoldableEventOnSpecificInput)
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

            ProjectionTreeNodeData currentMemoryTreeState = treeRoot;
            List<AbstractUMI3DInput> selectedInputs = new List<AbstractUMI3DInput>();

            for (int depth = 0; depth < interactions.Length; depth++)
            {
              AbstractInteractionDto interaction = interactions[depth];

                if (interaction is ManipulationDto manipulationDto)
                {
                    DofGroupOptionDto[] options = (interaction as ManipulationDto).dofSeparationOptions.ToArray();
                    DofGroupOptionDto bestDofGroupOption = inputManager.manipulationDelegate.FindBest(options);

                    foreach (DofGroupDto sep in bestDofGroupOption.separations)
                    {
                        currentMemoryTreeState = ProjectAndUpdateTree(
                            interaction,
                            currentMemoryTreeState,
                            environmentId,
                            toolId,
                            hoveredObjectId,
                            true,
                            false,
                            sep
                        );
                        selectedInputs.Add(currentMemoryTreeState.input);
                    }
                }
                else
                {
                    currentMemoryTreeState = ProjectAndUpdateTree(
                        interaction,
                        currentMemoryTreeState,
                        environmentId,
                        toolId,
                        hoveredObjectId,
                        true,
                        depth == 0 && foundHoldableEvent,
                        null
                    );
                    selectedInputs.Add(currentMemoryTreeState.input);
                }
            }

            return selectedInputs.ToArray();
        }

        /// <summary>
        /// Project a tool and its interaction.
        /// </summary>
        /// <param name="tool"> The ToolDto to be projected.</param>
        /// <see cref="Release(AbstractTool)"/>
        public void Project(
            AbstractTool tool, 
            bool releasable, 
            InteractionMappingReason reason, 
            ulong hoveredObjectId
        )
        {
            if (!toolManager.toolDelegate.IsCompatibleWith(tool))
            {
                throw new IncompatibleToolException($"For {tool.GetType().Name}: {tool.name}");
            }

            if (toolManager.toolDelegate.Tool.id == tool.id)
            {
                Release(
                    tool,
                    new ToolNeedToBeUpdated()
                );
            }

            if (toolManager.toolDelegate.Tool != null)
                throw new System.Exception("A tool is already projected !");

            if (toolManager.toolDelegate.RequiresMenu(tool))
            {
                toolManager.toolDelegate.CreateInteractionsMenuFor(tool);
            }
            else
            {
                AbstractInteractionDto[] interactions = tool.interactionsLoaded.ToArray();
                AbstractUMI3DInput[] inputs = Project(
                    interactions, 
                    tool.environmentId, 
                    tool.id, 
                    hoveredObjectId
                );
                //associatedInputs.Add(tool.id, inputs);
                eventDelegate.OnProjected(tool);
            }

            toolManager.toolDelegate.Tool = tool;
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
            if (toolManager.toolDelegate.Tool != tool)
            {
                throw new NoToolFoundException($"Try to update {tool.GetType().Name}: {tool.name} but this tool is not projected.");
            }

            if (toolManager.toolDelegate.RequiresMenu(tool))
            {
                toolManager.toolDelegate.CreateInteractionsMenuFor(tool);
            }
            else
            {
                AbstractUMI3DInput input = Project(
                    newInteraction,
                    tool.environmentId,
                    tool.id,
                    toolManager.toolDelegate.CurrentHoverTool.id
                );
                //if (associatedInputs.ContainsKey(tool.id))
                //{
                //    associatedInputs[tool.id] = associatedInputs[tool.id].Concat(inputs).ToArray();
                //}
                //else
                //{
                //    associatedInputs.Add(tool.id, new AbstractUMI3DInput[] { input });
                //}
            }
            toolManager.toolDelegate.Tool = tool;
        }

        /// <summary>
        /// Release a projected tool.
        /// </summary>
        /// <param name="tool">Tool to release</param>
        /// <see cref="Project(AbstractTool)"/>
        public void Release(AbstractTool tool, InteractionMappingReason reason)
        {
            if (toolManager.toolDelegate.Tool == null)
            {
                throw new NoToolFoundException($"No tool is currently projected on this controller");
                // TODO add controller id.
            }
            if (toolManager.toolDelegate.Tool.id != tool.id)
                throw new System.Exception("This tool is not currently projected on this controller");

            //if (associatedInputs.TryGetValue(tool.id, out AbstractUMI3DInput[] inputs))
            //{
            //    foreach (AbstractUMI3DInput input in inputs)
            //    {
            //        if (input.CurrentInteraction() != null)
            //            input.Dissociate();
            //    }
            //    associatedInputs.Remove(tool.id);
            //}
            toolManager.toolDelegate.Tool = null;
            eventDelegate.OnReleased(tool);
        }

        /// <summary>
        /// Updates the projection tree and project the interaction.<br/>
        /// 
        /// <b>Warning</b> interaction is associated with its input only if 
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
            bool unused = true,
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

                    ptManipulationNodeDelegate.sep = dof;
                    adequation = ptManipulationNodeDelegate.IsNodeCompatible(manipulationDto);
                    deepProjectionCreation = ptManipulationNodeDelegate.CreateNodeForInput(
                        manipulationDto,
                        () =>
                        {
                            inputManager.manipulationDelegate.dof = dof;
                            return inputManager.manipulationDelegate.FindInput(manipulationDto, unused);
                        }
                    );
                    chooseProjection = ptManipulationNodeDelegate.ChooseProjection(
                        environmentId, 
                        toolId, 
                        hoveredObjectId
                    );
                    break;

                case EventDto eventDto:

                    adequation = ptEventNodeDelegate.IsNodeCompatible(eventDto);
                    deepProjectionCreation = ptEventNodeDelegate.CreateNodeForInput(
                        eventDto,
                        () =>
                        {
                            inputManager.eventInputDelegate.tryToFindInputForHoldableEvent = tryToFindInputForHoldableEvent;
                            return inputManager.eventInputDelegate.FindInput(eventDto, unused);
                        }
                    );
                    chooseProjection = ptEventNodeDelegate.ChooseProjection(
                        environmentId,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case FormDto formDto:

                    adequation = ptFormNodeDelegate.IsNodeCompatible(formDto);
                    deepProjectionCreation = ptFormNodeDelegate.CreateNodeForInput(
                        formDto,
                        () =>
                        {
                            return inputManager.formInputDelegate.FindInput(formDto, unused);
                        }
                    );
                    chooseProjection = ptFormNodeDelegate.ChooseProjection(
                        environmentId,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case LinkDto linkDto:

                    adequation = ptLinkNodeDelegate.IsNodeCompatible(linkDto);
                    deepProjectionCreation = ptLinkNodeDelegate.CreateNodeForInput(
                        linkDto,
                        () =>
                        {
                            return inputManager.linkInputDelegate.FindInput(linkDto, unused);
                        }
                    );
                    chooseProjection = ptLinkNodeDelegate.ChooseProjection(
                        environmentId,
                        toolId,
                        hoveredObjectId
                    );
                    break;

                case AbstractParameterDto parameterDto:

                    adequation = ptParameterNodeDelegate.IsNodeCompatible(parameterDto);
                    deepProjectionCreation = ptParameterNodeDelegate.CreateNodeForInput(
                        parameterDto,
                        () =>
                        {
                            return inputManager.parameterInputDelegate.FindInput(parameterDto, unused);
                        }
                    );
                    chooseProjection = ptParameterNodeDelegate.ChooseProjection(
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
        /// <b>Warning</b> interaction is not necessary associated with an input.
        /// </summary>
        /// <param name="isAdequate">Whether the given projection node is adequate for the interaction to project</param>
        /// <param name="projectionNodeCreation">Create a new projection node, should throw an <see cref="NoInputFoundException"/> if no input is available</param>
        /// <param name="chooseProjection">Project the interaction to the given node's input</param>
        /// <param name="currentTreeNode">Current node in tree projection</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        /// <exception cref="NoInputFoundException"></exception>
        ProjectionTreeNodeData ProjectAndUpdateTree(
            ProjectionTreeNodeData currentTreeNode,
            Predicate<ProjectionTreeNodeData> isAdequate,
            Func<ProjectionTreeNodeData> projectionNodeCreation,
            Action<ProjectionTreeNodeData> chooseProjection,
            bool unusedInputsOnly = true
        )
        {
            // Try first to project on unused inputs.
            if (!unusedInputsOnly)
            {
                try
                {
                    return ProjectAndUpdateTree(
                        currentTreeNode,
                        isAdequate,
                        projectionNodeCreation,
                        chooseProjection,
                        true
                    );
                }
                catch (NoInputFoundException) { }
            }

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
                    if (unusedInputsOnly && !tmp.input.IsAvailable())
                    {
                        continue;
                    }
                    else
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
            eventDelegate.OnProjected(
                projection.Value.interactionData.Interaction,
                projection.Value.input
            );
            return projection.Value;
        }
    }
}