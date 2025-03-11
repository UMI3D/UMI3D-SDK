/*
Copyright 2019 - 2025 Inetum

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
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public static class InteractableNotificationKeys 
    {
        public class HoverStateChanged
        {
            /// <summary>
            /// The state of the hovering.
            /// </summary>
            /// <remarks>
            /// Value is <see cref="InteractableHoverStateListener.State"/>
            /// </remarks>
            public const string State = "State";
            /// <summary>
            /// The collider that is hovered.
            /// </summary>
            /// <remarks>
            /// Value is <see cref="Collider"/>
            /// </remarks>
            public const string Collider = "Collider";
            /// <summary>
            /// The hoveredDto.
            /// </summary>
            /// <remarks>
            /// Value is <see cref="HoveredDto"/>
            /// </remarks>
            public const string HoveredDto = "HoveredDto";
        }
    }
}