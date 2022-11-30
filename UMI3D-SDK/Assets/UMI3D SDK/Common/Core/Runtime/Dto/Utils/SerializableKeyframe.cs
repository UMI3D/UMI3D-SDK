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
    public class SerializableKeyframe : UMI3DDto
    {
        /// <summary>
        /// (time; value);
        /// </summary>
        public SerializableVector2 point;

        /// <summary>
        /// (slope; weight);
        /// </summary>
        public SerializableVector2 intTangeant;

        /// <summary>
        /// (slope; weight);
        /// </summary>
        public SerializableVector2 outTangeant;

        public SerializableKeyframe() : base()
        {
            point = new SerializableVector2(0, 0);

            intTangeant = new SerializableVector2(0, 0);

            outTangeant = new SerializableVector2(0, 0);
        }

        public SerializableKeyframe(float time, float value) : this(time, value, 0, 0, 0, 0)
        {

        }

        public SerializableKeyframe(float time, float value, float inTangeant, float outTangeant, float intWeight, float outWeight) : base()
        {
            this.point = new SerializableVector2(time, value);

            this.intTangeant = new SerializableVector2(inTangeant, intWeight);

            this.outTangeant = new SerializableVector2(outTangeant, outWeight);
        }

        public SerializableKeyframe(Keyframe frame) : base()
        {
            point = new SerializableVector2(frame.time, frame.value);

            intTangeant = new SerializableVector2(frame.inTangent, frame.inWeight);

            outTangeant = new SerializableVector2(frame.outTangent, frame.outWeight);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Keyframe : time : {point.X} s, value = {point.Y}, inTangeant ({intTangeant.X}, {intTangeant.Y}), outTangeant ({outTangeant.X}, {outTangeant.Y}).";
        }

        public static implicit operator SerializableKeyframe(Keyframe frame)
        {
            return new SerializableKeyframe(frame);
        }

        public static implicit operator Keyframe(SerializableKeyframe frame)
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

