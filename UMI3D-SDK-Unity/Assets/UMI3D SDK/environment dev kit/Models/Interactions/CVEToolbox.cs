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
using umi3d.common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Abstract UMI3D interaction.
    /// </summary>
    public class CVEToolbox : MonoBehaviour
    {

        public InteractionDisplay display = new InteractionDisplay()
        {
            name = "new toolbox"
        };

        public readonly List<CVETool> tools = new List<CVETool>();
        
        public void RemoveTool(CVETool tool)
        {
            tool.toolbox = null;
            tool.currentToolbox = null;
            tools.Remove(tool);
        }

        public void AddTool(CVETool tool)
        {
            tool.toolbox = this;
            tool.currentToolbox = this;
            if (!tools.Contains(tool))
                tools.Add(tool);
        }

        #region properties

        /// <summary>
        /// The interaction's unique id. 
        /// </summary>
        private string toolboxId;

        /// <summary>
        /// The public Getter for interactionId.
        /// </summary>
        public string Id
        {
            get
            {
                if (toolboxId == null)
                    toolboxId = UMI3D.Scene.Register(this);
                return toolboxId;
            }
        }

        /// <summary>
        /// Indicates if InitDefinition has been called.
        /// </summary>
        protected bool inited = false;

        /// <summary>
        /// Indicates the availability state of a user for the last frame check of visibility.
        /// </summary>
        protected Dictionary<UMI3DUser, bool> availableLastFrame = new Dictionary<UMI3DUser, bool>();

        #endregion


        #region initialization

        /// <summary>
        /// Unity MonoBehaviour Start method.
        /// </summary>
        protected virtual void Start()
        {
            initDefinition();
        }

        /// <summary>
        /// Initialize interaction's properties.
        /// </summary>
        protected virtual void initDefinition()
        {
            toolboxId = Id;
            inited = true;
        }

        #endregion


        /// <summary>
        /// Unity MonoBehaviour OnDestroy method.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (UMI3D.Scene)
                UMI3D.Scene.Remove(this);

            if (UMI3D.Exist)
                foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
                    if (availableLastFrame.ContainsKey(user) && availableLastFrame[user])
                        user.InteractionsIdsToRemove.Add(Id);
        }


        #region availability

        /// <summary>
        /// Return true if this interaction is available for the given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check availability for</param>
        /// <returns></returns>
        public virtual bool AvailableAsChildFor(UMI3DUser user)
        {
            if (toolboxId == null)
                return false;
            foreach (var filter in GetComponents<AvailabilityFilter>().Where(f => f.enabled))
                if (!filter.Accept(user))
                    return false;
            return true;
        }

        /// <summary>
        /// Return true if this interaction is available for the given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check availability for</param>
        /// <returns></returns>
        public virtual bool AvailableFor(UMI3DUser user)
        {
            if (toolboxId == null || !enabled || !gameObject.activeInHierarchy)
                return false;
            foreach (var filter in GetComponents<AvailabilityFilter>().Where(f => f.enabled))
                if (!filter.Accept(user))
                    return false;
            return true;
        }

        public void UpdateAvailabilityLastFrame(UMI3DUser user)
        {
            if (availableLastFrame.ContainsKey(user))
                availableLastFrame[user] = AvailableFor(user);
            else
                availableLastFrame.Add(user, AvailableFor(user));
        }

        public bool UpdateAvailabilityForUser(UMI3DUser user)
        {
            bool wasAvailable = (availableLastFrame.ContainsKey(user)) ? availableLastFrame[user] : false;
            bool available = AvailableFor(user);

            if (wasAvailable && !available)
                user.ToolboxesIdsToRemove.Add(Id);

            if (!wasAvailable && available)
                user.ToolboxesToLoad.Add(ConvertToDto(user));

            return wasAvailable && available;
        }

        #endregion
        
        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public ToolboxDto ConvertToDto(UMI3DUser user)
        {
            ToolboxDto dto = new ToolboxDto();
            dto.id = toolboxId;
            dto.name = display.name;
            dto.icon2D = display.icon2D.ToDto();
            dto.icon3D = display.icon3D.ToDto();
            IEnumerable<ToolDto> tools = this.tools.Where(t => (t != null) && t.AvailableFor(user)).Select(t=>t.ConvertToDto(user) as ToolDto);
            dto.tools.AddRange(tools);
            return dto;
        }

    }

}
