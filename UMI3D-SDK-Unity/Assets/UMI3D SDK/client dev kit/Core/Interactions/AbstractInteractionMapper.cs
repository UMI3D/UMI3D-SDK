/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
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

        #region Create

        /// <summary>
        /// Create new interaction toolboxes.
        /// </summary>
        /// <param name="dtos">A list of ToolboxDto representing the toolboxes to be created</param>
        public abstract IEnumerable<Toolbox> CreateToolboxes(IEnumerable<ToolboxDto> dtos);

        /// <summary>
        /// Create new interaction toolbox.
        /// </summary>
        public abstract Toolbox CreateToolbox(ToolboxDto dto);

        /// <summary>
        /// Create new tools.
        /// </summary>
        /// <param name="dtos">A list of ToolDto representing the tools to be inserted in existing toolboxes</param>
        public abstract IEnumerable<AbstractTool> CreateTools(IEnumerable<AbstractToolDto> dtos);

        /// <summary>
        /// Create new interactions.
        /// </summary>
        /// <param name="dtos">A list of InteractionDto representing the interactables to be created</param>
        public abstract IEnumerable<Interactable> CreateInteractables(IEnumerable<InteractableDto> dtos, IEnumerable<GameObject> interactableObjects);

        /// <summary>
        /// Create new tool.
        /// </summary>
        public abstract AbstractTool CreateTool(AbstractToolDto dto);

        /// <summary>
        /// Create new interactable.
        /// </summary>
        public abstract Interactable CreateInteractable(InteractableDto dto, GameObject interactableObject);

        /// <summary>
        /// Create new interaction.
        /// </summary>
        /// <param name="dtos">A list of ToolDto representing the tools to be inserted in existing toolboxes</param>
        public abstract int CreateInteractions(IEnumerable<AbstractInteractionDto> dtos);

        /// <summary>
        /// Create new interaction.
        /// </summary>
        public abstract bool CreateInteraction(AbstractInteractionDto dto);

        #endregion

        #region Read

        /// <summary>
        /// Check if a toolbox with the given id exists.
        /// </summary>
        public abstract bool ToolboxExists(string id);

        /// <summary>
        /// Get the toolbox with the given id (if any).
        /// </summary>
        public abstract Toolbox GetToolbox(string id);

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
        public abstract bool ToolExists(string id);

        /// <summary>
        /// Return true if the tool is currently projected on a controller.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract bool IsToolSelected(string id);

        /// <summary>
        /// Get the tool with the given id (if any).
        /// </summary>
        public abstract AbstractTool GetTool(string id);

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
        public abstract bool InteractionExists(string id);

        /// <summary>
        /// Get the interaction with the given id (if any).
        /// </summary>
        public abstract AbstractInteractionDto GetInteraction(string id);

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
        public abstract AbstractController GetController(string projectedToolId);

        #endregion

        #region Update

        public abstract void UpdateToolboxes(IEnumerable<ToolboxDto> dtos);
        public abstract void UpdateToolbox(ToolboxDto dto);
        public abstract void UpdateTools(IEnumerable<AbstractToolDto> dtos);
        public abstract void UpdateTool(AbstractToolDto dto);
        public abstract void UpdateInteractions(IEnumerable<AbstractInteractionDto> dtos);
        public abstract void UpdateInteraction(AbstractInteractionDto dto);

        #endregion

        #region Delete

        /// <summary>
        /// Remove toolboxes.
        /// </summary>
        /// <param name="dtos">The ids of the toolboxes to be deleted</param>
        public abstract int DeleteToolboxes(IEnumerable<string> ids);

        /// <summary>
        /// Remove a toolbox.
        /// </summary>
        /// <param name="dtos">The id of the toolbox to be deleted</param>
        public abstract bool DeleteToolbox(string id);

        /// <summary>
        /// Remove tools.
        /// </summary>
        /// <param name="dtos">The ids of the tools to be deleted</param>
        public abstract int DeleteTools(IEnumerable<string> ids);

        /// <summary>
        /// Remove a tool.
        /// </summary>
        /// <param name="dtos">The id of the tool to be deleted</param>
        public abstract bool DeleteTool(string id);

        /// <summary>
        /// Remove interactions.
        /// </summary>
        /// <param name="dtos">The ids of the interactions to be deleted from existing tools</param>
        public abstract int DeleteInteractions(IEnumerable<string> ids);

        /// <summary>
        /// Remove an interaction.
        /// </summary>
        /// <param name="dtos">The id of the interaction to be deleted from an existing tool</param>
        public abstract bool DeleteInteraction(string id);

        #endregion


        /// <summary>
        /// Request the selection of a Tool.
        /// Be careful,this method could be called before the tool is added for async loading reasons.
        /// Returns true if the tool has been successfuly selected, false otherwise.
        /// </summary>
        /// <param name="dto">The tool to be selected</param>
        public abstract bool SelectTool(string toolId, InteractionMappingReason reason = null);

        /// <summary>
        /// Request a Tool to be released.
        /// </summary>
        /// <param name="dto">The tool to be released</param>
        public abstract void ReleaseTool(string toolId, InteractionMappingReason reason = null);

        /// <summary>
        /// Request a Tool to be replaced by another one.
        /// </summary>
        /// <param name="selected">The tool to be selected</param>
        /// <param name="released">The tool to be released</param>
        public abstract bool SwitchTools(string selected, string released, InteractionMappingReason reason = null);


    }
}