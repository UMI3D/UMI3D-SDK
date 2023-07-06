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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.animation;
using UnityEngine;

namespace umi3d.edk.userCapture.animation
{
    [Serializable]
    public class SkeletonAnimationParameter
    {
        [SerializeField, ConstEnum(typeof(SkeletonAnimatorParameterKeys), typeof(uint))]
        public uint parameterKey;

        [SerializeField, Tooltip("Ranges to clamp value to a constant result when in an interval. " +
            "If no range is defined, the parameter value is directly given to the animator.")]
        public List<Range> ranges;

        public struct Range
        {
            public float startBound;
            public float endBound;
            public float result;
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
                parameterKey = parameterKey,
                ranges = ranges.Select(x => x.ToDto()).ToArray()
            };
        }
    }
}