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
    /// <summary>
    /// Default implementation of <see cref="AbstractInteractionMapper"/>.
    /// </summary>
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
        public Dictionary<(ulong, ulong), AbstractController> toolIdToController { get; protected set; } = new Dictionary<(ulong, ulong), AbstractController>();

        /// <summary>
        /// Id to Dto interactions map.
        /// </summary>
        public Dictionary<(ulong, ulong), AbstractInteractionDto> interactionsIdToDto { get; protected set; } = new Dictionary<(ulong, ulong), AbstractInteractionDto>();

        /// <summary>
        /// Currently projected tools.
        /// </summary>
        private readonly Dictionary<(ulong, ulong), InteractionMappingReason> projectedTools = new Dictionary<(ulong, ulong), InteractionMappingReason>();

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

            toolIdToController = new Dictionary<(ulong, ulong), AbstractController>();
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
                if (controller.toolManager.toolDelegate.IsCompatibleWith(tool) && controller.toolManager.toolDelegate.IsAvailableFor(tool))
                {
                    return controller;
                }
            }

            foreach (AbstractController controller in Controllers)
            {
                if (controller.toolManager.toolDelegate.IsCompatibleWith(tool))
                {
                    return controller;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override void ReleaseTool(ulong environmentId, ulong toolId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(environmentId, toolId);

            if (toolIdToController.TryGetValue((tool.environmentId,tool.id), out AbstractController controller))
            {
                controller.projectionManager.Release(tool, reason);
                toolIdToController.Remove((tool.environmentId, tool.id));
                tool.OnUpdated.RemoveAllListeners();
                tool.OnAdded.RemoveAllListeners();
                tool.OnRemoved.RemoveAllListeners();
                projectedTools.Remove((tool.environmentId, tool.id));
            }
            else
            {
                throw new Exception("Tool not selected");
            }
        }

        /// <inheritdoc/>
        public override bool SelectTool(ulong environmentId, ulong toolId, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(environmentId, toolId);
            if (tool == null)
                throw new Exception("tool does not exist");

            if (toolIdToController.ContainsKey((tool.environmentId, tool.id)))
            {
                throw new Exception("Tool already projected");
            }

            AbstractController controller = GetController(tool);
            if (controller != null)
            {
                if (!controller.toolManager.toolDelegate.IsAvailableFor(tool))
                {
                    if (ShouldForceProjection(controller, tool, reason))
                    {
                        ReleaseTool(environmentId, controller.toolManager.toolDelegate.Tool.id);
                    }
                    else
                    {
                        return false;
                    }
                }

                return SelectTool(tool.environmentId,tool.id, releasable, controller, hoveredObjectId, reason);
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
        public bool SelectTool(ulong environmentId, ulong toolId, bool releasable, AbstractController controller, ulong hoveredObjectId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(environmentId, toolId);
            if (controller.toolManager.toolDelegate.IsCompatibleWith(tool))
            {
                if (toolIdToController.ContainsKey((tool.environmentId, tool.id)))
                {
                    ReleaseTool(environmentId, tool.id, new SwitchController());
                }

                toolIdToController.Add((tool.environmentId, tool.id), controller);
                projectedTools.Add((tool.environmentId, tool.id), reason);
                tool.OnUpdated.AddListener(() => UpdateTools(environmentId, toolId, releasable, reason));
                tool.OnAdded.AddListener(abstractInteractionDto => { UpdateAddOnTools(environmentId, toolId, releasable, abstractInteractionDto, reason); });
                tool.OnRemoved.AddListener(abstractInteractionDto => { UpdateRemoveOnTools(environmentId, toolId, releasable, abstractInteractionDto, reason); });

                controller.projectionManager.Project(
                    tool, 
                    releasable, 
                    reason, 
                    hoveredObjectId
                );

                return true;
            }
            else
            {
                throw new Exception("This controller is not compatible with this tool");
            }
        }

        /// <inheritdoc/>
        public override bool UpdateTools(ulong environmentId, ulong toolId, bool releasable, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey((environmentId,toolId)))
            {
                AbstractController controller = toolIdToController[(environmentId, toolId)];
                AbstractTool tool = GetTool(environmentId, toolId);
                if (tool.interactionsId.Count <= 0)
                    ReleaseTool(environmentId, tool.id, new ToolNeedToBeUpdated());
                else
                    controller.projectionManager.Project(
                        tool, 
                        releasable, 
                        reason,
                        controller.toolManager.toolDelegate.CurrentHoverTool.id
                    );
                return true;
            }
            throw new Exception("no controller have this tool projected");
        }

        /// <inheritdoc/>
        public override bool UpdateAddOnTools(ulong environmentId, ulong toolId, bool releasable, AbstractInteractionDto abstractInteractionDto, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey((environmentId, toolId)))
            {
                AbstractController controller = toolIdToController[(environmentId, toolId)];
                AbstractTool tool = GetTool(environmentId, toolId);
                controller.projectionManager.Project(tool, abstractInteractionDto, releasable, reason);
                return true;
            }
            throw new Exception("no controller have this tool projected");
        }

        /// <inheritdoc/>
        public override bool UpdateRemoveOnTools(ulong environmentId, ulong toolId, bool releasable, AbstractInteractionDto abstractInteractionDto, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(environmentId, toolId);
            tool.interactionsId.Remove(abstractInteractionDto.id);

            foreach (AbstractController item in Controllers)
            {
                foreach (AbstractUMI3DInput input in item.inputs)
                {
                    if (input != null && !input.IsAvailable() && input.CurrentInteraction().id == abstractInteractionDto.id)
                    {
                        input.Dissociate();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool SwitchTools(ulong environmentId, ulong select, ulong release, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey((environmentId,release)))
            {
                AbstractController controller = toolIdToController[(environmentId, release)];
                ReleaseTool(environmentId, release);
                if (!SelectTool(environmentId, select, releasable, controller, hoveredObjectId, reason))
                {
                    if (SelectTool(environmentId, release, releasable, controller, hoveredObjectId))
                        return false;
                    else
                        throw new Exception("Internal error");
                }
            }
            else
            {
                if (!SelectTool(environmentId, select, releasable, hoveredObjectId, reason))
                {
                    throw new Exception("Internal error");
                }
            }
            return true;
        }

        //this function will change/move in the future.
        protected bool ShouldForceProjection(AbstractController controller, AbstractTool tool, InteractionMappingReason reason)
        {
            if (controller.toolManager.toolDelegate.IsAvailableFor(tool))
                return true;

            if (controller.toolManager.toolDelegate.Tool == null)
                return true; //check here

            if (projectedTools.TryGetValue((controller.toolManager.toolDelegate.Tool.environmentId, controller.toolManager.toolDelegate.Tool.id), out InteractionMappingReason lastProjectionReason))
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
        public override bool IsToolSelected(ulong environmentId, ulong toolId)
        {
            return projectedTools.ContainsKey((environmentId, toolId));
        }


        #region CRUD

        /// <inheritdoc/>
        public override Toolbox GetToolbox(ulong environmentId, ulong id)
        {
            if (!ToolboxExists(environmentId, id))
                throw new KeyNotFoundException();
            return UMI3DEnvironmentLoader.GetEntity(environmentId, id)?.Object as Toolbox;
        }

        /// <inheritdoc/>
        public override IEnumerable<Toolbox> GetToolboxes(Predicate<Toolbox> condition)
        {
            return Toolbox.GetToolboxes().FindAll(condition);
        }

        /// <inheritdoc/>
        public override AbstractTool GetTool(ulong environmentId, ulong id)
        {
            if (!ToolExists(environmentId, id))
                throw new KeyNotFoundException();
            return UMI3DEnvironmentLoader.GetEntity(environmentId, id)?.Object as AbstractTool;
        }

        /// <inheritdoc/>
        public override IEnumerable<AbstractTool> GetTools(Predicate<AbstractTool> condition)
        {
            return UMI3DEnvironmentLoader.AllEntities().Where(e => e?.Object is AbstractTool).Select(e => e?.Object as AbstractTool).ToList().FindAll(condition);
        }

        /// <inheritdoc/>
        public override AbstractInteractionDto GetInteraction(ulong environmentId,ulong id)
        {
            if (!InteractionExists(environmentId, id))
                throw new KeyNotFoundException();
            interactionsIdToDto.TryGetValue((environmentId, id), out AbstractInteractionDto inter);
            return inter;
        }

        /// <inheritdoc/>
        public override IEnumerable<AbstractInteractionDto> GetInteractions(Predicate<AbstractInteractionDto> condition)
        {
            return interactionsIdToDto.Values.ToList().FindAll(condition);
        }

        /// <inheritdoc/>
        public override bool ToolboxExists(ulong environmentId, ulong id)
        {
            return (UMI3DEnvironmentLoader.GetEntity(environmentId,id)?.Object as Toolbox) != null;
        }

        /// <inheritdoc/>
        public override bool ToolExists(ulong environmentId, ulong id)
        {
            return (UMI3DEnvironmentLoader.GetEntity(environmentId, id)?.Object as AbstractTool) != null;
        }

        /// <inheritdoc/>
        public override bool InteractionExists(ulong environmentId, ulong id)
        {
            return interactionsIdToDto.ContainsKey((environmentId, id));
        }

        /// <inheritdoc/>
        public override AbstractController GetController(ulong environmentId, ulong projectedToolId)
        {
            if (!IsToolSelected(environmentId, projectedToolId))
                return null;

            toolIdToController.TryGetValue((environmentId, projectedToolId), out AbstractController controller);
            return controller;
        }

        #endregion
    }
}
