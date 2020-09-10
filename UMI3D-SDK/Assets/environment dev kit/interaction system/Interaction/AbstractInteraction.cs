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

using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// Abstract UMI3D interaction.
    /// </summary>
    public abstract class AbstractInteraction : MonoBehaviour, UMI3DEntity
    {

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
        private string interactionId;

        /// <summary>
        /// The public Getter for interactionId.
        /// </summary>
        public string Id()
        {
            if (interactionId == null && UMI3DEnvironment.Exists)
                Register();
            return interactionId;
        }

        void Register()
        {
            if (interactionId == null && UMI3DEnvironment.Exists)
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
        protected virtual void InitDefinition(string id)
        {
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


    }
}