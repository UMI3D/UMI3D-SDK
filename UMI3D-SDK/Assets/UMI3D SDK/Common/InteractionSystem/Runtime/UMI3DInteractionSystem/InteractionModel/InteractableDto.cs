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

namespace umi3d.common.interaction
{
    /// <summary>
    /// DTO describing an Interactable object, a specialized tool for game objects.
    /// </summary>
    [System.Serializable]
    public class InteractableDto : AbstractToolDto, IEntity
    {
        /// <summary>
        /// Is the hover position tracked ?
        /// </summary>
        public bool notifyHoverPosition = false;

        /// <summary>
        /// Defines if the sub-objects hoverring should be notified.
        /// </summary>
        public bool notifySubObject = false;

        /// <summary>
        /// Node id.
        /// </summary>
        public ulong nodeId;

        /// <summary>
        /// Defines if this interactable has priority on browsers over other interactables with hasPriority false.
        /// </summary>
        public bool hasPriority = false;

        /// <summary>
        /// Distance for a user to interact with this tool. If value < 0, no distance check
        /// </summary>
        public float interactionDistance = -1;

        /// <summary>
        /// Animation id of the animation triggered when the interactable starts being hovered.
        /// </summary>
        public ulong HoverEnterAnimationId;

        /// <summary>
        /// Animation id of the animation triggered when the interactable stops being hovered.
        /// </summary>
        public ulong HoverExitAnimationId;

        public InteractableDto() : base() { }
    }
}