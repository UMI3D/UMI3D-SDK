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
using System.Linq;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Client's side interactable object.
    /// </summary>
    /// <see cref="InteractableDto"/>
    public class Interactable : AbstractTool
    {
        public class Event : UnityEvent<Interactable> { }

        public static InteractableDto IdToDto(string id) { return (UMI3DEnvironmentLoader.GetEntity(id)?.Object as Interactable).dto; }

        /// <summary>
        /// Interactable dto describing this object.
        /// </summary>
        public InteractableDto dto;

        public bool NotifySubObject { get => dto?.notifySubObject ?? false; }

        /// <summary>
        /// Defines if this interactable has priority on browsers over other interactables with hasPriority false.
        /// </summary>
        public bool HasPriority { get => dto?.hasPriority ?? false; }


        ///<inheritdoc/>
        protected override AbstractToolDto abstractDto { get => dto; set => dto = value as InteractableDto; }

        public Interactable(InteractableDto dto) : base(dto)
        {
        }

        /// <summary>
        /// Notify the hovering of the object by the user (first frame only).
        /// </summary>
        public void HoverEnter(string bone, string hoveredObjectId, Vector3 position, Vector3 normal, Vector3 direction)
        {
            HoverStateChangedDto hoverDto = new HoverStateChangedDto()
            {
                toolId = id,
                hoveredObjectId = hoveredObjectId,
                boneType = bone,
                state = true,
                normal = normal,
                position = position,
                direction = direction
            };
            UMI3DClientServer.SendData(hoverDto, true);
        }

        /// <summary>
        /// Notify the end of the object's hovering by the user (first frame only).
        /// </summary>
        public void HoverExit(string bone, string hoveredObjectId, Vector3 position, Vector3 normal, Vector3 direction)
        {
            HoverStateChangedDto hoverDto = new HoverStateChangedDto()
            {
                toolId = id,
                hoveredObjectId = hoveredObjectId,
                boneType = bone,
                state = false,
                normal = normal,
                position = position,
                direction = direction
            };
            UMI3DClientServer.SendData(hoverDto, true);
        }

        /// <summary>
        /// Notify the hovering of the object (every frame).
        /// </summary>
        /// <param name="position">Object's point hovered (in object's local frame)</param>
        /// <param name="normal">Normal of the hovered point (in objects's local frame)</param>
        public void Hovered(string bone, string hoveredObjectId, Vector3 position, Vector3 normal, Vector3 direction)
        {
            if (dto.notifyHoverPosition)
            {
                HoveredDto hoverDto = new HoveredDto()
                {
                    toolId = id,
                    hoveredObjectId = hoveredObjectId,
                    boneType = bone,
                    normal = normal,
                    position = position,
                    direction = direction
                };
                UMI3DClientServer.SendData(hoverDto, false);
            }
        }

        ///<inheritdoc/>
        public override void Destroy()
        {
            foreach (var container in InteractableContainer.containers.Where(c => c.Interactable == this))
                GameObject.Destroy(container);
            base.Destroy();
        }
    }
}