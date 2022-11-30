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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Serializable implementation of an animationCurve.
    /// </summary>
    [Serializable]
    public class SerializableAnimationCurve : UMI3DDto
    {
        /// <summary>
        /// Animation keys.
        /// </summary>
        public List<SerializableKeyframe> keys;

        public SerializableAnimationCurve() : base()
        {
            this.keys = new List<SerializableKeyframe>();
        }

        public SerializableAnimationCurve(AnimationCurve curve) : base()
        {
            this.keys = new List<SerializableKeyframe>();

            foreach (var key in curve.keys)
            {
                this.keys.Add(new SerializableKeyframe(key));
            }
        }

        public SerializableAnimationCurve(List<SerializableKeyframe> keys) : base()
        {
            this.keys = new List<SerializableKeyframe>();

            foreach (var key in keys)
            {
                this.keys.Add(new SerializableKeyframe(key));
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string res = $"Animation Curve : {keys.Count} keys. \n";
            for (int i = 0; i < keys.Count; i++)
            {
                res += i + " -> " + keys[i].ToString() + " \n";
            }
            return res;
        }

        public static implicit operator SerializableAnimationCurve(AnimationCurve c)
        {
            if (c == null) return default;
            return new SerializableAnimationCurve(c);
        }


        public static implicit operator AnimationCurve(SerializableAnimationCurve c)
        {
            if (c == null || c.keys.Count == 0) return new AnimationCurve();

            List<Keyframe> keys = new List<Keyframe>();
            foreach (var k in c.keys)
                keys.Add((Keyframe)k);

            return new AnimationCurve(keys.ToArray());
        }
    }
}