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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    public abstract class AbstractTool : MonoBehaviour, UMI3DMediaEntity
    {
        #region properties

        public InteractionDisplay Display = new InteractionDisplay()
        {
            name = "new tool"
        };

        [SerializeField, EditorReadOnly]
        public List<AbstractInteraction> Interactions = new List<AbstractInteraction>();
        public UMI3DAsyncListProperty<AbstractInteraction> objectInteractions { get { Register(); return _objectInteractions; } protected set => _objectInteractions = value; }
        UMI3DAsyncListProperty<AbstractInteraction> _objectInteractions;

        [SerializeField, EditorReadOnly]
        public bool Active = true;

        public UMI3DAsyncProperty<bool> objectActive { get { Register(); return _objectActive; } protected set => _objectActive = value; }
        UMI3DAsyncProperty<bool> _objectActive;

        /// <summary>
        /// The tool's unique id. 
        /// </summary>
        protected ulong toolId;

        /// <summary>
        /// The public Getter for interactionId.
        /// </summary>
        public ulong Id()
        {
            if (toolId == 0 && UMI3DEnvironment.Exists)
                Register();
            return toolId;
        }

        /// <summary>
        /// Check if the AbstractTool has been registered to to the UMI3DScene and do it if not
        /// </summary>
        public virtual void Register()
        {
            if (toolId == 0 && UMI3DEnvironment.Exists)
            {
                toolId = UMI3DEnvironment.Register(this);
                InitDefinition(toolId);
            }
        }



        /// <summary>
        /// Indicates if InitDefinition has been called.
        /// </summary>
        protected bool inited = false;

        #endregion

        /// <summary>
        /// Indicates the availability state of a user for the last frame check of visibility.
        /// </summary>
        protected Dictionary<UMI3DUser, bool> availableLastFrame = new Dictionary<UMI3DUser, bool>();

        #region initialization

        /// <summary>
        /// Initialize interaction's properties.
        /// </summary>
        protected virtual void InitDefinition(ulong id)
        {
            BeardedManStudios.Forge.Networking.Unity.MainThreadManager.Run(() =>
            {
                if (this != null)
                    foreach (var f in GetComponents<UMI3DUserFilter>())
                        AddConnectionFilter(f);
            });

            toolId = id;
            objectInteractions = new UMI3DAsyncListProperty<AbstractInteraction>(toolId, UMI3DPropertyKeys.AbstractToolInteractions, Interactions, (i, u) => UMI3DEnvironment.Instance.useDto ? i.ToDto(u) : (object)i);
            objectActive = new UMI3DAsyncProperty<bool>(toolId, UMI3DPropertyKeys.ToolActive,Active);
            inited = true;
        }

        #endregion


        /// <summary>
        /// Unity MonoBehaviour OnDestroy method.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UMI3DEnvironment.Remove(toolId);
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected abstract AbstractToolDto CreateDto();

        /// <summary>
        /// Return Project Tool
        /// </summary>        
        /// <param name="releasable">Can the client choose to release the tool.
        /// if false, the only way of releasing it is through a ReleaseToolDto.</param>
        /// <param name="users">List of users to which this operation should be send.</param>
        /// <returns></returns>
        public ProjectTool GetProjectTool(bool releasable = true, HashSet<UMI3DUser> users = null)
        {
            return new ProjectTool() { tool = this, releasable = releasable, users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>()) };
        }

        /// <summary>
        /// Return Release Tool
        /// </summary>
        /// <param name="users">List of users to which this operation should be send.</param>
        /// <returns></returns>
        public ReleaseTool GetReleaseTool(HashSet<UMI3DUser> users = null)
        {
            return new ReleaseTool() { tool = this, users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>()) };
        }

        /// <summary>
        /// Return Switch Tool
        /// </summary>
        /// <param name="toolToReplace">Tool that should be replaced.</param>
        /// <param name="releasable">Can the client choose to release the tool.
        /// if false, the only way of releasing it is through a ReleaseToolDto.</param>
        /// <param name="users">List of users to which this operation should be send.</param>
        /// <returns></returns>
        public SwitchTool GetSwitchTool(AbstractTool toolToReplace, bool releasable = true, HashSet<UMI3DUser> users = null)
        {
            return new SwitchTool() { tool = this, toolToReplace = toolToReplace, releasable = releasable, users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>()) };
        }


        /// <summary>
        /// Called by a user on Tool projected.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="request">Interaction request</param>
        public void OnToolProjected(UMI3DUser user, ToolProjectedDto request) { onProjection.Invoke(new ProjectionContent(user, request.boneType, this)); }
        public void OnToolProjected(UMI3DUser user, uint boneType, ByteContainer container) { onProjection.Invoke(new ProjectionContent(user, boneType, this)); }
        /// <summary>
        /// Called by a user on Tool release.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="request">Interaction request</param>
        public void OnToolReleased(UMI3DUser user, ToolReleasedDto request) { onRelease.Invoke(new ProjectionContent(user, request.boneType, this)); }
        public void OnToolReleased(UMI3DUser user, uint boneType, ByteContainer container) { onRelease.Invoke(new ProjectionContent(user, boneType, this)); }

        #region event
        /// <summary>
        /// Called when this tool is projected.
        /// </summary>
        [SerializeField]
        public ProjectionEvent onProjection = new ProjectionEvent();

        /// <summary>
        /// Called when this tool is released.
        /// </summary>
        [SerializeField]
        public ProjectionEvent onRelease = new ProjectionEvent();

        /// <summary>
        /// Class for event rising on Projection. 
        /// </summary>
        [Serializable]
        public class ProjectionEvent : UnityEvent<ProjectionContent> { }

        /// <summary>
        /// Class for event rising on Release. 
        /// </summary>
        [Serializable]
        public class ReleaseEvent : UnityEvent<ProjectionContent> { }

        public class ProjectionContent
        {
            public UMI3DUser user;
            public uint boneType;
            public AbstractTool tool;

            public ProjectionContent(UMI3DUser user, uint boneType, AbstractTool tool)
            {
                this.user = user;
                this.boneType = boneType;
                this.tool = tool;
            }
        }
        #endregion



        /// <summary>
        /// Write the AbstractTool properties in an object AbstractToolDto is assignable from.
        /// </summary>
        /// <param name="scene">The AbstractToolDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected virtual void WriteProperties(AbstractToolDto dto, UMI3DUser user)
        {
            dto.id = Id();
            dto.name = Display.name;
            dto.description = Display.description;
            dto.icon2D = Display.icon2D?.ToDto();
            dto.icon3D = Display.icon3D?.ToDto();
            dto.interactions = objectInteractions.GetValue(user).Where(i => i != null).Select(i => i.ToDto(user)).ToList();
            dto.active = objectActive.GetValue(user);
        }


        public virtual Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(Id())
                + UMI3DNetworkingHelper.Write(Display.name)
                + UMI3DNetworkingHelper.Write(Display.description)
                + Display.icon2D?.ToByte()
                + Display.icon3D?.ToByte()
                + UMI3DNetworkingHelper.WriteIBytableCollection(objectInteractions.GetValue(user).Where(i => i != null), user)
                + UMI3DNetworkingHelper.Write(objectActive.GetValue(user));
        }

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public virtual AbstractToolDto ToDto(UMI3DUser user)
        {
            var dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
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