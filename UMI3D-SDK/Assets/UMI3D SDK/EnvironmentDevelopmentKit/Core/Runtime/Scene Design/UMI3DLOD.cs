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
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DLOD : MonoBehaviour
    {
        [Serializable]
        public struct UMI3DLODGroup
        {
            /// <summary>
            /// Nodes to display or not according to the current level of detail.
            /// </summary>
            public List<UMI3DNode> nodes;

            /// <summary>
            /// The screen relative height to use for the transition [0-1].
            /// </summary>
            public float screenSize;

            /// <summary>
            /// Value between 0 and 1 that define the transition zone size.
            /// </summary>
            public float fadeTransition;
        }

        public List<UMI3DLODGroup> groups;
    }
}

