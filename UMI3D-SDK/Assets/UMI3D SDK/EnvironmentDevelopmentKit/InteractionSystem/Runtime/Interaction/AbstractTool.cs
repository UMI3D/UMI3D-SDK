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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// List of interactions that could be projected on a client controller.
    /// </summary>
    public abstract class AbstractTool : MonoBehaviour, UMI3DMediaEntity
    {
        #region properties

        /// <summary>
        /// Serializable object that displays the tool properties.
        /// </summary>
        [Tooltip("Displayed information related to the tool.")]
        public InteractionDisplay Display = new InteractionDisplay()
        {
            name = "new tool"
        };

        /// <summary>
        /// Interactions handled by the tool.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Interactions handled by the tool.")]
        public List<AbstractInteraction> Interactions = new List<AbstractInteraction>();
        public UMI3DAsyncListProperty<AbstractInteraction> objectInteractions { get { Register(); return _objectInteractions; } protected set => _objectInteractions = value; }

        private UMI3DAsyncListProperty<AbstractInteraction> _objectInteractions;

        /// <summary>
        /// True if the tool is active.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Set to true if the tool should be active.")]
        public bool Active = true;

        public UMI3DAsyncProperty<bool> objectActive { get { Register(); return _objectActive; } protected set => _objectActive = value; }

        private UMI3DAsyncProperty<bool> _objectActive;

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
                {
                    foreach (UMI3DUserFilter f in GetComponents<UMI3DUserFilter>())
                        AddConnectionFilter(f);
                }
            });

            toolId = id;
            objectInteractions = new UMI3DAsyncListProperty<AbstractInteraction>(toolId, UMI3DPropertyKeys.AbstractToolInteractions, Interactions, (i, u) => UMI3DEnvironment.Instance.useDto ? i.ToDto(u) : (object)i.Id());
            objectActive = new UMI3DAsyncProperty<bool>(toolId, UMI3DPropertyKeys.AbstractToolActive, Active);
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
        /// <returns>Created Dto.</returns>
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
            return new ProjectTool() { tool = this, releasable = releasable, users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined() };
        }

        /// <summary>
        /// Return Release Tool
        /// </summary>
        /// <param name="users">List of users to which this operation should be send.</param>
        /// <returns></returns>
        public ReleaseTool GetReleaseTool(HashSet<UMI3DUser> users = null)
        {
            return new ReleaseTool() { tool = this, users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined() };
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
            return new SwitchTool() { tool = this, toolToReplace = toolToReplace, releasable = releasable, users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined() };
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
        [SerializeField, Tooltip("Called when this tool is projected.")]
        public ProjectionEvent onProjection = new ProjectionEvent();

        /// <summary>
        /// Called when this tool is released.
        /// </summary>
        [SerializeField, Tooltip("Called when this tool is released.")]
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

        /// <summary>
        /// Information on projection
        /// </summary>
        public class ProjectionContent
        {
            /// <summary>
            /// Target user for projection
            /// </summary>
            public UMI3DUser user;
            /// <summary>
            /// Target bonetype for projection
            /// </summary>
            public uint boneType;
            /// <summary>
            /// Tool used in projection
            /// </summary>
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
            dto.interactions = objectInteractions.GetValue(user).Where(i => i != null).Select(i => i.Id()).ToList();
            dto.active = objectActive.GetValue(user);
        }

        /// <inheritdoc/>
        public virtual Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(Id())
                + UMI3DSerializer.Write(Display.name)
                + UMI3DSerializer.Write(Display.description)
                + Display.icon2D?.ToByte()
                + Display.icon3D?.ToByte()
                + UMI3DSerializer.WriteIBytableCollection(objectInteractions.GetValue(user).Where(i => i != null), user)
                + UMI3DSerializer.Write(objectActive.GetValue(user));
        }

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public virtual AbstractToolDto ToDto(UMI3DUser user)
        {
            AbstractToolDto dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

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