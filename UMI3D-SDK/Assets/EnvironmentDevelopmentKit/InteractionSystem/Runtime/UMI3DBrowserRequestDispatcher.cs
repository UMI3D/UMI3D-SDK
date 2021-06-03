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
using UnityEngine;


namespace umi3d.edk.interaction
{
    static public class UMI3DBrowserRequestDispatcher
    {

        static public void DispatchBrowserRequest(UMI3DUser user, UMI3DDto dto)
        {
            switch (dto)
            {
                case TransactionDto transaction:
                    Debug.Log($"receive transaction from browser {user.Id()}:{dto}");
                    break;
                case ToolReleasedDto toolReleased:
                    UMI3DEnvironment.GetEntity<AbstractTool>(toolReleased.toolId)?.OnToolReleased(user, toolReleased);
                    break;
                case ToolProjectedDto toolProjected:
                    UMI3DEnvironment.GetEntity<AbstractTool>(toolProjected.toolId)?.OnToolProjected(user, toolProjected);
                    break;
                case HoverStateChangedDto hoverStateChanged:
                    UMI3DEnvironment.GetEntity<UMI3DInteractable>(hoverStateChanged.toolId)?.HoverStateChanged(user, hoverStateChanged);
                    break;
                case HoveredDto hoveredDto:
                    UMI3DEnvironment.GetEntity<UMI3DInteractable>(hoveredDto.toolId)?.Hovered(user, hoveredDto);
                    break;
                case InteractionRequestDto interaction:
                    UMI3DEnvironment.GetEntity<AbstractInteraction>(interaction.id)?.OnUserInteraction(user, interaction);
                    break;
                default:
                    Debug.LogWarning($"Missing case {dto.GetType()}");
                    break;
            }
        }

        static public void DispatchBrowserRequest(UMI3DUser user, uint operationKey, byte[] array, int position, int length)
        {
            var toolId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
            ulong interactionId, hoverredId;
            uint bonetype;
            switch (operationKey)
            {
                case UMI3DOperationKeys.Transaction:
                    Debug.Log($"receive transaction from browser {user.Id()}");
                    break;
                case UMI3DOperationKeys.ToolReleased:
                    bonetype = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                    UMI3DEnvironment.GetEntity<AbstractTool>(toolId)?.OnToolReleased(user, bonetype, array, position, length);
                    break;
                case UMI3DOperationKeys.ToolProjected:
                    bonetype = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                    UMI3DEnvironment.GetEntity<AbstractTool>(toolId)?.OnToolProjected(user, bonetype, array, position, length);
                    break;
                case UMI3DOperationKeys.HoverStateChanged:
                    interactionId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    hoverredId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    bonetype = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                    UMI3DEnvironment.GetEntity<UMI3DInteractable>(toolId)?.HoverStateChanged(user, toolId, interactionId, hoverredId, bonetype, array, position, length);
                    break;
                case UMI3DOperationKeys.Hoverred:
                    interactionId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    hoverredId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    bonetype = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                    UMI3DEnvironment.GetEntity<UMI3DInteractable>(toolId)?.Hovered(user, toolId, interactionId, hoverredId, bonetype, array, position, length);
                    break;
                default:
                    if (UMI3DOperationKeys.InteractionRequest <= operationKey && operationKey <= UMI3DOperationKeys.UserTrackingFrame)
                    {
                        interactionId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                        hoverredId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                        bonetype = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                        UMI3DEnvironment.GetEntity<AbstractInteraction>(interactionId)?.OnUserInteraction(user, operationKey, toolId, interactionId, hoverredId, bonetype, array, position, length);
                        break;
                    }
                    Debug.LogWarning($"Missing case {operationKey}");
                    break;
            }
        }
    }
}