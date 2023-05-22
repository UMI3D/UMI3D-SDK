/*
Copyright 2019 - 2022 Inetum

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

namespace umi3d.common
{

    /// <summary>
    /// Serializable implementation of a keyframe.
    /// </summary>
    [Serializable]
    public class KeyframeDto : UMI3DDto
    {
        /// <summary>
        /// (time; value);
        /// </summary>
        public Vector2Dto point { get; set; } = new Vector2Dto();

        /// <summary>
        /// (slope; weight);
        /// </summary>
        public Vector2Dto intTangeant { get; set; } = new Vector2Dto();

        /// <summary>
        /// (slope; weight);
        /// </summary>
        public Vector2Dto outTangeant { get; set; } = new Vector2Dto();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Keyframe : time : {point.X} s, value = {point.Y}, inTangeant ({intTangeant.X}, {intTangeant.Y}), outTangeant ({outTangeant.X}, {outTangeant.Y}).";
        }
    }
}

