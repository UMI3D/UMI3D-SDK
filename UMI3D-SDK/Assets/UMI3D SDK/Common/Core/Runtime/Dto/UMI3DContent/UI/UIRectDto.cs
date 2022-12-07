﻿/*
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

using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// DTO describing a rectangular component for UI.
    /// </summary>
    /// For unity see <see cref="Rect"/> and <seealso cref="UnityEngine.UI.RectMask2D"/>.
    [System.Serializable]
    public class UIRectDto : UMI3DNodeDto
    {
        /// <summary>
        /// Relative position of the pivot point of the rectangle to the anchors.
        /// </summary>
        public SerializableVector2 anchoredPosition;

        /// <summary>
        /// Relative position of the pivot point of the rectangle to the anchors in 3D.
        /// </summary>
        public SerializableVector3 anchoredPosition3D;

        /// <summary>
        /// The upper right corner's anchor point as a fraction relative to 
        /// the size of the parent rectangle. 
        /// </summary>
        /// (0,0) corresponds to an anchor at the lower left corner of the parent,
        /// while (1,1) corresponds to an anchor at the parent's upper right corner.
        public SerializableVector2 anchorMax;

        /// <summary>
        /// The lower left corner's anchor point as a fraction relative to 
        /// the size of the parent rectangle. 
        /// </summary>
        /// (0,0) corresponds to an anchor at the e lower left corner of the parent, 
        /// while (1,1) corresponds to an anchor at the parent's upper right corne.
        public SerializableVector2 anchorMin;

        /// <summary>
        /// Offset of the upper right corner relative to the upper right anchor.
        /// </summary>
        public SerializableVector2 offsetMax;

        /// <summary>
        /// Offset of the lower left corner relative to the lower left anchor.
        /// </summary>
        public SerializableVector2 offsetMin;

        /// <summary>
        /// Position of the pivot point as a fraction relative to 
        /// the size of the rectangle.
        /// </summary>
        /// (0,0) corresponds to the lower left corner 
        /// while (1,1) corresponds to the upper right corner.
        public SerializableVector2 pivot;

        /// <summary>
        /// Size of the rectangle relative to the distance between the two anchors.
        /// </summary>
        public SerializableVector2 sizeDelta;

        /// <summary>
        /// Should the rectangle be considered as a mask?
        /// </summary>
        public bool rectMask;

        public UIRectDto() : base() { }
    }
}