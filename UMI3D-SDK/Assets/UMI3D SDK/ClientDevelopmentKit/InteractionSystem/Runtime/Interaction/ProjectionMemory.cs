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
using System.IO;
using umi3d.common.interaction;
using UnityEngine;


namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Saves and manages the links between projected tools and their associated inputs. 
    /// This projection is based on a tree constituted of <see cref="ProjectionTreeNode"/>.
    /// </summary>
    public class ProjectionMemory : MonoBehaviour
    {
        protected string id_ = "";
        public string id
        {
            get
            {
                if (id_.Equals(""))
                    id_ = (this.gameObject.GetInstanceID() + Random.Range(0, 1000)).ToString();
                return id_;
            }
        }

        /// <summary>
        /// Projection memory.
        /// </summary>
        protected ProjectionTreeNode memoryRoot;

        protected virtual void Awake()
        {
            memoryRoot = new ProjectionTreeNode(id) { id = 0 };
        }


        /// <summary>
        /// Get Inputs of a controller for a list of interactions.
        /// </summary>
        /// <param name="controller">The controller on which the input should be</param>
        /// <param name="interactions">the array of interaction for which an input is seeked</param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public AbstractUMI3DInput[] GetInputs(AbstractController controller, AbstractInteractionDto[] interactions, bool unused = true)
        {
            ProjectionTreeNode currentMemoryTreeState = memoryRoot;

            System.Func<ProjectionTreeNode> deepProjectionCreation;
            System.Predicate<ProjectionTreeNode> adequation;
            System.Action<ProjectionTreeNode> chooseProjection;

            var selectedInputs = new List<AbstractUMI3DInput>();

            for (int depth = 0; depth < interactions.Length; depth++)
            {
                AbstractInteractionDto interaction = interactions[depth];

                switch (interaction)
                {
                    case ManipulationDto manipulationDto:

                        DofGroupOptionDto[] options = (interaction as ManipulationDto).dofSeparationOptions.ToArray();
                        DofGroupOptionDto bestDofGoupOption = controller.FindBest(options);

                        foreach (DofGroupDto sep in bestDofGoupOption.separations)
                        {
                            adequation = node =>
                            {
                                return (node is ManipulationNode) && ((node as ManipulationNode).manipulationDofGroupDto.dofs == sep.dofs);
                            };

                            deepProjectionCreation = () =>
                            {
                                AbstractUMI3DInput projection = controller.FindInput(manipulationDto, sep, unused);

                                if (projection == null)
                                    throw new NoInputFoundException();

                                return new ManipulationNode(id)
                                {
                                    id = manipulationDto.id,
                                    manipulation = manipulationDto,
                                    manipulationDofGroupDto = sep,
                                    projectedInput = projection
                                };
                            };

                            chooseProjection = node =>
                            {
                                selectedInputs.Add(node.projectedInput);
                            };

                            currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        }
                        break;

                    case EventDto eventDto:

                        adequation = node =>
                        {
                            return (node is EventNode) && (node as EventNode).evt.name.Equals(interaction.name);
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(eventDto, unused);

                            if (projection == null)
                                throw new NoInputFoundException();
                            return new EventNode(id)
                            {
                                id = eventDto.id,
                                evt = eventDto,
                                projectedInput = projection
                            };
                        };

                        chooseProjection = node =>
                        {
                            if (node == null)
                                throw new System.Exception("Internal error");
                            if (node.projectedInput == null)
                            {
                                throw new NoInputFoundException();
                            }

                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    case FormDto formDto:

                        adequation = node =>
                        {
                            return (node is FormNode) && (node as FormNode).form.name.Equals(interaction.name);
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(formDto, unused);

                            if (projection == null)
                                throw new NoInputFoundException();
                            return new FormNode(id)
                            {
                                id = formDto.id,
                                form = formDto,
                                projectedInput = projection
                            };
                        };

                        chooseProjection = node =>
                        {
                            if (node == null)
                                throw new System.Exception("Internal error");
                            if (node.projectedInput == null)
                            {
                                throw new NoInputFoundException();
                            }

                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    case LinkDto linkDto:

                        adequation = node =>
                        {
                            return (node is LinkNode) && (node as LinkNode).link.name.Equals(interaction.name);
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(linkDto, unused);

                            if (projection == null)
                                throw new NoInputFoundException();
                            return new LinkNode(id)
                            {
                                id = linkDto.id,
                                link = linkDto,
                                projectedInput = projection
                            };
                        };

                        chooseProjection = node =>
                        {
                            if (node == null)
                                throw new System.Exception("Internal error");
                            if (node.projectedInput == null)
                            {
                                throw new NoInputFoundException();
                            }

                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    case AbstractParameterDto parameterDto:

                        adequation = node =>
                        {
                            return (node is ParameterNode)
                                && (node as ParameterNode).parameter.GetType().Equals(parameterDto.GetType());
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(parameterDto, unused);

                            if (projection == null)
                                throw new NoInputFoundException();

                            var param = new ParameterNode(id)
                            {
                                id = parameterDto.id,
                                parameter = parameterDto,
                                projectedInput = projection
                            };

                            return param;
                        };

                        chooseProjection = node =>
                        {
                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    default:
                        throw new System.Exception("Unknown interaction type, can't project !");
                }
            }

            return selectedInputs.ToArray();
        }

        /// <summary>
        /// Project a manipulation dof on a given controller and return associated input.
        /// </summary>
        /// <param name="controller">Controller to project on</param>
        /// <param name="manip">Manipulation to project</param>
        /// <param name="dof">Dof to project</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        public AbstractUMI3DInput PartialProject(AbstractController controller, ManipulationDto manip, DofGroupDto dof, bool unusedInputsOnly, ulong toolId, ulong hoveredObjectId)
        {
            ProjectionTreeNode currentMemoryTreeState = memoryRoot;

            DofGroupOptionDto[] options = manip.dofSeparationOptions.ToArray();
            DofGroupOptionDto bestDofGoupOption = controller.FindBest(options);

            System.Predicate<ProjectionTreeNode> adequation = node =>
            {
                return (node is ManipulationNode) && ((node as ManipulationNode).manipulationDofGroupDto.dofs == dof.dofs);
            };

            System.Func<ProjectionTreeNode> deepProjectionCreation = () =>
            {
                AbstractUMI3DInput projection = controller.FindInput(manip, dof, unusedInputsOnly);

                if (projection == null)
                    throw new NoInputFoundException();

                return new ManipulationNode(id)
                {
                    id = manip.id,
                    manipulation = manip,
                    manipulationDofGroupDto = dof,
                    projectedInput = projection
                };
            };

            System.Action<ProjectionTreeNode> chooseProjection = node =>
            {
                if (!node.projectedInput.IsAvailable())
                {
                    if (!unusedInputsOnly)
                        node.projectedInput.Dissociate();
                    else
                        throw new System.Exception("Internal error");
                }
                node.projectedInput.Associate(manip, dof.dofs, toolId, hoveredObjectId);
            };

            currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection, unusedInputsOnly);
            return currentMemoryTreeState.projectedInput;
        }

        /// <summary>
        /// Project a form dto on a controller and return associated input.
        /// </summary>
        /// <param name="controller">Controller to project on</param>
        /// <param name="form">form dto to project</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        public AbstractUMI3DInput PartialProject(AbstractController controller, FormDto form, ulong toolId, ulong hoveredObjectId, bool unusedInputsOnly = false)
        {
            System.Func<ProjectionTreeNode> deepProjectionCreation = () =>
            {
                AbstractUMI3DInput projection = controller.FindInput(form, true);
                if ((projection == null) && !unusedInputsOnly)
                    projection = controller.FindInput(form, false);

                if (projection == null)
                    throw new NoInputFoundException();

                return new FormNode(id)
                {
                    id = form.id,
                    form = form,
                    projectedInput = projection
                };
            };

            System.Predicate<ProjectionTreeNode> adequation = node =>
            {
                return (node is FormNode) && (node as FormNode).form.name.Equals(form.name);
            };

            System.Action<ProjectionTreeNode> chooseProjection = node =>
            {
                if (!node.projectedInput.IsAvailable())
                {
                    if (!unusedInputsOnly)
                        node.projectedInput.Dissociate();
                    else
                        throw new System.Exception("Internal error");
                }
                node.projectedInput.Associate(form, toolId, hoveredObjectId);
            };

            return Project(memoryRoot, adequation, deepProjectionCreation, chooseProjection, unusedInputsOnly).projectedInput;
        }

        /// <summary>
        /// Project a link dto on a controller and return associated input.
        /// </summary>
        /// <param name="controller">Controller to project on</param>
        /// <param name="link">link dto to project</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        public AbstractUMI3DInput PartialProject(AbstractController controller, LinkDto link, ulong toolId, ulong hoveredObjectId, bool unusedInputsOnly = false)
        {
            System.Func<ProjectionTreeNode> deepProjectionCreation = () =>
            {
                AbstractUMI3DInput projection = controller.FindInput(link, true);
                if ((projection == null) && !unusedInputsOnly)
                    projection = controller.FindInput(link, false);

                if (projection == null)
                    throw new NoInputFoundException();

                return new LinkNode(id)
                {
                    id = link.id,
                    link = link,
                    projectedInput = projection
                };
            };

            System.Predicate<ProjectionTreeNode> adequation = node =>
            {
                return (node is LinkNode) && (node as LinkNode).link.name.Equals(link.name);
            };

            System.Action<ProjectionTreeNode> chooseProjection = node =>
            {
                if (!node.projectedInput.IsAvailable())
                {
                    if (!unusedInputsOnly)
                        node.projectedInput.Dissociate();
                    else
                        throw new System.Exception("Internal error");
                }
                node.projectedInput.Associate(link, toolId, hoveredObjectId);
            };

            return Project(memoryRoot, adequation, deepProjectionCreation, chooseProjection, unusedInputsOnly).projectedInput;
        }

        /// <summary>
        /// Project an event dto on a controller and return associated input.
        /// </summary>
        /// <param name="controller">Controller to project on</param>
        /// <param name="evt">Event dto to project</param>
        /// <param name="unusedInputsOnly">Project on unused inputs only</param>
        public AbstractUMI3DInput PartialProject(AbstractController controller, EventDto evt, ulong toolId, ulong hoveredObjectId, bool unusedInputsOnly = false)
        {
            System.Func<ProjectionTreeNode> deepProjectionCreation = () =>
            {
                AbstractUMI3DInput projection = controller.FindInput(evt, true);
                if ((projection == null) && !unusedInputsOnly)
                    projection = controller.FindInput(evt, false);

                if (projection == null)
                    throw new NoInputFoundException();

                return new EventNode(id)
                {
                    id = evt.id,
                    evt = evt,
                    projectedInput = projection
                };
            };

            System.Predicate<ProjectionTreeNode> adequation = node =>
            {
                return (node is EventNode) && (node as EventNode).evt.name.Equals(evt.name);
            };

            System.Action<ProjectionTreeNode> chooseProjection = node =>
            {
                if (!node.projectedInput.IsAvailable())
                {
                    if (!unusedInputsOnly)
                        node.projectedInput.Dissociate();
                    else
                        throw new System.Exception("Internal error");
                }
                node.projectedInput.Associate(evt, toolId, hoveredObjectId);
            };

            return Project(memoryRoot, adequation, deepProjectionCreation, chooseProjection, unusedInputsOnly).projectedInput;
        }

        /// <summary>
        /// Project on a given controller a set of interactions and return associated inputs.
        /// </summary>
        /// <param name="controller">Controller to project interactions on</param>
        /// <param name="interactions">Interactions to project</param>
        public AbstractUMI3DInput[] Project(AbstractController controller, AbstractInteractionDto[] interactions, ulong toolId, ulong hoveredObjectId)
        {
            ProjectionTreeNode currentMemoryTreeState = memoryRoot;

            System.Func<ProjectionTreeNode> deepProjectionCreation;
            System.Predicate<ProjectionTreeNode> adequation;
            System.Action<ProjectionTreeNode> chooseProjection;

            var selectedInputs = new List<AbstractUMI3DInput>();

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
                    interactions = temp.ToArray();
            }

            for (int depth = 0; depth < interactions.Length; depth++)
            {
                AbstractInteractionDto interaction = interactions[depth];

                switch (interaction)
                {
                    case ManipulationDto manipulationDto:

                        DofGroupOptionDto[] options = (interaction as ManipulationDto).dofSeparationOptions.ToArray();
                        DofGroupOptionDto bestDofGoupOption = controller.FindBest(options);

                        foreach (DofGroupDto sep in bestDofGoupOption.separations)
                        {
                            adequation = node =>
                            {
                                return (node is ManipulationNode) && ((node as ManipulationNode).manipulationDofGroupDto.dofs == sep.dofs);
                            };

                            deepProjectionCreation = () =>
                            {
                                AbstractUMI3DInput projection = controller.FindInput(manipulationDto, sep, true);

                                if (projection == null)
                                    throw new NoInputFoundException();

                                return new ManipulationNode(id)
                                {
                                    id = manipulationDto.id,
                                    manipulation = manipulationDto,
                                    manipulationDofGroupDto = sep,
                                    projectedInput = projection
                                };
                            };

                            chooseProjection = node =>
                            {
                                node.projectedInput.Associate(interaction as ManipulationDto, sep.dofs, toolId, hoveredObjectId);
                                selectedInputs.Add(node.projectedInput);
                            };

                            currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);

                        }
                        break;

                    case EventDto eventDto:

                        adequation = node =>
                        {
                            return (node is EventNode) && (node as EventNode).evt.name.Equals(interaction.name);
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(eventDto, true, depth == 0 && foundHoldableEvent);

                            if (projection == null)
                                throw new NoInputFoundException();
                            return new EventNode(id)
                            {
                                id = eventDto.id,
                                evt = eventDto,
                                projectedInput = projection
                            };
                        };

                        chooseProjection = node =>
                        {
                            if (node == null)
                                throw new System.Exception("Internal error");
                            if (node.projectedInput == null)
                            {
                                throw new System.Exception("No input found");
                            }

                            node.projectedInput.Associate(interaction, toolId, hoveredObjectId);
                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    case FormDto formDto:

                        adequation = node =>
                        {
                            return (node is FormNode) && (node as FormNode).form.name.Equals(interaction.name);
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(formDto, true);

                            if (projection == null)
                                throw new NoInputFoundException();
                            return new FormNode(id)
                            {
                                id = formDto.id,
                                form = formDto,
                                projectedInput = projection
                            };
                        };

                        chooseProjection = node =>
                        {
                            if (node == null)
                                throw new System.Exception("Internal error");
                            if (node.projectedInput == null)
                            {
                                throw new System.Exception("No input found");
                            }

                            node.projectedInput.Associate(interaction, toolId, hoveredObjectId);
                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    case LinkDto linkDto:

                        adequation = node =>
                        {
                            return (node is LinkNode) && (node as LinkNode).link.name.Equals(interaction.name);
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(linkDto, true);

                            if (projection == null)
                                throw new NoInputFoundException();
                            return new LinkNode(id)
                            {
                                id = linkDto.id,
                                link = linkDto,
                                projectedInput = projection
                            };
                        };

                        chooseProjection = node =>
                        {
                            if (node == null)
                                throw new System.Exception("Internal error");
                            if (node.projectedInput == null)
                            {
                                throw new System.Exception("No input found");
                            }

                            node.projectedInput.Associate(interaction, toolId, hoveredObjectId);
                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    case AbstractParameterDto parameterDto:

                        adequation = node =>
                        {
                            return (node is ParameterNode)
                                && (node as ParameterNode).parameter.GetType().Equals(parameterDto.GetType());
                        };

                        deepProjectionCreation = () =>
                        {
                            AbstractUMI3DInput projection = controller.FindInput(parameterDto, true);

                            if (projection == null)
                                throw new NoInputFoundException();

                            var param = new ParameterNode(id)
                            {
                                id = parameterDto.id,
                                parameter = parameterDto,
                                projectedInput = projection
                            };

                            return param;
                        };

                        chooseProjection = node =>
                        {
                            node.projectedInput.Associate(interaction, toolId, hoveredObjectId);
                            selectedInputs.Add(node.projectedInput);
                        };

                        currentMemoryTreeState = Project(currentMemoryTreeState, adequation, deepProjectionCreation, chooseProjection);
                        break;

                    default:
                        throw new System.Exception("Unknown interaction type : " + interaction);
                }
            }

            return selectedInputs.ToArray();
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
        private ProjectionTreeNode Project(ProjectionTreeNode currentTreeNode,
            System.Predicate<ProjectionTreeNode> nodeAdequationTest,
            System.Func<ProjectionTreeNode> deepProjectionCreation,
            System.Action<ProjectionTreeNode> chooseProjection,
            bool unusedInputsOnly = true,
            bool updateMemory = true)
        {
            if (!unusedInputsOnly)
            {
                try
                {
                    ProjectionTreeNode p = Project(currentTreeNode, nodeAdequationTest, deepProjectionCreation, chooseProjection, true, updateMemory);
                    return p;
                }
                catch (NoInputFoundException) { }
            }

            ProjectionTreeNode deepProjection = currentTreeNode.children.Find(nodeAdequationTest);
            if (deepProjection != null)
            {
                if (unusedInputsOnly && !deepProjection.projectedInput.IsAvailable())
                {
                    ProjectionTreeNode alternativeProjection = deepProjectionCreation();
                    chooseProjection(alternativeProjection);
                    return alternativeProjection;
                }
                else
                {
                    chooseProjection(deepProjection);
                    return deepProjection;
                }
            }
            else
            {
                ProjectionTreeNode rootProjection = memoryRoot.children.Find(nodeAdequationTest);
                if ((rootProjection == null) || (unusedInputsOnly && !rootProjection.projectedInput.IsAvailable()))
                {
                    deepProjection = deepProjectionCreation();
                    chooseProjection(deepProjection);
                    if (updateMemory)
                        currentTreeNode.AddChild(deepProjection);
                    return deepProjection;
                }
                else
                {
                    chooseProjection(rootProjection);
                    if (updateMemory)
                        currentTreeNode.AddChild(rootProjection);
                    return rootProjection;
                }
            }
        }

        /// <summary>
        /// Save current state of the memory.
        /// </summary>
        /// <param name="path">path to the file</param>
        public void SaveToFile(string path)
        {
            memoryRoot.SaveToFile(path);
        }

        /// <summary>
        /// Load a state of memory.
        /// </summary>
        /// <param name="path">path to the file</param>
        public void LoadFromFile(string path)
        {
            memoryRoot.LoadFromFile(path);
        }

        /// <summary>
        /// Exception thrown when not associated input has been found for an interaction.
        /// </summary>
        public class NoInputFoundException : System.Exception
        {
            public NoInputFoundException() { }
            public NoInputFoundException(string message) : base(message) { }
            public NoInputFoundException(string message, System.Exception inner) : base(message, inner) { }
        }
    }



    /// <summary>
    /// Projection tree node for projection memory.
    /// </summary>
    [System.Serializable]
    public class ProjectionTreeNode
    {
        /// <summary>
        /// Nodes collection by tree id.
        /// </summary>
        [SerializeField]
        protected static Dictionary<string, Dictionary<ulong, ProjectionTreeNode>> nodesByTree = new Dictionary<string, Dictionary<ulong, ProjectionTreeNode>>();

        /// <summary>
        /// Node's children. Please avoid calling this field too often as it is slow to compute.
        /// </summary>
        public List<ProjectionTreeNode> children
        {
            get
            {
                var buffer = new List<ProjectionTreeNode>();
                foreach (ulong id in childrensId)
                {
                    if (nodesByTree.TryGetValue(treeId, out Dictionary<ulong, ProjectionTreeNode> nodes))
                    {
                        if (nodes.TryGetValue(id, out ProjectionTreeNode child))
                            buffer.Add(child);
                    }
                }
                return buffer;
            }
        }

        /// <summary>
        /// Node's children IDs.
        /// </summary>
        [SerializeField]
        protected List<ulong> childrensId = new List<ulong>();

        /// <summary>
        /// Node id.
        /// </summary>
        [SerializeField]
        public ulong id;

        /// <summary>
        /// Tree's id the node belongs to.
        /// </summary>
        public string treeId { get; protected set; }

        /// <summary>
        /// UMI3D Input projected to this node. 
        /// </summary>
        [SerializeField]
        public AbstractUMI3DInput projectedInput;


        public ProjectionTreeNode(string treeId)
        {
            this.treeId = treeId;
        }

        /// <summary>
        /// Add a child to this node.
        /// </summary>
        /// <param name="child">Node to add</param>
        public void AddChild(ProjectionTreeNode child)
        {
            if (childrensId.Contains(child.id))
                return;
            childrensId.Add(child.id);
            if (nodesByTree.TryGetValue(treeId, out Dictionary<ulong, ProjectionTreeNode> nodes))
            {
                if (!nodes.ContainsKey(child.id))
                {
                    nodes.Add(child.id, child);
                }
            }
        }

        /// <summary>
        /// save a node state to a file.
        /// </summary>
        /// <param name="path">path to the file</param>
        public void SaveToFile(string path)
        {
            if (nodesByTree.TryGetValue(treeId, out Dictionary<ulong, ProjectionTreeNode> nodes))
            {

                string objectJson = JsonUtility.ToJson(this, true);
                string staticRefIds = JsonUtility.ToJson(nodes.Keys, true);
                string storage = objectJson;// + "@\n" + static_json;

                var writer = new StreamWriter(path);
                writer.Write(storage);
                writer.Close();
            }
        }

        /// <summary>
        /// Load A node state from a file
        /// </summary>
        /// <param name="path">path to the file</param>
        public void LoadFromFile(string path)
        {
            if (nodesByTree.TryGetValue(treeId, out Dictionary<ulong, ProjectionTreeNode> nodes))
            {
                var reader = new StreamReader(path);
                string storage = reader.ReadToEnd();
                string[] buffer = storage.Split('@');

                string object_json = buffer[0];
                string static_json = buffer[1];

                JsonUtility.FromJsonOverwrite(static_json, nodes);
                JsonUtility.FromJsonOverwrite(object_json, this);
            }
        }
    }

    /// <summary>
    /// Projection tree node associated to an <see cref="EventDto"/>.
    /// </summary>
    [System.Serializable]
    public class EventNode : ProjectionTreeNode
    {
        /// <summary>
        /// Associated Event DTO
        /// </summary>
        [SerializeField, Tooltip("Associated Event DTO")]
        public EventDto evt;

        public EventNode(string treeId) : base(treeId) { }
    }

    /// <summary>
    /// Projection tree node associated to a <see cref="ManipulationDto"/>.
    /// </summary>
    [System.Serializable]
    public class ManipulationNode : ProjectionTreeNode
    {
        /// <summary>
        /// Associated Manipulation DTO
        /// </summary>
        [SerializeField, Tooltip("Associated Manipulation DTO")]
        public ManipulationDto manipulation;

        /// <summary>
        /// Associated Degree of Freedom Group DTO
        /// </summary>
        [SerializeField, Tooltip("Associated Degree of Freedom Group DTO")]
        public DofGroupDto manipulationDofGroupDto;

        public ManipulationNode(string treeId) : base(treeId) { }
    }

    /// <summary>
    /// Projection tree node associated to a <see cref="FormDto"/>.
    /// </summary>
    [System.Serializable]
    public class FormNode : ProjectionTreeNode
    {
        /// <summary>
        /// Associated Form DTO
        /// </summary>
        [SerializeField, Tooltip("Associated Form DTO")]
        public FormDto form;

        public FormNode(string treeId) : base(treeId) { }
    }

    /// <summary>
    /// Projection tree node associated to a <see cref="LinkDto"/>.
    /// </summary>
    [System.Serializable]
    public class LinkNode : ProjectionTreeNode
    {
        /// <summary>
        /// Associated Link DTO
        /// </summary>
        [SerializeField, Tooltip("Associated Link DTO")]
        public LinkDto link;

        public LinkNode(string treeId) : base(treeId) { }
    }

    /// <summary>
    /// Projection tree node associated to an <see cref="AbstractParameterDto"/>.
    /// </summary>
    [System.Serializable]
    public class ParameterNode : ProjectionTreeNode
    {
        /// <summary>
        /// Associated Parameter DTO
        /// </summary>
        [SerializeField, Tooltip("Associated Parameter DTO")]
        public AbstractParameterDto parameter;

        public ParameterNode(string treeId) : base(treeId) { }
    }
}