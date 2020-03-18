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
using UnityEngine;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Abstract UMI3D interaction.
    /// </summary>
    public abstract class AbstractInteraction : MonoBehaviour
    {

        #region properties

        /// <summary>
        /// Indicates if the interaction is part of another.
        /// </summary>
        [HideInInspector] public bool IsSubInteraction = false;

        /// <summary>
        /// The interaction's 2D icon. 
        /// </summary>
        public CVEResource Icon2D = new CVEResource();

        /// <summary>
        /// The interaction's 3D icon. 
        /// </summary>
        public CVEResource Icon3D = new CVEResource();

        /// <summary>
        /// The interaction's name. 
        /// </summary>
        public string InteractionName = null;

        /// <summary>
        /// The interaction's unique id. 
        /// </summary>
        private string interactionId;

        /// <summary>
        /// The public Getter for interactionId.
        /// </summary>
        public string Id
        {
            get
            {
                if (interactionId == null)
                    interactionId = UMI3D.Scene.Register(this);
                return interactionId;
            }
        }

        /// <summary>
        /// Indicates if InitDefinition has been called.
        /// </summary>
        protected bool inited = false;

        public AsyncPropertiesHandler PropertiesHandler { protected set; get; }

        /// <summary>
        /// The interaction's name on last updates broadcast.
        /// </summary>
        protected string currentInteractionName;

        /// <summary>
        /// The interaction's icon on last updates broadcast.
        /// </summary>
        protected string currentIcon;

        /// <summary>
        /// Indicates the availability state of a user for the last frame check of visibility.
        /// </summary>
        protected Dictionary<UMI3DUser, bool> availableLastFrame = new Dictionary<UMI3DUser, bool>();

        internal AbstractCVETool currentTool = null;
        [SerializeField] internal AbstractCVETool tool = null;

        public void SetTool(AbstractCVETool tool)
        {
            if (currentTool == tool)
                return;
            if (currentTool != null)
                currentTool.RemoveInteraction(this);
            if (tool != null)
                tool.AddInteraction(this);
        }

        #endregion


        #region initialization

        /// <summary>
        /// Unity MonoBehaviour Start method.
        /// </summary>
        protected virtual void Start()
        {
            initDefinition();
            syncProperties();
        }

        /// <summary>
        /// Initialize interaction's properties.
        /// </summary>
        protected virtual void initDefinition()
        {
            interactionId = Id;
            PropertiesHandler = new AsyncPropertiesHandler();
            PropertiesHandler.DelegateBroadcastUpdate += BroadcastUpdates;
            PropertiesHandler.DelegatebroadcastUpdateForUser += BroadcastUpdates;
            inited = true;
            SetTool(tool);
        }

        #endregion


        #region updates

        private float clock = 0;
        protected virtual void Update()
        {
            checkForUpdates();
            syncProperties();
            if (clock > 1f / UMI3D.TargetFrameRate)
            {
                PropertiesHandler.BroadcastUpdates();
                clock = 0;
            }
            clock += Time.deltaTime;
        }

        /// <summary>
        /// Unity MonoBehaviour OnValidate method.
        /// </summary>
        protected virtual void OnValidate()
        {
            SetTool(tool);
        }

        /// <summary>
        /// automatically check if the object has been updated in the editor
        /// </summary>
        protected virtual void checkForUpdates()
        {
            if ((currentInteractionName != InteractionName) || (currentIcon != Icon2D.GetUrl()))
                PropertiesHandler.NotifyUpdate();
        }

        /// <summary>
        /// Update interaction's properties.
        /// </summary>
        protected virtual void syncProperties()
        {
            if (inited)
            {
                currentIcon = Icon2D.GetUrl();
                currentInteractionName = InteractionName;
            }
        }


        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        private void BroadcastUpdates()
        {
            foreach (UMI3DUser user in UMI3D.UserManager.GetUsers().Where(u => AvailableFor(u)))
                PropertiesHandler.BroadcastUpdates(user);
        }

        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        /// <param name="user">the user</param>
        private void BroadcastUpdates(UMI3DUser user)
        {
            if (!AvailableFor(user))
                return;
            var res = new UpdateInteractionDto();
            res.Entity = ConvertToDto(user) as AbstractInteractionDto;
            user.Send(res);
        }

        #endregion


        #region destroy

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

        #endregion


        #region availability

        /// <summary>
        /// Return true if this interaction is available for the given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check availability for</param>
        /// <returns></returns>
        public virtual bool AvailableAsChildFor(UMI3DUser user)
        {
            if (interactionId == null)
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
            if (interactionId == null || !enabled || !gameObject.activeInHierarchy)
                return false;
            if (IsSubInteraction)
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

        public void UpdateAvailabilityForUser(UMI3DUser user)
        {
            bool wasAvailable = (availableLastFrame.ContainsKey(user)) ? availableLastFrame[user] : false;
            bool available = AvailableFor(user);

            if (wasAvailable && !available && !IsSubInteraction)
                user.InteractionsIdsToRemove.Add(Id);

            if (!wasAvailable && available)
                user.InteractionsToLoad.Add(ConvertToDto(user) as AbstractInteractionDto);
        }

        #endregion



        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public abstract void OnUserInteraction(UMI3DUser user, JSONObject evt);

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public abstract AbstractInteractionDto ConvertToDto(UMI3DUser user);

    }


    /// <summary>
    /// Abstract interaction associated to a specific type of Data Tranfer Object.
    /// </summary>
    /// <typeparam name="DTO">type of Data Tranfer Object</typeparam>
    public abstract class AbstractInteraction<DTO> : AbstractInteraction where DTO : AbstractInteractionDto
    {
       
        /// <summary>
        /// Create an empty Dto for this interaction.
        /// </summary>
        /// <returns></returns>
        public abstract DTO CreateDto();

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>a DTO representing this interaction</returns>
        public virtual DTO ToDto(UMI3DUser user)
        {
            syncProperties();
            var dto = CreateDto();
            dto.id = Id;
            dto.Name = currentInteractionName;
            dto.ToolId = currentTool == null ? null : currentTool.Id;
            dto.ToolboxId = 
                (currentTool != null)
                    && (currentTool is CVETool)
                    && (currentTool as CVETool).currentToolbox == null 
                ? (currentTool as CVETool).currentToolbox.Id
                : null;
            dto.Icon2D = Icon2D.ToDto();
            dto.Icon3D = Icon3D.ToDto();
            return dto;
        }

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public override AbstractInteractionDto ConvertToDto(UMI3DUser user)
        {
            return ToDto(user);
        }

    }
}
