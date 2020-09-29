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

using umi3d.common;
using umi3d.common.interaction;
using umi3d.edk.interaction;
using UnityEngine;


namespace umi3d.edk.interaction
{
    static public class UMI3DBrowserRequestDispatcher
    {

        static public void DispatchBrowserRequest(UMI3DUser user,UMI3DDto dto)
        {
            switch (dto)
            {
                case TransactionDto transaction:
                    Debug.Log($"receive transaction from browser {user.Id()}:{dto}");
                    break;
                case HoveredDto hoveredDto:
                    UMI3DEnvironment.GetEntity<UMI3DInteractable>(hoveredDto.toolId)?.Hovered(user, hoveredDto);
                    break;
                case HoverStateChangedDto hoverStateChanged:
                    UMI3DEnvironment.GetEntity<UMI3DInteractable>(hoverStateChanged.toolId)?.HoverStateChanged(user, hoverStateChanged);
                    break;
                case EventStateChangedDto eventState:
                case EventTriggeredDto eventTriggered:
                case ParameterSettingRequestDto parameterSetting:
                case ManipulationRequestDto manipulation:
                    var interaction = dto as InteractionRequestDto;
                    UMI3DEnvironment.GetEntity<AbstractInteraction>(interaction.toolId)?.OnUserInteraction(user, interaction);
                    break;
                default:
                    Debug.LogWarning($"Missing case {dto.GetType()}");
                    break;
            }
        }
    }
}