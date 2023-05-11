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
        public Vector2Dto point;

        /// <summary>
        /// (slope; weight);
        /// </summary>
        public Vector2Dto intTangeant;

        /// <summary>
        /// (slope; weight);
        /// </summary>
        public Vector2Dto outTangeant;

        public KeyframeDto() : base()
        {
            point = new Vector2Dto(0, 0);

            intTangeant = new Vector2Dto(0, 0);

            outTangeant = new Vector2Dto(0, 0);
        }

        public KeyframeDto(float time, float value) : this(time, value, 0, 0, 0, 0)
        {

        }

        public KeyframeDto(float time, float value, float inTangeant, float outTangeant, float intWeight, float outWeight) : base()
        {
            this.point = new Vector2Dto(time, value);

            this.intTangeant = new Vector2Dto(inTangeant, intWeight);

            this.outTangeant = new Vector2Dto(outTangeant, outWeight);
        }

        public KeyframeDto(Keyframe frame) : base()
        {
            point = new Vector2Dto(frame.time, frame.value);

            intTangeant = new Vector2Dto(frame.inTangent, frame.inWeight);

            outTangeant = new Vector2Dto(frame.outTangent, frame.outWeight);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Keyframe : time : {point.X} s, value = {point.Y}, inTangeant ({intTangeant.X}, {intTangeant.Y}), outTangeant ({outTangeant.X}, {outTangeant.Y}).";
        }

        public static implicit operator KeyframeDto(Keyframe frame)
        {
            return new KeyframeDto(frame);
        }

        public static implicit operator Keyframe(KeyframeDto frame)
        {
            if (frame == null) return default;
            return new Keyframe
            {
                time = frame.point.X,
                value = frame.point.Y,
                inTangent = frame.intTangeant.X,
                inWeight = frame.intTangeant.Y,
                outTangent = frame.outTangeant.X,
                outWeight = frame.outTangeant.Y
            };
        }
    }
}

