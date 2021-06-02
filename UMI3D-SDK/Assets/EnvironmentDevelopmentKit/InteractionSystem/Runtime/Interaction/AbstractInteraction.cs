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
    /// Abstract UMI3D interaction.
    /// </summary>
    public abstract class AbstractInteraction : MonoBehaviour, UMI3DMediaEntity, IByte
    {

        /// <summary>
        /// Class for event rising on Interaction. 
        /// </summary>
        [Serializable]
        public class InteractionEvent : UnityEvent<InteractionEventContent> { }

        /// <summary>
        /// InteractionEvent Content. 
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
            }

            public InteractionEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType)
            {
                this.user = user;
                this.boneType = boneType;
                this.hoveredId = hoveredObjectId;
                this.toolId = toolId;
                this.interactionId = id;
            }
        }

        #region properties

        /// <summary>
        /// Indicates if the interaction is part of another.
        /// </summary>
        [HideInInspector] public bool IsSubInteraction = false;

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

        void Register()
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
                    foreach (var f in GetComponents<UMI3DUserFilter>())
                        AddConnectionFilter(f);
            });
            interactionId = id;
            inited = true;
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

        public abstract void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, uint ParameterId, byte[] array, int position, int length);

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public AbstractInteractionDto ToDto(UMI3DUser user)
        {
            var dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected abstract AbstractInteractionDto CreateDto();

        /// <summary>
        /// Writte the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected virtual void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            dto.name = Display.name;
            dto.icon2D = Display.icon2D.ToDto();
            dto.icon3D = Display.icon3D.ToDto();
            dto.id = Id();
            dto.description = Display.description;
        }

        public virtual (int, Func<byte[], int, int>) ToByte(UMI3DUser user)
        {
            throw new NotImplementedException();
        }

        (int, Func<byte[], int, int>) IByte.ToByteArray(params object[] parameters)
        {
            if (parameters.Length < 1)
                return ToByte(null);
            return ToByte(parameters[0] as UMI3DUser);
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