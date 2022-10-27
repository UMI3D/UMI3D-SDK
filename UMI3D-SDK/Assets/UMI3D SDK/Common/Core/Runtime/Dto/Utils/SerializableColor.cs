/*
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

using System;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Serializable implementation of a color using RGBA channels with float values.
    /// </summary>
    [Serializable]
    public class SerializableColor : UMI3DDto
    {
        /// <summary>
        /// Red color channel. Value between 0 and 1.
        /// </summary>
        public float R;

        /// <summary>
        /// Green color channel. Value between 0 and 1.
        /// </summary>
        public float G;

        /// <summary>
        /// Blue color channel. Value between 0 and 1.
        /// </summary>
        public float B;

        /// <summary>
        /// Alpha opacity channel. Value between 0 and 1.
        /// </summary>
        public float A;

        public SerializableColor() : base()
        {
            R = 0;
            G = 0;
            B = 0;
            A = 0;
        }

        public SerializableColor(float r, float g, float b, float a) : base()
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ((Color)this).ToString();
        }

        public static implicit operator SerializableColor(Color c)
        {
            if (c == null) return default;
            return new SerializableColor(c.r, c.g, c.b, c.a);
        }

        public static implicit operator Color(SerializableColor c)
        {
            if (c == null) return default;
            return new Color(c.R, c.G, c.B, c.A);
        }
    }
}