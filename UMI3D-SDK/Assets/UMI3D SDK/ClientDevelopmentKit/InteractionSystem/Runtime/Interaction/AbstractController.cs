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
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Abstract class for UMI3D Controller.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractController : MonoBehaviour
    {
        #region properties
        /// <summary>
        /// Currently projected tool's id.
        /// </summary>
        protected AbstractTool currentTool = null;
        public AbstractTool tool => currentTool;

        /// <summary>
        /// Controller's inputs.
        /// </summary>
        public abstract List<AbstractUMI3DInput> inputs { get; }

        /// <summary>
        /// Inputs associated to a given tool (keys are tools' ids).
        /// </summary>
        protected Dictionary<ulong, AbstractUMI3DInput[]> associatedInputs = new Dictionary<ulong, AbstractUMI3DInput[]>();

        public ProjectionMemory projectionMemory;

        #endregion

        #region interface

        /// <summary>
        /// Check if the user is currently using the selected Tool
        /// </summary>
        /// <returns>returns true if the user is currently interacting with the tool.</returns>
        protected abstract bool isInteracting();
        /// <summary>
        /// Check if the user is currently using the selected Tool
        /// </summary>
        public bool Interacting => isInteracting();

        /// <summary>
        /// Check if the user is currently manipulating the tools menu
        /// </summary>
        /// <returns>returns true if the user is currently interacting with the tool.</returns>
        protected abstract bool isNavigating();
        /// <summary>
        /// Check if the user is currently manipulating the tools menu
        /// </summary>
        public bool Navigating => isNavigating();

        /// <summary>
        /// Clear all menus and the projected tools
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Find the best DofGroupOptionDto for this controller.
        /// </summary>
        /// <param name="options">Options to search in</param>
        /// <returns></returns>
        public abstract DofGroupOptionDto FindBest(DofGroupOptionDto[] options);

        /// <summary>
        /// Return an input for a given manipulation dof. Return null if no input is available.
        /// </summary>
        /// <param name="manip">Manipulation to find input for</param>
        /// <param name="dof">Manipulation dof</param>
        /// <param name="unused">Should the input be unused ?</param>
        public abstract AbstractUMI3DInput FindInput(ManipulationDto manip, DofGroupDto dof, bool unused = true);

        /// <summary>
        /// Return an input for a given event. Return null if no input is available.
        /// </summary>
        /// <param name="evt">EventDto to find input for</param>
        /// <param name="unused">Should the input be unused ?</param>
        /// <param name="tryToFindInputForHoldableEvent">Asks controller to find an input dedicated to holdable event if it can ?</param>
        public abstract AbstractUMI3DInput FindInput(EventDto evt, bool unused = true, bool tryToFindInputForHoldableEvent = false);

        /// <summary>
        /// Return an input for a given parameter. Return null if no input is available.
        /// </summary>
        /// <param name="param">Parameter to find an input for</param>
        /// <param name="unused">Should the input be unused ?</param>
        /// <returns></returns>
        public abstract AbstractUMI3DInput FindInput(AbstractParameterDto param, bool unused = true);

        /// <summary>
        /// Return an input for a given form. Return null if no input is available.
        /// </summary>
        /// <param name="form">Form to find an input for</param>
        /// <param name="unused">Should the input be unused ?</param>
        /// <returns></returns>
        public abstract AbstractUMI3DInput FindInput(FormDto form, bool unused = true);

        /// <summary>
        /// Return an input for a given form. Return null if no input is available.
        /// </summary>
        /// <param name="link">Form to find an input for</param>
        /// <param name="unused">Should the input be unused ?</param>
        /// <returns></returns>
        public abstract AbstractUMI3DInput FindInput(LinkDto link, bool unused = true);

        /// <summary>
        /// Check if a tool can be projected on this controller.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public abstract bool IsCompatibleWith(AbstractTool tool);

        /// <summary>
        /// Check if a compatible tool is currently projectable on this controller.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        /// <see cref="IsCompatibleWith(AbstractToolDto)"/>
        public virtual bool IsAvailableFor(AbstractTool tool)
        {
            if (!IsCompatibleWith(tool))
                return false;

            return currentTool == null;
        }

        /// <summary>
        /// Check if a tool requires the generation of a menu to be projected.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public abstract bool RequiresMenu(AbstractTool tool);

        /// <summary>
        /// Create a menu to access each interactions of a tool separately.
        /// </summary>
        /// <param name="interactions"></param>
        public abstract void CreateInteractionsMenuFor(AbstractTool tool);

        #endregion

        /// <summary>
        /// Project a tool on this controller.
        /// </summary>
        /// <param name="tool"> The ToolDto to be projected.</param>
        /// <see cref="Release(AbstractTool)"/>
        public virtual void Project(AbstractTool tool, bool releasable, InteractionMappingReason reason, ulong hoveredObjectId)
        {
            if (!IsCompatibleWith(tool))
                throw new System.Exception("Trying to project an uncompatible tool !");

            if (currentTool != null)
                throw new System.Exception("A tool is already projected !");

            if (RequiresMenu(tool))
            {
                CreateInteractionsMenuFor(tool);
            }
            else
            {
                AbstractInteractionDto[] interactions = tool.interactions.ToArray();
                AbstractUMI3DInput[] inputs = projectionMemory.Project(this, interactions, tool.id, hoveredObjectId);
                associatedInputs.Add(tool.id, inputs);
            }

            currentTool = tool;
        }


        /// <summary>
        /// Project a tool on this controller.
        /// </summary>
        /// <param name="tool"> The ToolDto to be projected.</param>
        /// <see cref="Release(AbstractTool)"/>
        public virtual void Update(AbstractTool tool, bool releasable, InteractionMappingReason reason)
        {
            if (currentTool != tool)
                throw new System.Exception("Try to update wrong tool");

            Release(tool, new ToolNeedToBeUpdated());
            Project(tool, releasable, reason, GetCurrentHoveredId());
        }

        protected abstract ulong GetCurrentHoveredId();


        /// <summary>
        /// Release a projected tool from this controller.
        /// </summary>
        /// <param name="tool">Tool to release</param>
        /// <see cref="Project(AbstractTool)"/>
        public virtual void Release(AbstractTool tool, InteractionMappingReason reason)
        {
            if (currentTool == null)
                throw new System.Exception("no tool is currently projected on this controller");
            if (currentTool.id != tool.id)
                throw new System.Exception("This tool is not currently projected on this controller");

            if (associatedInputs.TryGetValue(tool.id, out AbstractUMI3DInput[] inputs))
            {
                foreach (AbstractUMI3DInput input in inputs)
                {
                    if (input.CurrentInteraction() != null)
                        input.Dissociate();
                }
                associatedInputs.Remove(tool.id);
            }
            currentTool = null;
        }

        /// <summary>
        /// Change a tool on this controller to add a new interaction
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="releasable"></param>
        /// <param name="abstractInteractionDto"></param>
        /// <param name="reason"></param>
        public virtual void AddUpdate(AbstractTool tool, bool releasable, AbstractInteractionDto abstractInteractionDto, InteractionMappingReason reason)
        {
            if (currentTool != tool)
                throw new System.Exception("Try to update wrong tool");

            if (RequiresMenu(tool))
            {
                CreateInteractionsMenuFor(tool);
            }
            else
            {
                var interaction = new AbstractInteractionDto[] { abstractInteractionDto };
                AbstractUMI3DInput[] inputs = projectionMemory.Project(this, interaction, tool.id, GetCurrentHoveredId());
                if (associatedInputs.ContainsKey(tool.id))
                {
                    associatedInputs[tool.id] = associatedInputs[tool.id].Concat(inputs).ToArray();
                }
                else
                {
                    associatedInputs.Add(tool.id, inputs);
                }
            }
            currentTool = tool;
        }
    }
}