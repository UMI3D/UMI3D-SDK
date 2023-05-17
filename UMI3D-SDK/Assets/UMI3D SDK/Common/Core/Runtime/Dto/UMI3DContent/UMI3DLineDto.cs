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
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// DTO to describe a rendered line.
    /// </summary>
    [System.Serializable]
    public class UMI3DLineDto : UMI3DRenderedNodeDto
    {
        /// <summary>
        /// Color of the line first point.
        /// </summary>
        public ColorDto startColor { get; set; } = Color.white.Dto();

        /// <summary>
        /// Color of the line last point.
        /// </summary>
        public ColorDto endColor { get; set; } = Color.white.Dto();

        /// <summary>
        /// If true, a line will be draw between the first and the last point.
        /// </summary>
        public bool loop { get; set; } = false;

        /// <summary>
        /// Draw line in world space
        /// </summary>
        public bool useWorldSpace { get; set; } = false;

        /// <summary>
        /// line width on first point
        /// </summary>
        public float startWidth { get; set; } = 0.01f;

        /// <summary>
        /// line width on last point
        /// </summary>
        public float endWidth { get; set; } = 0.01f;

        /// <summary>
        /// The positions of points on the line
        /// </summary>
        public List<Vector3Dto> positions { get; set; } = new List<Vector3Dto>();
    }
}
