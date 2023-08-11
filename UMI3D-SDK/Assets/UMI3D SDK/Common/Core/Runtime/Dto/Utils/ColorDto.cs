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

namespace umi3d.common
{
    /// <summary>
    /// Serializable implementation of a color using RGBA channels with float values.
    /// </summary>
    [Serializable]
    public class ColorDto : UMI3DDto
    {
        /// <summary>
        /// Red color channel. Value between 0 and 1.
        /// </summary>
        public float R { get; set; }

        /// <summary>
        /// Green color channel. Value between 0 and 1.
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// Blue color channel. Value between 0 and 1.
        /// </summary>
        public float B { get; set; }

        /// <summary>
        /// Alpha opacity channel. Value between 0 and 1.
        /// </summary>
        public float A { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}[{R},{G},{B},{A}]";
        }

    }
}