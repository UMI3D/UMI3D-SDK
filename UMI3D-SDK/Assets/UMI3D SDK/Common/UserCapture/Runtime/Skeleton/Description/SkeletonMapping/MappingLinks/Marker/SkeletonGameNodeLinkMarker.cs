﻿/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Indicate that a game node link should be generated
    /// </summary>
    public class SkeletonGameNodeLinkMarker : SkeletonMappingLinkMarker
    {
        /// <summary>
        /// Game node from which to generate the link
        /// </summary>
        [Tooltip("Game node from which to generate the link.")]
        public GameObject node;

        public override ISkeletonMappingLink ToLink()
        {
            return new GameNodeLink(node.transform);
        }
    }
}