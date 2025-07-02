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

using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.cdk.interaction
{
    public class InteractionScreenSpace : MonoBehaviour
    {
        public Button _button;
        public Interactable _interactable;
        public InteractableContainer _container;
        public GameObject _go;
        public ReadUMI3DExtensionData _value;

        [ContextMenu("AssignListenner ")]
        public void AssignListenner()
        {
            if(_go.TryGetComponent<InteractableContainer>(out InteractableContainer container))
            {
                _container = container;
                _button.onClick.AddListener(SendEvent);
            }
            
        }

        public void SendEvent()
        {
            foreach (var item in _container.Interactable.interactionsLoaded)
            {
                if (item is EventDto _eventDto)
                {
                    var eventdto = new common.interaction.EventTriggeredDto
                    {
                        boneType = 3,
                        id = _eventDto.id,
                        toolId = _interactable.id,
                        hoveredObjectId = (_value.dto as InteractableDto).id,
                        bonePosition = common.Vector3Dto.zero,
                        boneRotation = Quaternion.identity.Dto(),
                        environmentId = _value.environmentId
                    };
                    cdk.UMI3DClientServer.SendRequest(eventdto, true);
                    break;
                }
            }
        }
    }
}
