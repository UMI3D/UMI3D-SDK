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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.animation;
using UnityEngine;

namespace umi3d.edk.userCapture.animation
{
    /// <summary>
    /// Properties of a parameter that is self-computed by the browsers based on the skeleton movement.
    /// </summary>
    [Serializable]
    public class SkeletonAnimationParameter
    {
        /// <summary>
        /// Key of the parameter name in the animator.
        /// </summary>
        public string parameterName;

        /// <summary>
        /// Key of the parameter to compute in <see cref="SkeletonAnimatorParameterKeys"/>. <br/>
        /// Each key result in a different computation in browsers.
        /// </summary>
        [SerializeField, ConstEnum(typeof(SkeletonAnimatorParameterKeys), typeof(uint))]
        public uint parameterKey;

        /// <summary>
        /// Ranges to clamp value to a constant result when in an interval. <br/>
        /// "If no range is defined, the parameter value is directly given to the animator.
        /// </summary>
        [SerializeField, Tooltip("Ranges to clamp value to a constant result when in an interval. " +
            "If no range is defined, the parameter value is directly given to the animator.")]
        public List<Range> ranges;

        /// <summary>
        ///  Ranges to clamp value to a constant result when in an interval.
        /// </summary>
        public struct Range
        {
            /// <summary>
            /// Start value of range
            /// </summary>
            public float startBound;

            /// <summary>
            /// End value of range
            /// </summary>
            public float endBound;

            /// <summary>
            /// Resulting value if the computed value is within <see cref="startBound"/> and <see cref="endBound"/>.
            /// </summary>
            public float result;

            /// <summary>
            /// If true, raw value is used as result if computed value is within <see cref="startBound"/> and <see cref="endBound"/>.
            /// </summary>
            public bool rawValue;

            public SkeletonAnimationParameterDto.RangeDto ToDto()
            {
                return new SkeletonAnimationParameterDto.RangeDto()
                {
                    startBound = startBound,
                    endBound = endBound,
                    result = result,
                    rawValue = rawValue
                };
            }
        }

        public SkeletonAnimationParameterDto ToDto()
        {
            return new SkeletonAnimationParameterDto()
            {
                parameterName = parameterName,
                parameterKey = parameterKey,
                ranges = ranges.Select(x => x.ToDto()).ToArray()
            };
        }
    }
}