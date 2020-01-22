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
using UnityEngine;

namespace umi3d.edk
{
    public abstract class AbstractCVETool : MonoBehaviour
    {
        #region properties

        public InteractionDisplay Display = new InteractionDisplay()
        {
            name = "new tool"
        };

        public readonly List<GenericInteraction> Interactions = new List<GenericInteraction>();

        public void RemoveInteraction(GenericInteraction interaction)
        {
            interaction.tool = null;
            interaction.currentTool = null;
            Interactions.Remove(interaction);
        }

        public void AddInteraction(GenericInteraction interaction)
        {
            interaction.tool = this;
            interaction.currentTool = this;
            if (!Interactions.Contains(interaction))
                Interactions.Add(interaction);
        }

        /// <summary>
        /// The tool's unique id. 
        /// </summary>
        protected string toolId;

        /// <summary>
        /// The public Getter for interactionId.
        /// </summary>
        public string Id
        {
            get
            {
                if (toolId == null)
                    toolId = UMI3D.Scene.Register(this);
                return toolId;
            }
        }

        public string description;

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
            toolId = Id;
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
            if (toolId == null)
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
            if (toolId == null || !enabled || !gameObject.activeInHierarchy)
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

        public abstract bool UpdateAvailabilityForUser(UMI3DUser user);

        #endregion

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public abstract AbstractToolDto ConvertToDto(UMI3DUser user);
    }
}