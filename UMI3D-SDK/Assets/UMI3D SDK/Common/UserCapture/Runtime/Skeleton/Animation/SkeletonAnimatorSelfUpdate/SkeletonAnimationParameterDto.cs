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
    /// DTO for properties of a parameter that is self-computed by the browsers based on the skeleton movement.
    /// </summary>
    public class SkeletonAnimationParameterDto
    {
        /// <summary>
        /// Key of the parameter to compute in <see cref="SkeletonAnimatorParameterKeys"/>. <br/>
        /// Each key result in a different computation in browsers.
        /// </summary>
        public uint parameterKey { get; set; }

        /// <summary>
        /// Ranges to clamp value to a constant result when in an interval. <br/>
        /// "If no range is defined, the parameter value is directly given to the animator.
        /// </summary>
        public RangeDto[] ranges { get; set; }

        /// <summary>
        ///  Ranges to clamp value to a certain result when in an interval.
        /// </summary>
        public class RangeDto
        {
            /// <summary>
            /// Start value of range
            /// </summary>
            public float startBound { get; set; }

            /// <summary>
            /// End value of range
            /// </summary>
            public float endBound { get; set; }

            /// <summary>
            /// Resulting value if the computed value is within <see cref="startBound"/> and <see cref="endBound"/>.
            /// </summary>
            public float result { get; set; }

            /// <summary>
            /// If true, raw value is used as result if computed value is within <see cref="startBound"/> and <see cref="endBound"/>.
            /// </summary>
            public bool rawValue { get; set; }
        }
    }
}