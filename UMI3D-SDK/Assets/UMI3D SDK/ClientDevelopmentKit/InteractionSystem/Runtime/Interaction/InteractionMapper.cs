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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.menu;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public class InteractionMapper : AbstractInteractionMapper
    {
        public static new InteractionMapper Instance => AbstractInteractionMapper.Instance as InteractionMapper;

        /// <summary>
        /// Menu to store toolboxes into.
        /// </summary>
        public Menu toolboxMenu;

        #region Data

        /// <summary>
        /// Associate a toolid with the controller the tool is projected on.
        /// </summary>
        public Dictionary<ulong, AbstractController> toolIdToController { get; protected set; } = new Dictionary<ulong, AbstractController>();

        /// <summary>
        /// Id to Dto interactions map.
        /// </summary>
        public Dictionary<ulong, AbstractInteractionDto> interactionsIdToDto { get; protected set; } = new Dictionary<ulong, AbstractInteractionDto>();

        /// <summary>
        /// Currently projected tools.
        /// </summary>
        private readonly Dictionary<ulong, InteractionMappingReason> projectedTools = new Dictionary<ulong, InteractionMappingReason>();

        #endregion


        /// <inheritdoc/>
        public override void ResetModule()
        {
            foreach (AbstractController c in Controllers)
                c.Clear();

            if (toolboxMenu != null)
            {
                toolboxMenu.RemoveAllSubMenu();
                toolboxMenu.RemoveAllMenuItem();
            }

            toolIdToController = new Dictionary<ulong, AbstractController>();
        }

        /// <summary>
        /// Select the best compatible controller for a given tool (not necessarily available).
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        protected AbstractController GetController(AbstractTool tool)
        {
            foreach (AbstractController controller in Controllers)
            {
                if (controller.IsCompatibleWith(tool) && controller.IsAvailableFor(tool))
                {
                    return controller;
                }
            }

            foreach (AbstractController controller in Controllers)
            {
                if (controller.IsCompatibleWith(tool))
                {
                    return controller;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override void ReleaseTool(ulong toolId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(toolId);

            if (toolIdToController.TryGetValue(tool.id, out AbstractController controller))
            {
                controller.Release(tool, reason);
                toolIdToController.Remove(tool.id);
                tool.OnUpdated.RemoveAllListeners();
                projectedTools.Remove(tool.id);
            }
            else
            {
                throw new Exception("Tool not selected");
            }
        }

        /// <inheritdoc/>
        public override bool SelectTool(ulong toolId, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(toolId);
            if (tool == null)
                throw new Exception("tool does not exist");

            if (toolIdToController.ContainsKey(tool.id))
            {
                throw new Exception("Tool already projected");
            }

            AbstractController controller = GetController(tool);
            if (controller != null)
            {
                if (!controller.IsAvailableFor(tool))
                {
                    if (ShouldForceProjection(controller, tool, reason))
                    {
                        ReleaseTool(controller.tool.id);
                    }
                    else
                    {
                        return false;
                    }
                }

                return SelectTool(tool.id, releasable, controller, hoveredObjectId, reason);
            }
            else
            {
                throw new Exception("No controller is compatible with this tool");
            }
        }

        /// <summary>
        /// Request the selection of a Tool for a given controller.
        /// Be careful, this method could be called before the tool is added for async loading reasons.
        /// </summary>
        /// <param name="tool">The tool to select</param>
        /// <param name="controller">Controller to project the tool on</param>
        public bool SelectTool(ulong toolId, bool releasable, AbstractController controller, ulong hoveredObjectId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(toolId);
            if (controller.IsCompatibleWith(tool))
            {
                if (toolIdToController.ContainsKey(tool.id))
                {
                    ReleaseTool(tool.id, new SwitchController());
                }

                toolIdToController.Add(tool.id, controller);
                projectedTools.Add(tool.id, reason);
                tool.OnUpdated.AddListener(() => UpdateTools(toolId, releasable, reason));
                controller.Project(tool, releasable, reason, hoveredObjectId);

                return true;
            }
            else
            {
                throw new Exception("This controller is not compatible with this tool");
            }
        }

        /// <inheritdoc/>
        public override bool UpdateTools(ulong toolId, bool releasable, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey(toolId))
            {
                AbstractController controller = toolIdToController[toolId];
                AbstractTool tool = GetTool(toolId);
                if (tool.interactions.Count <= 0)
                    ReleaseTool(tool.id, new ToolNeedToBeUpdated());
                else
                    controller.Update(tool, releasable, reason);
                return true;
            }
            throw new Exception("no controller have this tool projected");
        }

        /// <inheritdoc/>
        public override bool SwitchTools(ulong select, ulong release, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey(release))
            {
                AbstractController controller = toolIdToController[release];
                ReleaseTool(release);
                if (!SelectTool(select, releasable, controller, hoveredObjectId, reason))
                {
                    if (SelectTool(release, releasable, controller, hoveredObjectId))
                        return false;
                    else
                        throw new Exception("Internal error");
                }
            }
            else
            {
                if (!SelectTool(select, releasable, hoveredObjectId, reason))
                {
                    throw new Exception("Internal error");
                }
            }
            return true;
        }

        //this function will change/move in the future.
        protected bool ShouldForceProjection(AbstractController controller, AbstractTool tool, InteractionMappingReason reason)
        {
            if (controller.IsAvailableFor(tool))
                return true;

            if (controller.tool == null)
                return true; //check here

            if (projectedTools.TryGetValue(controller.tool.id, out InteractionMappingReason lastProjectionReason))
            {
                //todo : add some intelligence here.
                return !(reason is AutoProjectOnHover);
            }
            else
            {
                throw new Exception("Internal error");
            }
        }

        /// <inheritdoc/>
        public override bool IsToolSelected(ulong toolId)
        {
            return projectedTools.ContainsKey(toolId);
        }


        #region CRUD

        /// <inheritdoc/>
        public override void CreateToolbox(Toolbox toolbox)
        {
            toolboxMenu.Add(toolbox.sub);
        }

        /// <inheritdoc/>
        public override void CreateTool(Tool tool)
        {
            foreach (AbstractInteractionDto interaction in tool.dto.interactions)
            {
                interactionsIdToDto[interaction.id] = interaction;
            }
            tool.Menu.Subscribe(() =>
            {
                if (tool.Menu.toolSelected)
                {
                    ReleaseTool(tool.id, new RequestedFromMenu());
                }
                else
                {
                    SelectTool(tool.id, true, 0, new RequestedFromMenu());
                }
            });
        }

        /// <inheritdoc/>
        public override Toolbox GetToolbox(ulong id)
        {
            if (!ToolboxExists(id))
                throw new KeyNotFoundException();
            return UMI3DEnvironmentLoader.GetEntity(id)?.Object as Toolbox;
        }

        /// <inheritdoc/>
        public override IEnumerable<Toolbox> GetToolboxes(Predicate<Toolbox> condition)
        {
            return Toolbox.Toolboxes().FindAll(condition);
        }

        /// <inheritdoc/>
        public override AbstractTool GetTool(ulong id)
        {
            if (!ToolExists(id))
                throw new KeyNotFoundException();
            return UMI3DEnvironmentLoader.GetEntity(id)?.Object as AbstractTool;
        }

        /// <inheritdoc/>
        public override IEnumerable<AbstractTool> GetTools(Predicate<AbstractTool> condition)
        {
            return UMI3DEnvironmentLoader.Entities().Where(e => e?.Object is AbstractTool).Select(e => e?.Object as AbstractTool).ToList().FindAll(condition);
        }

        /// <inheritdoc/>
        public override AbstractInteractionDto GetInteraction(ulong id)
        {
            if (!InteractionExists(id))
                throw new KeyNotFoundException();
            interactionsIdToDto.TryGetValue(id, out AbstractInteractionDto inter);
            return inter;
        }

        /// <inheritdoc/>
        public override IEnumerable<AbstractInteractionDto> GetInteractions(Predicate<AbstractInteractionDto> condition)
        {
            return interactionsIdToDto.Values.ToList().FindAll(condition);
        }

        /// <inheritdoc/>
        public override bool ToolboxExists(ulong id)
        {
            return UMI3DEnvironmentLoader.GetEntity(id)?.Object as Toolbox != null;
        }

        /// <inheritdoc/>
        public override bool ToolExists(ulong id)
        {
            return UMI3DEnvironmentLoader.GetEntity(id)?.Object as AbstractTool != null;
        }

        /// <inheritdoc/>
        public override bool InteractionExists(ulong id)
        {
            return interactionsIdToDto.ContainsKey(id);
        }

        /// <inheritdoc/>
        public override AbstractController GetController(ulong projectedToolId)
        {
            if (!IsToolSelected(projectedToolId))
                return null;

            toolIdToController.TryGetValue(projectedToolId, out AbstractController controller);
            return controller;
        }

        #endregion
    }
}