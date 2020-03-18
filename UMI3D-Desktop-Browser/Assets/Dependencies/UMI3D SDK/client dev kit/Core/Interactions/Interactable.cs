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
using UnityEngine;
using umi3d.common;
using UnityEngine.Events;

namespace umi3d.cdk
{
    /// <summary>
    /// Client's side interactable object.
    /// </summary>
    /// <see cref="InteractableDto"/>
    public class Interactable : AbstractTool
    {
        public class Event : UnityEvent<Interactable> { }

        /// <summary>
        /// Interactable dto describing this object.
        /// </summary>
        public InteractableDto dto;

        /// <summary>
        /// Id of the interactable object 3D.
        /// </summary>
        /// <see cref="EmptyObject3DDto"/>
        public string objectId { get; protected set; }
        protected override AbstractToolDto abstractDto { get => dto; set => dto = value as InteractableDto; }

        /// <summary>
        /// Initialize an Interactable according to an AbstractToolDto.
        /// </summary>
        /// <param name="_dto"></param>
        public override void SetFromDto(AbstractToolDto _dto)
        {
            InteractableDto dto = _dto as InteractableDto;
            if (dto == null) return;
            base.SetFromDto(dto);
            objectId = dto.objectId;
        }

        /// <summary>
        /// Notify the hovering of the object by the user (first frame only).
        /// </summary>
        public void HoverEnter(string bone)
        {
            HoveredDto hoverDo = new HoveredDto()
            {
                abstractObject3DId = objectId,
                boneId = bone,
                State = true
            };
            UMI3DHttpClient.Interact(hoverDo);

        }

        /// <summary>
        /// Notify the end of the object's hovering by the user (first frame only).
        /// </summary>
        public void HoverExit(string bone)
        {
            HoveredDto hoverDo = new HoveredDto()
            {
                abstractObject3DId = objectId,
                boneId = bone,
                State = false
            };
            UMI3DHttpClient.Interact(hoverDo);
        }

        /// <summary>
        /// Notify the hovering of the object (every frame).
        /// </summary>
        /// <param name="position">Object's point hovered (in object's local frame)</param>
        /// <param name="normal">Normal of the hovered point (in objects's local frame)</param>
        public void Hovered(string bone, Vector3 position, Vector3 normal)
        {
            if (dto.trackHoverPosition)
            {
                HoveredDto hoverDo = new HoveredDto()
                {
                    abstractObject3DId = objectId,
                    boneId = bone,
                    State = true,
                    Normal = normal,
                    Position = position
                };
                UMI3DWebSocketClient.Interact(hoverDo);
            }
        }

    }
}