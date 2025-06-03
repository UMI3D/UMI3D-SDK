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
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Default implementation of <see cref="AbstractInteractionMapper"/>.
    /// </summary>
    public class InteractionMapper : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static InteractionMapper Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

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

        /// <summary>
        /// The Interaction Controllers.
        /// Should be input devices (or groups of input devices) connectors.
        /// </summary>
        [SerializeField, Tooltip("The Interaction Controllers.\nShould be input devices (or groups of input devices) connectors")]
        protected List<AbstractController> Controllers = new List<AbstractController>();

        /// <summary>
        /// If true, when a tool with holdable events is projected, InteractionMapper will
        /// ask to selected AbstractController to project this event on a specific input if
        /// it can.
        /// </summary>
        [Tooltip("If true, when a tool with holdable events is projected, " +
            "InteractionMapper will ask to selected AbstractController " +
            "to project this event on a specific input if it can")]
        public bool shouldProjectHoldableEventOnSpecificInput = false;

        #endregion


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

        /// <summary>
        /// Reset the InteractionMapper module.
        /// </summary>
        public virtual void ResetModule()
        {
            foreach (AbstractController c in Controllers)
                c.Clear();

            toolIdToController = new Dictionary<(ulong, ulong), AbstractController>();
        }

        /// <summary>
        /// Request a Tool to be released.
        /// </summary>
        /// <param name="toolId">Id of the tool to release.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        public virtual void ReleaseTool(ulong environmentId, ulong toolId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(environmentId, toolId);

            if (toolIdToController.TryGetValue((tool.environmentId,tool.id), out AbstractController controller))
            {
                controller.Release(tool, reason);
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

        /// <summary>
        /// Request the selection of a Tool.
        /// Be careful,this method could be called before the tool is added for async loading reasons.
        /// Returns true if the tool has been successfuly selected, false otherwise.
        /// </summary>
        /// <param name="toolId">Id of the tool to release.</param>
        /// <param name="releasable">The selected tool releasable.</param>
        /// <param name="hoveredObjectId">The id of the hovered object.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public virtual bool SelectTool(ulong environmentId, ulong toolId, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
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
                if (!controller.IsAvailableFor(tool))
                {
                    if (ShouldForceProjection(controller, tool, reason))
                    {
                        ReleaseTool(environmentId, controller.tool.id);
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
            if (controller.IsCompatibleWith(tool))
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

                controller.Project(tool, releasable, reason, hoveredObjectId);

                return true;
            }
            else
            {
                throw new Exception("This controller is not compatible with this tool");
            }
        }

        /// <summary>
        /// Request a Tool to be updated.
        /// </summary>
        /// <param name="toolId">Id of the Tool.</param>
        /// <param name="releasable">Is the tool releasable.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public bool UpdateTools(ulong environmentId, ulong toolId, bool releasable, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey((environmentId,toolId)))
            {
                AbstractController controller = toolIdToController[(environmentId, toolId)];
                AbstractTool tool = GetTool(environmentId, toolId);
                if (tool.interactionsId.Count <= 0)
                    ReleaseTool(environmentId, tool.id, new ToolNeedToBeUpdated());
                else
                    controller.Update(tool, releasable, reason);
                return true;
            }
            throw new Exception("no controller have this tool projected");
        }

        /// <summary>
        /// Request a Tool to be updated when one element was added on the tool.
        /// </summary>
        /// <param name="toolId">Id of the Tool.</param>
        /// <param name="releasable">Is the tool releasable.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public bool UpdateAddOnTools(ulong environmentId, ulong toolId, bool releasable, AbstractInteractionDto abstractInteractionDto, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey((environmentId, toolId)))
            {
                AbstractController controller = toolIdToController[(environmentId, toolId)];
                AbstractTool tool = GetTool(environmentId, toolId);
                controller.AddUpdate(tool, releasable, abstractInteractionDto, reason);
                return true;
            }
            throw new Exception("no controller have this tool projected");
        }

        /// <summary>
        /// Request a Tool to be updated when one element was removed on the tool.
        /// </summary>
        /// <param name="toolId">Id of the Tool.</param>
        /// <param name="releasable">Is the tool releasable.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public bool UpdateRemoveOnTools(ulong environmentId, ulong toolId, bool releasable, AbstractInteractionDto abstractInteractionDto, InteractionMappingReason reason = null)
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

        /// <summary>
        /// Request a Tool to be replaced by another one.
        /// </summary>
        /// <param name="selected">Id of the tool to select</param>
        /// <param name="released">Id of the tool to release</param>
        /// <param name="releasable">The selected tool releasable.</param>
        /// <param name="hoveredObjectId">The id of the hovered object.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public virtual bool SwitchTools(ulong environmentId, ulong select, ulong release, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
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
            if (controller.IsAvailableFor(tool))
                return true;

            if (controller.tool == null)
                return true; //check here

            if (projectedTools.TryGetValue((controller.tool.environmentId,controller.tool.id), out InteractionMappingReason lastProjectionReason))
            {
                //todo : add some intelligence here.
                return !(reason is AutoProjectOnHover);
            }
            else
            {
                throw new Exception("Internal error");
            }
        }

        /// <summary>
        /// Return true if the tool is currently projected on a controller.
        /// </summary>
        /// <param name="id">Id of the tool.</param>
        /// <returns></returns>
        public bool IsToolSelected(ulong environmentId, ulong toolId)
        {
            return projectedTools.ContainsKey((environmentId, toolId));
        }


        #region CRUD

        /// <summary>
        /// Get the tool with the given id (if any).
        /// </summary>
        public AbstractTool GetTool(ulong environmentId, ulong id)
        {
            if (!ToolExists(environmentId, id))
                throw new KeyNotFoundException();
            return UMI3DEnvironmentLoader.GetEntity(environmentId, id)?.Object as AbstractTool;
        }

        /// <summary>
        /// Return the tools matching a given condition.
        /// </summary>
        public IEnumerable<AbstractTool> GetTools(Predicate<AbstractTool> condition)
        {
            return UMI3DEnvironmentLoader.AllEntities().Where(e => e?.Object is AbstractTool).Select(e => e?.Object as AbstractTool).ToList().FindAll(condition);
        }

        /// <summary>
        /// Return all known tools.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AbstractTool> GetTools() { return GetTools(t => true); }

        /// <summary>
        /// Get the interaction with the given id (if any).
        /// </summary>
        public AbstractInteractionDto GetInteraction(ulong environmentId,ulong id)
        {
            if (!InteractionExists(environmentId, id))
                throw new KeyNotFoundException();
            interactionsIdToDto.TryGetValue((environmentId, id), out AbstractInteractionDto inter);
            return inter;
        }

        /// <summary>
        /// Return the interactions matching a given condition.
        /// </summary>
        public IEnumerable<AbstractInteractionDto> GetInteractions(Predicate<AbstractInteractionDto> condition)
        {
            return interactionsIdToDto.Values.ToList().FindAll(condition);
        }

        /// <summary>
        /// Return all known interactions.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AbstractInteractionDto> GetInteractions() { return GetInteractions(t => true); }

        /// <summary>
        /// Check if a tool with the given id exists.
        /// </summary>
        public bool ToolExists(ulong environmentId, ulong id)
        {
            return (UMI3DEnvironmentLoader.GetEntity(environmentId, id)?.Object as AbstractTool) != null;
        }

        /// <summary>
        /// Check if an interaction with the given id exists.
        /// </summary>
        public bool InteractionExists(ulong environmentId, ulong id)
        {
            return interactionsIdToDto.ContainsKey((environmentId, id));
        }

        /// <summary>
        /// Get the controller onto a given tool has been projected.
        /// </summary>
        /// <param name="projectedToolId">Tool's id</param>
        /// <returns></returns>
        public AbstractController GetController(ulong environmentId, ulong projectedToolId)
        {
            if (!IsToolSelected(environmentId, projectedToolId))
                return null;

            toolIdToController.TryGetValue((environmentId, projectedToolId), out AbstractController controller);
            return controller;
        }

        #endregion
    }
}
