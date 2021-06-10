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
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Abstract class for UMI3D Player.
    /// </summary>
    public abstract class AbstractInteractionMapper : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static AbstractInteractionMapper Instance;

        /// <summary>
        /// The Interaction Controllers.
        /// Should be input devices (or groups of input devices) connectors.
        /// </summary>
        [SerializeField]
        protected List<AbstractController> Controllers = new List<AbstractController>();

        /// <summary>
        /// If true, when a tool with holdable events is projected, InteractionMapper will
        /// ask to selected AbstractController to project this event on a specific input if
        /// it can.
        /// </summary>
        public bool shouldProjectHoldableEventOnSpecificInput = false;


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


        /// <summary>
        /// Reset the InteractionMapper module.
        /// </summary>
        public abstract void ResetModule();


        /// <summary>
        /// Check if a toolbox with the given id exists.
        /// </summary>
        public abstract bool ToolboxExists(ulong id);

        /// <summary>
        /// Get the toolbox with the given id (if any).
        /// </summary>
        public abstract Toolbox GetToolbox(ulong id);

        /// <summary>
        /// Return the toolboxes matching a given condition.
        /// </summary>
        public abstract IEnumerable<Toolbox> GetToolboxes(Predicate<Toolbox> condition);

        /// <summary>
        /// Return all known toolboxes.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Toolbox> GetToolboxes() { return GetToolboxes(t => true); }


        /// <summary>
        /// Check if a tool with the given id exists.
        /// </summary>
        public abstract bool ToolExists(ulong id);

        /// <summary>
        /// Return true if the tool is currently projected on a controller.
        /// </summary>
        /// <param name="id">Id of the tool.</param>
        /// <returns></returns>
        public abstract bool IsToolSelected(ulong id);

        /// <summary>
        /// Get the tool with the given id (if any).
        /// </summary>
        public abstract AbstractTool GetTool(ulong id);

        /// <summary>
        /// Return the tools matching a given condition.
        /// </summary>
        public abstract IEnumerable<AbstractTool> GetTools(Predicate<AbstractTool> condition);

        /// <summary>
        /// Return all known tools.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AbstractTool> GetTools() { return GetTools(t => true); }


        /// <summary>
        /// Check if an interaction with the given id exists.
        /// </summary>
        public abstract bool InteractionExists(ulong id);

        /// <summary>
        /// Get the interaction with the given id (if any).
        /// </summary>
        public abstract AbstractInteractionDto GetInteraction(ulong id);

        /// <summary>
        /// Return the interactions matching a given condition.
        /// </summary>
        public abstract IEnumerable<AbstractInteractionDto> GetInteractions(Predicate<AbstractInteractionDto> condition);

        /// <summary>
        /// Return all known interactions.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AbstractInteractionDto> GetInteractions() { return GetInteractions(t => true); }

        /// <summary>
        /// Get the controller onto a given tool has been projected.
        /// </summary>
        /// <param name="projectedToolId">Tool's id</param>
        /// <returns></returns>
        public abstract AbstractController GetController(ulong projectedToolId);


        public abstract void CreateToolbox(Toolbox toolbox);

        public abstract void CreateTool(Tool tool);

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
        public abstract bool SelectTool(ulong toolId, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null);

        /// <summary>
        /// Request a Tool to be released.
        /// </summary>
        /// <param name="toolId">Id of the tool to release.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        public abstract void ReleaseTool(ulong toolId, InteractionMappingReason reason = null);

        /// <summary>
        /// Request a Tool to be replaced by another one.
        /// </summary>
        /// <param name="selected">Id of the tool to select</param>
        /// <param name="released">Id of the tool to release</param>
        /// <param name="releasable">The selected tool releasable.</param>
        /// <param name="hoveredObjectId">The id of the hovered object.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public abstract bool SwitchTools(ulong selected, ulong released, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null);

        /// <summary>
        /// Request a Tool to be updated.
        /// </summary>
        /// <param name="toolId">Id of the Tool.</param>
        /// <param name="releasable">Is the tool releasable.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public abstract bool UpdateTools(ulong toolId, bool releasable, InteractionMappingReason reason = null);

    }
}