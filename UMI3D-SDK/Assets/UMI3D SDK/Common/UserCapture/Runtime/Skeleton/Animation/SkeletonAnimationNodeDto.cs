/*
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

namespace umi3d.common.userCapture.animation
{
    /// <summary>
    /// DTO describing a subskeleton used for animation.
    /// </summary>
    /// A Skeleton node is a subskeleton with a Unity Animator
    /// that is packaged in a bundle. It is loaded the same way as a Mesh.
    public class SkeletonAnimationNodeDto : UMI3DMeshNodeDto
    {
        /// <summary>
        /// User that will use this skeleton.
        /// </summary>
        public ulong userId { get; set; }

        /// <summary>
        /// Level of priority of the skeleton animation.
        /// </summary>
        public int priority { get; set; }

        /// <summary>
        /// UMI3D animations ids supported by the bundled animator.
        /// </summary>
        public ulong[] relatedAnimationsId { get; set; }

        /// <summary>
        /// List of parameters that are updated by the browsers themselves based on skeleton movement.
        /// </summary>
        /// Available parameters are listed in <see cref="SkeletonAnimatorParameterKeys"/>.
        public SkeletonAnimationParameterDto[] animatorSelfTrackedParameters { get; set; }
    }
}