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

using umi3d.common;
using umi3d.common.interaction;


namespace umi3d.edk.interaction
{
    /// <summary>
    /// Dispactcher managing incoming <see cref="AbstractBrowserRequestDto"/> from clients.
    /// </summary>
    public static class UMI3DBrowserRequestDispatcher
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Interaction | DebugScope.Networking;

        /// <summary>
        /// Triggers the right events according to the received DTO.
        /// </summary>
        /// <param name="user">User sending the DTO</param>
        /// <param name="dto">Received DTO</param>
        public static void DispatchBrowserRequest(UMI3DUser user, UMI3DDto dto)
        {
            switch (dto)
            {
                case TransactionDto transaction:
                    UMI3DLogger.Log($"receive transaction from browser {user.Id()}:{dto}", scope);
                    break;
                case ToolReleasedDto toolReleased:
                    UMI3DEnvironment.GetEntityInstance<AbstractTool>(toolReleased.toolId)?.OnToolReleased(user, toolReleased);
                    break;
                case ToolProjectedDto toolProjected:
                    UMI3DEnvironment.GetEntityInstance<AbstractTool>(toolProjected.toolId)?.OnToolProjected(user, toolProjected);
                    break;
                case NotificationCallbackDto notificationCallback:
                    UMI3DEnvironment.GetEntityInstance<UMI3DNotification>(notificationCallback.id)?.OnCallbackReceived(notificationCallback);
                    break;
                case HoverStateChangedDto hoverStateChanged:
                    UMI3DEnvironment.GetEntityInstance<UMI3DInteractable>(hoverStateChanged.toolId)?.HoverStateChanged(user, hoverStateChanged);
                    break;
                case HoveredDto hoveredDto:
                    UMI3DEnvironment.GetEntityInstance<UMI3DInteractable>(hoveredDto.toolId)?.Hovered(user, hoveredDto);
                    break;
                case InteractionRequestDto interaction:
                    UMI3DEnvironment.GetEntityInstance<AbstractInteraction>(interaction.id)?.OnUserInteraction(user, interaction);
                    break;
                default:
                    UMI3DLogger.LogWarning($"Missing case {dto.GetType()}", scope);
                    break;
            }
        }

        /// <summary>
        /// Triggers the right events based on the received byte container and its key that is in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="operationKey"></param>
        /// <param name="container">Received byte container</param>
        public static void DispatchBrowserRequest(UMI3DUser user, uint operationKey, ByteContainer container)
        {
            ulong toolId = UMI3DSerializer.Read<ulong>(container);
            ulong interactionId, hoverredId;
            uint bonetype;
            SerializableVector3 bonePosition;
            SerializableVector4 boneRotation;
            switch (operationKey)
            {
                case UMI3DOperationKeys.Transaction:
                    UMI3DLogger.Log($"receive transaction from browser {user.Id()}", scope);
                    break;
                case UMI3DOperationKeys.ToolReleased:
                    bonetype = UMI3DSerializer.Read<uint>(container);
                    UMI3DEnvironment.GetEntityInstance<AbstractTool>(toolId)?.OnToolReleased(user, bonetype, container);
                    break;
                case UMI3DOperationKeys.ToolProjected:
                    bonetype = UMI3DSerializer.Read<uint>(container);
                    UMI3DEnvironment.GetEntityInstance<AbstractTool>(toolId)?.OnToolProjected(user, bonetype, container);
                    break;
                case UMI3DOperationKeys.NotificationCallback:
                    UMI3DEnvironment.GetEntityInstance<UMI3DNotification>(toolId)?.OnCallbackReceived(container);
                    break;
                case UMI3DOperationKeys.HoverStateChanged:
                    interactionId = UMI3DSerializer.Read<ulong>(container);
                    hoverredId = UMI3DSerializer.Read<ulong>(container);
                    bonetype = UMI3DSerializer.Read<uint>(container);
                    bonePosition = UMI3DSerializer.Read<SerializableVector3>(container);
                    boneRotation = UMI3DSerializer.Read<SerializableVector4>(container);
                    UMI3DEnvironment.GetEntityInstance<UMI3DInteractable>(toolId)?.HoverStateChanged(user, toolId, interactionId, hoverredId, bonetype, bonePosition, boneRotation, container);
                    break;
                case UMI3DOperationKeys.Hoverred:
                    interactionId = UMI3DSerializer.Read<ulong>(container);
                    hoverredId = UMI3DSerializer.Read<ulong>(container);
                    bonetype = UMI3DSerializer.Read<uint>(container);
                    bonePosition = UMI3DSerializer.Read<SerializableVector3>(container);
                    boneRotation = UMI3DSerializer.Read<SerializableVector4>(container);
                    UMI3DEnvironment.GetEntityInstance<UMI3DInteractable>(toolId)?.Hovered(user, toolId, interactionId, hoverredId, bonetype, bonePosition, boneRotation, container);
                    break;
                default:
                    if (UMI3DOperationKeys.InteractionRequest <= operationKey && operationKey <= UMI3DOperationKeys.UserTrackingFrame)
                    {
                        interactionId = UMI3DSerializer.Read<ulong>(container);
                        hoverredId = UMI3DSerializer.Read<ulong>(container);
                        bonetype = UMI3DSerializer.Read<uint>(container);
                        bonePosition = UMI3DSerializer.Read<SerializableVector3>(container);
                        boneRotation = UMI3DSerializer.Read<SerializableVector4>(container);
                        UMI3DEnvironment.GetEntityInstance<AbstractInteraction>(interactionId)?.OnUserInteraction(user, operationKey, toolId, interactionId, hoverredId, bonetype, bonePosition, boneRotation, container);
                        break;
                    }
                    UMI3DLogger.LogWarning($"Missing case {operationKey}", scope);
                    break;
            }
        }
    }
}