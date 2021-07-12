﻿/*
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

using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.interaction
{
    public class UMI3DEvent : AbstractInteraction
    {
        public bool Hold = false;

        /// <summary>
        /// Called the first frame of hoverring.
        /// </summary>
        [SerializeField]
        public InteractionEvent onHold = new InteractionEvent();

        /// <summary>
        /// Called the first frame after hoverring.
        /// </summary>
        [SerializeField]
        public InteractionEvent onRelease = new InteractionEvent();

        /// <summary>
        /// Called the first frame after hoverring.
        /// </summary>
        [SerializeField]
        public InteractionEvent onTrigger = new InteractionEvent();

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
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

        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.EventTriggered:
                    onTrigger.Invoke(new InteractionEventContent(user, toolId, interactionId, hoverredId, boneType));
                    break;
                case UMI3DOperationKeys.EventStateChanged:
                    var active = UMI3DNetworkingHelper.Read<bool>(container);
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

        public override Bytable ToByte(UMI3DUser user)
        {
            return base.ToByte(user)
                    + UMI3DNetworkingHelper.Write(Hold);
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new EventDto();
        }

        /// <summary>
        /// Write the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            (dto as EventDto).hold = Hold;
        }

    }
}