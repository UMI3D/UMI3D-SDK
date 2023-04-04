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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// Interaction focused on Hold/Project/Release event from the UMI3D workflow. <br/>
    /// See <seealso cref="ProjectTool"/> and <seealso cref="ReleaseTool"/>.
    /// </summary>
    public class UMI3DEvent : AbstractInteraction
    {
        #region fields
        /// <summary>
        /// Is the interaction an hold interaction (continuous) or a trigger one (instantaneous) ?
        /// </summary>
        [Tooltip("True if the interaction is an hold interaction (continuous) instead of a trigger one (instantaneous)")]
        public bool Hold = false;

        /// <summary>
        /// Called during the first frame when the interaction is held by a user.
        /// </summary>
        [SerializeField, Tooltip("Called during the first frame when the interaction is held by a user.")]
        public InteractionEvent onHold = new InteractionEvent();

        /// <summary>
        /// Called during the first frame after the interaction is no longer held by for a user.
        /// </summary>
        [SerializeField, Tooltip("Called during the first frame after the interaction is no longer held by a user.")]
        public InteractionEvent onRelease = new InteractionEvent();

        /// <summary>
        /// Called when the interaction is triggerred by a user.
        /// </summary>
        [SerializeField, Tooltip("Called when the interaction is triggerred by a user.")]
        public InteractionEvent onTrigger = new InteractionEvent();

        /// <summary>
        /// Animation triggered when the interaction is triggered.
        /// </summary>
        [SerializeField, Tooltip("Client animation triggered when the interaction is triggerred by a user.")]
        public ulong TriggerPoseOverriderContainerID;
        /// <summary>
        /// Animation triggered when the interaction is released.
        /// </summary>
        [SerializeField, Tooltip("Client animation triggered when is released by a user.")]
        public ulong ReleasePoseOverriderContainerID;

        /// <summary>
        /// Animation Asych property of the animatiuon triggered when the interaction is triggered
        /// </summary>
        private UMI3DAsyncProperty<ulong> _triggerPoseOverriderContainerID;
        /// <summary>
        /// Animation Asych property of the animatiuon triggered when the interaction is realeased
        /// </summary>
        private UMI3DAsyncProperty<ulong> _releasePoseOverriderContainerId;

        /// <summary>
        /// Animation Asych Attribute of the animatiuon triggered when the interaction is triggered
        /// </summary>
        public UMI3DAsyncProperty<ulong> triggerPoseOverriderContainerId { get { Register(); return _triggerPoseOverriderContainerID; } set => _triggerPoseOverriderContainerID = value; }
        /// <summary>
        /// Animation Asych Attribute of the animatiuon triggered when the interaction is realeased
        /// </summary>
        public UMI3DAsyncProperty<ulong> releasePoseOverriderContainerId { get { Register(); return _releasePoseOverriderContainerId; } set => _releasePoseOverriderContainerId = value; }

        #endregion

        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            triggerPoseOverriderContainerId = new UMI3DAsyncProperty<ulong>(id, UMI3DPropertyKeys.EventTriggerAnimation, TriggerPoseOverriderContainerID, null);
            releasePoseOverriderContainerId = new UMI3DAsyncProperty<ulong>(id, UMI3DPropertyKeys.EventReleaseAnimation, ReleasePoseOverriderContainerID, null);
        }

        public void SetReleaseExitPose(ulong poseOverriderDtosID)
        {
            ReleasePoseOverriderContainerID = poseOverriderDtosID;  
        }

        public void SetTriggerExitPose(ulong poseOverriderDtosID)
        {
            TriggerPoseOverriderContainerID^= poseOverriderDtosID;
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">Interacting User</param>
        /// <param name="interactionRequest">Received interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case EventTriggeredDto eventTriggered:
                    onTrigger.Invoke(new InteractionEventContent(user, interactionRequest));
                    break;
                case EventStateChangedDto eventStateChanged:
                    if (eventStateChanged.active)
                    {
                        onHold.Invoke(new InteractionEventContent(user, interactionRequest));
                    }
                    else
                    {
                        onRelease.Invoke(new InteractionEventContent(user, interactionRequest));
                    }
                    break;
            }
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">Interacting user</param>
        /// <param name="operationId">Operatin id in <see cref="UMI3DOperationKeys"/></param>
        /// <param name="toolId">Tool id in </param>
        /// <param name="interactionId">Id of the interaction</param>
        /// <param name="hoverredId">The id of the currently hoverred object.</param>
        /// <param name="boneType">User's used bone</param>
        /// <param name="container">Byte container</param>
        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.EventTriggered:
                    onTrigger.Invoke(new InteractionEventContent(user, toolId, interactionId, hoverredId, boneType));
                    break;
                case UMI3DOperationKeys.EventStateChanged:
                    bool active = UMI3DSerializer.Read<bool>(container);
                    if (active)
                    {
                        onHold.Invoke(new InteractionEventContent(user, toolId, interactionId, hoverredId, boneType));
                    }
                    else
                    {
                        onRelease.Invoke(new InteractionEventContent(user, toolId, interactionId, hoverredId, boneType));
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                    + UMI3DSerializer.Write(Hold);
                    //+ ((UMI3DLoadableEntity)this.triggerAnimation).ToBytes(user)
                    //+ ((UMI3DLoadableEntity)this.releaseAnimation).ToBytes(user);
        }

        /// <inheritdoc/>
        protected override AbstractInteractionDto CreateDto()
        {
            return new EventDto();
        }

        /// <inheritdoc/>
        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.Event;
        }

        /// <inheritdoc/>
        protected override void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            if (dto is EventDto _dto)
            {
                _dto.hold = Hold;
                _dto.TriggerPoseOverriderId = (ulong)triggerPoseOverriderContainerId.GetValue(user);
                _dto.ReleasePoseOverriderId = (ulong)releasePoseOverriderContainerId.GetValue(user);
            }
        }
    }
}