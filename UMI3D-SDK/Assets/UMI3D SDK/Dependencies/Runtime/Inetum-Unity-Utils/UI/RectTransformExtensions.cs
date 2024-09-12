/*
Copyright 2019 - 2024 Inetum

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
using UnityEngine.EventSystems;

namespace inetum.unityUtils.ui
{
    public static class RectTransformExtensions 
    {
        public enum Pivot
        {
            TopLeft, Top, TopRight,
            Left, Center, Right,
            BottomLeft, Bottom, BottomRight,
        }

        /// <summary>
        /// Return the pointer position relative to the <paramref name="pivot"/> of the UI object.<br/>
        /// <br/>
        /// The direction is bottom to up and left to right.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="eventData"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        public static Vector2 PointerRelativeToUI(this RectTransform rectTransform, PointerEventData eventData, Pivot pivot = Pivot.Center)
        {
            Vector3 offset;
            switch (pivot)
            {
                case Pivot.TopLeft:
                    offset = new(
                        rectTransform.rect.width / 2, 
                        -rectTransform.rect.height / 2
                    );
                    break;
                case Pivot.Top:
                    offset = new(
                        0f, 
                        -rectTransform.rect.height / 2
                    );
                    break;
                case Pivot.TopRight:
                    offset = new(
                        -rectTransform.rect.width / 2, 
                        -rectTransform.rect.height / 2
                    );
                    break;
                case Pivot.Left:
                    offset = new(
                        rectTransform.rect.width / 2, 
                        0f
                    );
                    break;
                case Pivot.Center:
                    offset = Vector3.zero;
                    break;
                case Pivot.Right:
                    offset = new(
                        -rectTransform.rect.width / 2, 
                        0f
                    );
                    break;
                case Pivot.BottomLeft:
                    offset = new(
                        rectTransform.rect.width / 2, 
                        rectTransform.rect.height / 2
                    );
                    break;
                case Pivot.Bottom:
                    offset = new(
                        0f, 
                        rectTransform.rect.height / 2
                    );
                    break;
                case Pivot.BottomRight:
                    offset = new(
                        -rectTransform.rect.width / 2, 
                        rectTransform.rect.height / 2
                    );
                    break;
                default:
                    offset = Vector3.zero;
                    break;
            }

            return rectTransform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition) + offset;
        }
    }
}