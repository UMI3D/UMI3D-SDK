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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// Abstract UMI3D interaction collection.
    /// </summary>
    public class UMI3DToolbox : MonoBehaviour, UMI3DLoadableEntity
    {
        public InteractionDisplay display = new InteractionDisplay()
        {
            name = "new toolbox"
        };

        [SerializeField]
        protected UMI3DScene Scene;
        public UMI3DAsyncProperty<UMI3DScene> objectScene;

        public List<UMI3DTool> tools = new List<UMI3DTool>();
        public UMI3DAsyncListProperty<UMI3DTool> objectTools;


        #region properties

        /// <summary>
        /// The interaction's unique id. 
        /// </summary>
        private string toolboxId;

        /// <summary>
        /// The public Getter for interactionId.
        /// </summary>
        public string Id()
        {
            if (toolboxId == null && UMI3DEnvironment.Exists)
                Register();
            return toolboxId;
        }

        void Register()
        {
            if (toolboxId == null && UMI3DEnvironment.Exists)
            {
                toolboxId = UMI3DEnvironment.Register(this);
                InitDefinition(toolboxId);
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
        /// Initialize interaction's properties.
        /// </summary>
        protected virtual void InitDefinition(string id)
        {
            toolboxId = id;
            if (Scene == null) Scene = GetComponent<UMI3DScene>();
            objectTools = new UMI3DAsyncListProperty<UMI3DTool>(toolboxId, UMI3DPropertyKeys.ToolboxTools, tools);
            objectScene = new UMI3DAsyncProperty<UMI3DScene>(toolboxId, UMI3DPropertyKeys.ToolboxSceneId, Scene, (s, u) => s.Id());
            inited = true;
        }

        #endregion


        /// <summary>
        /// Unity MonoBehaviour OnDestroy method.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UMI3DEnvironment.Remove(this);
        }

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public ToolboxDto ToDto(UMI3DUser user)
        {
            ToolboxDto dto = new ToolboxDto();
            dto.id = Id();
            dto.name = display.name;
            dto.description = display.description;
            dto.icon2D = display.icon2D.ToDto();
            dto.icon3D = display.icon3D.ToDto();
            dto.tools = objectTools?.GetValue(user).Where(t => t != null).Select(t => t.ToDto(user) as ToolDto).ToList();
            return dto;
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user);
        }
    }
}