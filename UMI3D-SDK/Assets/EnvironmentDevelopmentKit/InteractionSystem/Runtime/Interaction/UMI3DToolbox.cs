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

        [SerializeField, EditorReadOnly]
        protected UMI3DScene Scene;
        public UMI3DAsyncProperty<UMI3DScene> objectScene { get { Register(); return _objectScene; } protected set => _objectScene = value; }

        [SerializeField, EditorReadOnly]
        protected List<UMI3DTool> tools = new List<UMI3DTool>();
        public UMI3DAsyncListProperty<UMI3DTool> objectTools { get { Register(); return _objectTools; } protected set => _objectTools = value; }


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

        /// <summary>
        /// Check if the Toolbox has been registered to the UMI3DScene and do it if not
        /// </summary>
        /// <returns>Return a LoadEntity</returns>
        public virtual LoadEntity Register()
        {
            if (toolboxId == null && UMI3DEnvironment.Exists)
            {
                toolboxId = UMI3DEnvironment.Register(this);
                InitDefinition(toolboxId);
            }
            return GetLoadEntity();
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entity = this,
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
            };
            return operation;
        }

        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        /// <summary>
        /// Indicates if InitDefinition has been called.
        /// </summary>
        protected bool inited = false;

        /// <summary>
        /// Indicates the availability state of a user for the last frame check of visibility.
        /// </summary>
        protected Dictionary<UMI3DUser, bool> availableLastFrame = new Dictionary<UMI3DUser, bool>();
        private UMI3DAsyncProperty<UMI3DScene> _objectScene;
        private UMI3DAsyncListProperty<UMI3DTool> _objectTools;

        #endregion


        #region initialization

        /// <summary>
        /// Initialize interaction's properties.
        /// </summary>
        protected virtual void InitDefinition(string id)
        {
            BeardedManStudios.Forge.Networking.Unity.MainThreadManager.Run(() =>
            {
                if (this != null)
                    foreach (var f in GetComponents<UMI3DUserFilter>())
                        AddConnectionFilter(f);
            });

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

        #region filter
        HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion
    }
}