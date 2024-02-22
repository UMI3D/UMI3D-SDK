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
using System.Collections.Generic;
using System.Linq;
using umi3d.common.interaction;
using umi3d.debug;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Manage the links between projected tools and their associated inputs. 
    /// This projection is based on a tree <see cref="ProjectionTreeDto"/> constituted of <see cref="ProjectionTreeNodeDto"/>.
    /// </summary>
    public class ProjectionMemory : MonoBehaviour
    {
        [SerializeField]
        UMI3DLogger logger = new();
        public ProjectionTree_SO projectionTree_SO;
        public ProjectionTreeManipulationNodeDelegate ptManipulationNodeDelegate;
        public ProjectionTreeEventNodeDelegate ptEventNodeDelegate;
        public ProjectionTreeFormNodeDelegate ptFormNodeDelegate;
        public ProjectionTreeLinkNodeDelegate ptLinkNodeDelegate;
        public ProjectionTreeParameterNodeDelegate ptParameterNodeDelegate;

        string treeId = "";
        /// <summary>
        /// Id of the projection tree.
        /// </summary>
        public string TreeId
        {
            get
            {
                if (string.IsNullOrEmpty(treeId))
                {
                    treeId = (this.gameObject.GetInstanceID() + Random.Range(0, 1000)).ToString();
                }
                return treeId;
            }
        }
        /// <summary>
        /// The root of the tree.
        /// </summary>
        ProjectionTreeNodeDto treeRoot;
        ProjectionTreeModel treeModel;

        protected virtual void Awake()
        {
            logger.MainContext = this;
            logger.MainTag = nameof(ProjectionMemory);

            treeRoot = new ProjectionTreeNodeDto()
            {
                treeId = treeId,
                id = 0,
                children = new(),
                interactionDto = null,
                input = null
            };
            treeModel = new(
                projectionTree_SO,
                treeId
            );

            treeModel.AddRoot(treeRoot);
        }

        private void Start()
        {
            logger.Assert(ptManipulationNodeDelegate != null, $"{nameof(ptManipulationNodeDelegate)} is null");
            logger.Assert(ptEventNodeDelegate != null, $"{nameof(ptEventNodeDelegate)} is null");
            logger.Assert(ptFormNodeDelegate != null, $"{nameof(ptFormNodeDelegate)} is null");
            logger.Assert(ptLinkNodeDelegate != null, $"{nameof(ptLinkNodeDelegate)} is null");
            logger.Assert(ptParameterNodeDelegate != null, $"{nameof(ptParameterNodeDelegate)} is null");

            ptManipulationNodeDelegate.Init(projectionTree_SO, TreeId);
            ptEventNodeDelegate.Init(projectionTree_SO, TreeId);
            ptFormNodeDelegate.Init(projectionTree_SO, TreeId);
            ptLinkNodeDelegate.Init(projectionTree_SO, TreeId);
            ptParameterNodeDelegate.Init(projectionTree_SO, TreeId);
        }

        /// <summary>
        /// Get Inputs of a controller for a list of interactions.
        /// </summary>
        /// <param name="controller">The controller on which the input should be</param>
        /// <param name="interactions">the array of interaction for which an input is seeked</param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public AbstractUMI3DInput[] GetInputs(
            AbstractController controller,
            AbstractInteractionDto[] interactions,
            bool unused = true
        )
        {
            ProjectionTreeNodeDto currentMemoryTreeState = treeRoot;

            List<AbstractUMI3DInput> selectedInputs = new();

            for (int depth = 0; depth < interactions.Length; depth++)
            {
                AbstractInteractionDto interaction = interactions[depth];

                if (interaction is ManipulationDto manipulationDto)
                {
                    DofGroupOptionDto[] options = manipulationDto.dofSeparationOptions.ToArray();
                    DofGroupOptionDto bestDofGroupOption = controller.FindBest(options);

                    foreach (DofGroupDto sep in bestDofGroupOption.separations)
                    {
                        currentMemoryTreeState = GetProjectionNode(
                            controller,
                            interaction,
                            currentMemoryTreeState,
                            null,
                            null,
                            null,
                            selectedInputs,
                            unused,
                            false,
                            sep
                        );
                    }
                }
                else
                {
                    currentMemoryTreeState = GetProjectionNode(
                            controller,
                            interaction,
                            currentMemoryTreeState,
                            null,
                            null,
                            null,
                            selectedInputs,
                            unused,
                            false,
                            null
                        );
                }
            }

            return selectedInputs.ToArray();
        }

        /// <summary>
        /// Project a dto on a controller and return associated input.
        /// </summary>
        /// <param name="controller">Controller to project on</param>
        /// <param name="evt">Event dto to project</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        public AbstractUMI3DInput PartialProject<Dto>(
            Dto dto,
            AbstractController controller,
            ulong environmentId,
            ulong toolId,
            ulong hoveredObjectId,
            bool unusedInputsOnly = false,
            DofGroupDto dof = null
        )
            where Dto : AbstractInteractionDto
        {
            return GetProjectionNode(
                controller,
                dto,
                treeRoot,
                environmentId,
                toolId,
                hoveredObjectId,
                null,
                unusedInputsOnly,
                false,
                dof
            ).input;
        }

        /// <summary>
        /// Project on a given controller a set of interactions and return associated inputs.
        /// </summary>
        /// <param name="controller">Controller to project interactions on</param>
        /// <param name="interactions">Interactions to project</param>
        public AbstractUMI3DInput[] Project(
            AbstractController controller, 
            ulong environmentId, 
            AbstractInteractionDto[] interactions, 
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

            ProjectionTreeNodeDto currentMemoryTreeState = treeRoot;
            List<AbstractUMI3DInput> selectedInputs = new List<AbstractUMI3DInput>();

            for (int depth = 0; depth < interactions.Length; depth++)
            {
              AbstractInteractionDto interaction = interactions[depth];

                if (interaction is ManipulationDto manipulationDto)
                {
                    DofGroupOptionDto[] options = (interaction as ManipulationDto).dofSeparationOptions.ToArray();
                    DofGroupOptionDto bestDofGroupOption = controller.FindBest(options);

                    foreach (DofGroupDto sep in bestDofGroupOption.separations)
                    {
                        currentMemoryTreeState = GetProjectionNode(
                            controller,
                            interaction,
                            currentMemoryTreeState,
                            environmentId,
                            toolId,
                            hoveredObjectId,
                            selectedInputs,
                            true,
                            false,
                            sep
                        );
                    }
                }
                else
                {
                    currentMemoryTreeState = GetProjectionNode(
                        controller,
                        interaction,
                        currentMemoryTreeState,
                        environmentId,
                        toolId,
                        hoveredObjectId,
                        selectedInputs,
                        true,
                        depth == 0 && foundHoldableEvent,
                        null
                    );
                }
            }

            return selectedInputs.ToArray();
        }

        /// <summary>
        /// Return projection node.<br/>
        /// If <paramref name="environmentId"/>, <paramref name="hoveredObjectId"/>, <paramref name="toolId"/> are not null then associate the interaction with its input.<br/>
        /// If <paramref name="selectedInputs"/> is not null add the found input to the list.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="interaction"></param>
        /// <param name="currentMemoryTreeState"></param>
        /// <param name="environmentId"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="selectedInputs"></param>
        /// <param name="unused"></param>
        /// <param name="tryToFindInputForHoldableEvent"></param>
        /// <param name="dof"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        ProjectionTreeNodeDto GetProjectionNode(
            AbstractController controller,
            AbstractInteractionDto interaction,
            ProjectionTreeNodeDto currentMemoryTreeState,
            ulong? environmentId = null,
            ulong? toolId = null,
            ulong? hoveredObjectId = null,
            List<AbstractUMI3DInput> selectedInputs = null,
            bool unused = true,
            bool tryToFindInputForHoldableEvent = false,
            DofGroupDto dof = null
        )
        {
            System.Predicate<ProjectionTreeNodeDto> adequation;
            System.Func<ProjectionTreeNodeDto> deepProjectionCreation;
            System.Action<ProjectionTreeNodeDto> chooseProjection;

            switch (interaction)
            {
                case ManipulationDto manipulationDto:

                    ptManipulationNodeDelegate.sep = dof;
                    adequation = ptManipulationNodeDelegate.IsNodeCompatible(manipulationDto);
                    deepProjectionCreation = ptManipulationNodeDelegate.CreateNodeForInput(
                        manipulationDto,
                        () =>
                        {
                            return controller.FindInput(manipulationDto, dof, unused);
                        }
                    );
                    chooseProjection = ptManipulationNodeDelegate.ChooseProjection(environmentId, toolId, hoveredObjectId, selectedInputs);
                    break;

                case EventDto eventDto:

                    adequation = ptEventNodeDelegate.IsNodeCompatible(eventDto);
                    deepProjectionCreation = ptEventNodeDelegate.CreateNodeForInput(
                        eventDto,
                        () =>
                        {
                            return controller.FindInput(eventDto, unused, tryToFindInputForHoldableEvent);
                        }
                    );
                    chooseProjection = ptEventNodeDelegate.ChooseProjection(environmentId, toolId, hoveredObjectId, selectedInputs);
                    break;

                case FormDto formDto:

                    adequation = ptFormNodeDelegate.IsNodeCompatible(formDto);
                    deepProjectionCreation = ptFormNodeDelegate.CreateNodeForInput(
                        formDto,
                        () =>
                        {
                            return controller.FindInput(formDto, unused);
                        }
                    );
                    chooseProjection = ptFormNodeDelegate.ChooseProjection(environmentId, toolId, hoveredObjectId, selectedInputs);
                    break;

                case LinkDto linkDto:

                    adequation = ptLinkNodeDelegate.IsNodeCompatible(linkDto);
                    deepProjectionCreation = ptLinkNodeDelegate.CreateNodeForInput(
                        linkDto,
                        () =>
                        {
                            return controller.FindInput(linkDto, unused);
                        }
                    );
                    chooseProjection = ptLinkNodeDelegate.ChooseProjection(environmentId, toolId, hoveredObjectId, selectedInputs);
                    break;

                case AbstractParameterDto parameterDto:

                    adequation = ptParameterNodeDelegate.IsNodeCompatible(parameterDto);
                    deepProjectionCreation = ptParameterNodeDelegate.CreateNodeForInput(
                        parameterDto,
                        () =>
                        {
                            return controller.FindInput(parameterDto, unused);
                        }
                    );
                    chooseProjection = ptParameterNodeDelegate.ChooseProjection(environmentId, toolId, hoveredObjectId, selectedInputs);
                    break;

                default:
                    throw new System.Exception("Unknown interaction type, can't project !");
            }

            return Project(
                currentMemoryTreeState,
                adequation,
                deepProjectionCreation,
                chooseProjection
            );
        }

        /// <summary>
        /// Navigates through tree and project an interaction. Updates the tree if necessary.
        /// </summary>
        /// <param name="nodeAdequationTest">Decides if the given projection node is adequate for the interaction to project</param>
        /// <param name="deepProjectionCreation">Create a new deep projection node, should throw an <see cref="NoInputFoundException"/> if no input is available</param>
        /// <param name="chooseProjection">Project the interaction to the given node's input</param>
        /// <param name="currentTreeNode">Current node in tree projection</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        /// <exception cref="NoInputFoundException"></exception>
        private ProjectionTreeNodeDto Project(
            ProjectionTreeNodeDto currentTreeNode,
            System.Predicate<ProjectionTreeNodeDto> nodeAdequationTest,
            System.Func<ProjectionTreeNodeDto> deepProjectionCreation,
            System.Action<ProjectionTreeNodeDto> chooseProjection,
            bool unusedInputsOnly = true,
            bool updateMemory = true
        )
        {
            // Try first to project on unused inputs.
            if (!unusedInputsOnly)
            {
                try
                {
                    return Project(
                        currentTreeNode,
                        nodeAdequationTest,
                        deepProjectionCreation,
                        chooseProjection,
                        true,
                        updateMemory
                    );
                }
                catch (NoInputFoundException) { }
            }

            ProjectionTreeNodeDto? projection;
            ///<summary>
            /// Return 1 when projection has been found.<br/>
            /// Return 0 when projection has been found but the input is not available.<br/>
            /// Return -1 when projection has not been found.
            /// </summary>
            int _FindProjection(List<ProjectionTreeNodeDto> children, out ProjectionTreeNodeDto? projection)
            {
                IEnumerable<int> indexes = Enumerable
                    .Range(0, children.Count)
                    .Where(i =>
                    {
                        return nodeAdequationTest?.Invoke(children[i]) ?? false;
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

            int projectionStatus = _FindProjection(treeModel.GetAllSubNodes(currentTreeNode), out projection);
            if (projectionStatus == -1) 
            {
                projectionStatus = _FindProjection(treeModel.GetAllSubNodes(treeRoot), out projection);
                if (projectionStatus <= 0)
                {
                    projection = deepProjectionCreation();

                    if (updateMemory)
                    {
                        treeModel.AddChild(currentTreeNode.id, projection.Value);
                    }
                }
                else
                {
                    if (updateMemory)
                    {
                        treeModel.RemoveChild(projection.Value.parentId, projection.Value.id);
                        treeModel.AddChild(currentTreeNode.id, projection.Value);
                    }
                }
            }
            else if (projectionStatus == 1)
            {
                if (updateMemory)
                {
                    treeModel.RemoveChild(projection.Value.parentId, projection.Value.id);
                    treeModel.AddChild(currentTreeNode.id, projection.Value);
                }
            }

            chooseProjection(projection.Value);
            return projection.Value;
        }
    }
}