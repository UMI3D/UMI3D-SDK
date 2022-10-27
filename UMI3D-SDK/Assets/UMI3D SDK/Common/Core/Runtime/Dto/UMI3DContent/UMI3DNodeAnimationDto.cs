﻿/*
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

using System.Collections.Generic;

namespace umi3d.common
{
    /// <summary>
    /// DTO describing an animation that triggers others animations by applying operations on the node.
    /// </summary>
    /// Animations of a same group could be triggered at the same time and executed in a particular order that way.
    public class UMI3DNodeAnimationDto : UMI3DAbstractAnimationDto
    {
        /// <summary>
        /// Duration of the animation, in seconds.
        /// </summary>
        public float duration = 10f;

        /// <summary>
        /// Operations to apply to create the animation.
        /// </summary>
        public List<OperationChainDto> animationChain = null;

        /// <summary>
        /// DTO describing a piece of animation to play using an operation.
        /// </summary>
        /// Animations in animation chain could be played simultaneously ou one after another by setting up the <see cref="startOnProgress"/> field.
        public class OperationChainDto
        {   
            /// <summary>
            /// Operation to apply.
            /// </summary>
            public AbstractOperationDto operation;
            /// <summary>
            /// Time in second after which this operation will be performed.
            /// </summary>
            public float startOnProgress = -1f;
        }
    }
}