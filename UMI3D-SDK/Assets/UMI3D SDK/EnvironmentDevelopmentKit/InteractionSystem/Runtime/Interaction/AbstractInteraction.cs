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

using BeardedManStudios.Forge.Networking.Unity;
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
    /// Abstract UMI3D interaction. Base class for all interactions.
    /// </summary>
    public abstract class AbstractInteraction : MonoBehaviour, UMI3DLoadableEntity, IBytable
    {

        /// <summary>
        /// Class for event rising on Interaction. 
        /// </summary>
        [Serializable]
        public class InteractionEvent : UnityEvent<InteractionEventContent> { }

        /// <summary>
        /// <see cref="InteractionEvent"/> content indicating who is interacting with which interaction.
        /// </summary>
        [Serializable]
        public class InteractionEventContent
        {
            /// <summary>
            /// User who performed the interaction.
            /// </summary>
            public UMI3DUser user { get; private set; }
            /// <summary>
            /// Bonetype use to performe the interaction.
            /// </summary>
            public uint boneType { get; private set; }
            /// <summary>
            /// The global position of the bone use to performe the interaction.
            /// </summary>
            public SerializableVector3 bonePosition { get; private set; }
            /// <summary>
            /// The global rotation of the bone use to performe the interaction.
            /// </summary>
            public SerializableVector4 boneRotation { get; private set; }
            /// <summary>
            /// The id of the currently hoverred object.
            /// It will be always null for an Interaction inside a Tool.
            /// For an Interaction inside an Interactable, it could be the Id of the Interactable associated object, or the Id of a sub-object if Interaction.notifyHoverPosition == true.
            /// </summary>
            public ulong hoveredId { get; private set; }
            /// <summary>
            /// Id of the tool.
            /// </summary>
            public ulong toolId { get; private set; }
            /// <summary>
            /// Id of the Interaction.
            /// </summary>
            public ulong interactionId { get; private set; }

            public InteractionEventContent(UMI3DUser user, InteractionRequestDto dto)
            {
                this.user = user;
                boneType = dto.boneType;
                hoveredId = dto.hoveredObjectId;
                toolId = dto.toolId;
                interactionId = dto.id;
                bonePosition = dto.bonePosition;
                boneRotation = dto.boneRotation;
            }

            public InteractionEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType, SerializableVector3 bonePosition, SerializableVector4 boneRotation)
            {
                this.user = user;
                this.boneType = boneType;
                this.hoveredId = hoveredObjectId;
                this.toolId = toolId;
                this.interactionId = id;
                this.bonePosition = bonePosition;
                this.boneRotation = boneRotation;
            }
        }

        #region properties
        public UMI3DAsyncProperty<UIRect> ObjectUIRect { get { Register(); return _objectUIRect; } protected set => _objectUIRect = value; }

        private UMI3DAsyncProperty<UIRect> _objectUIRect;

        /// <summary>
        /// Link an ui rect to an interactable.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Check this box if a collider is attached to that node.")]
        public UIRect UILink = null;

        /// <summary>
        /// Indicates if the interaction is part of another.
        /// </summary>
        [HideInInspector]
        public bool IsSubInteraction = false;

        /// <summary>
        /// Available display information on the interaction
        /// </summary>
        public InteractionDisplay Display = new InteractionDisplay()
        {
            name = null
        };


        /// <summary>
        /// The interaction's unique id. 
        /// </summary>
        private ulong interactionId;

        /// <summary>
        /// The public Getter for interactionId.
        /// </summary>
        public ulong Id()
        {
            if (interactionId == 0 && UMI3DEnvironment.Exists)
                Register();
            return interactionId;
        }

        /// <summary>
        /// Register the interaction in the <see cref="UMI3DEnvironment"/> if necessary.
        /// </summary>
        protected virtual void Register()
        {
            if (interactionId == 0 && UMI3DEnvironment.Exists)
            {
                interactionId = UMI3DEnvironment.Register(this);
                InitDefinition(interactionId);
            }
        }

        /// <summary>
        /// Indicates if InitDefinition has been called.
        /// </summary>
        protected bool inited = false;

        #endregion


        #region initialization

        /// <summary>
        /// Initialize interaction's properties.
        /// </summary>
        protected virtual void InitDefinition(ulong id)
        {
            MainThreadManager.Run(() =>
            {
                if (this != null)
                {
                    foreach (UMI3DUserFilter f in GetComponents<UMI3DUserFilter>())
                        AddConnectionFilter(f);
                }
            });
            interactionId = id;
            inited = true;

            ObjectUIRect = new UMI3DAsyncProperty<UIRect>(id, UMI3DPropertyKeys.Interaction_UI_Link, UILink, null, (o, u) => o.Equals(u));

        }

        #endregion

        #region destroy

        /// <summary>
        /// Unity MonoBehaviour OnDestroy method.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UMI3DEnvironment.Remove(this);
        }

        #endregion

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="request">Interaction request</param>
        public abstract void OnUserInteraction(UMI3DUser user, InteractionRequestDto request);

        public abstract void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, SerializableVector3 bonePosition, SerializableVector4 boneRotation, ByteContainer container);

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public AbstractInteractionDto ToDto(UMI3DUser user)
        {
            AbstractInteractionDto dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected abstract AbstractInteractionDto CreateDto();

        /// <summary>
        /// Write the AbstractInteractionDto properties in an object AbstractInteractionDto is assignable from.
        /// </summary>
        /// <param name="dto">The AbstractInteractionDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected virtual void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            dto.name = Display.name;
            dto.icon2D = Display.icon2D.ToDto();
            dto.icon3D = Display.icon3D.ToDto();
            dto.id = Id();
            dto.description = Display.description;
            dto.uiLinkId = ObjectUIRect.GetValue(user)?.Id() ?? 0;
        }

        /// <summary>
        /// Retrieve the key associated to the interaction from <see cref="UMI3DInteractionKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract byte GetInteractionKey();

        /// <summary>
        /// Convert interaction to a <see cref="Bytable"/> container for a given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetInteractionKey())
                    + UMI3DSerializer.Write(Id())
                    + UMI3DSerializer.Write(Display.name)
                    + Display.icon2D.ToByte()
                    + Display.icon3D.ToByte()
                    + UMI3DSerializer.Write(Display.description);
        }

        /// <inheritdoc/>
        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            if (parameters.Length < 1)
                return ToBytes(null);
            return ToBytes(parameters[0] as UMI3DUser);
        }

        /// <inheritdoc/>
        bool IBytable.IsCountable()
        {
            return true;
        }

        public LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user);
        }

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        /// <inheritdoc/>
        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        /// <inheritdoc/>
        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        /// <inheritdoc/>
        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion
    }
}