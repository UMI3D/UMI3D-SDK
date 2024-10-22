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

namespace inetum.unityUtils.math
{
    public static class TransformUtils 
    {
        /// <summary>
        /// Translate <paramref name="parent"/> so that <paramref name="child"/> ends up at <paramref name="worldPosition"/>.<br/>
        /// <br/>
        /// <remarks>WARNING: <paramref name="parent"/> has to be a parent but not necessary the parent of <paramref name="child"/>.</remarks>
        /// </summary>
        /// <param name="parent">The object to move.</param>
        /// <param name="child">The object to center.</param>
        /// <param name="worldPosition">The world position where child will end up.</param>
        /// <param name="filter"></param>
        public static void TranslateParentToCenterChild(
            this Transform parent, 
            Transform child, 
            Vector3 worldPosition, 
            Vector3? filter = null
        )
        {
            // Get the local position of 'child' with respect to 'parent'.
            Vector3 localPosition_child = parent.GetRelativeTranslationOfAToB(child);

            Vector3 delta = worldPosition - parent.position;

            if (filter.HasValue)
            {
                localPosition_child = Vector3.Scale(localPosition_child, filter.Value);

                delta = Vector3.Scale(delta, filter.Value);
            }

            // Move 'parent' so that the world position of 'child' is equal to 'worldPosition'.
            parent.localPosition -= delta + localPosition_child;
        }

        /// <summary>
        /// Rotate <paramref name="parent"/> so that <paramref name="child"/> ends up with <paramref name="worldRotation"/>.<br/>
        /// <br/>
        /// <remarks>WARNING: <paramref name="parent"/> has to be a parent but not necessary the parent of <paramref name="child"/>.</remarks>
        /// </summary>
        /// <param name="parent">The object to move.</param>
        /// <param name="child">The object to center.</param>
        /// <param name="worldRotation">The world rotation where child will end up.</param>
        /// <param name="filter"></param>
        public static void RotateParentToCenterChild(
            this Transform parent, 
            Transform child,
            Quaternion worldRotation,
            Vector3? filter = null
        )
        {
            // Get the local rotation of 'child' with respect to 'parent'.
            Quaternion localRotation_child = parent.GetRelativeRotationOfAToB(child);

            Quaternion delta = Quaternion.Inverse(parent.rotation) * worldRotation;

            if (filter.HasValue)
            {
                Vector3 euler_otc = localRotation_child.eulerAngles;
                localRotation_child.eulerAngles = Vector3.Scale(euler_otc, filter.Value);

                Vector3 euler_delta = delta.eulerAngles;
                delta.eulerAngles = Vector3.Scale(euler_delta, filter.Value);
            }

            // Rotate 'parent' so that the world rotation of the 'child' is equal to 'worldRotation'.
            parent.localRotation *= delta * Quaternion.Inverse(localRotation_child);
        }

        /// <summary>
        /// Translate and rotate <paramref name="parent"/> so that <paramref name="child"/> is center in position and rotation compared to <paramref name="parent"/>'s parent.
        /// </summary>
        /// <param name="parent">The object to move.</param>
        /// <param name="child">The object to center.</param>
        /// <param name="worldPosition">The world position where child will end up.</param>
        /// <param name="worldRotation">The world rotation where child will end up.</param>
        /// <param name="translationFilter"></param>
        /// <param name="rotationFilter"></param>
        public static void TranslateAndRotateParentToCenterChild(
            this Transform parent, 
            Transform child, 
            Vector3 worldPosition, 
            Quaternion worldRotation, 
            Vector3? translationFilter = null, 
            Vector3? rotationFilter = null
        )
        {
            // === Rotate the offsetToMove. ===
            parent.RotateParentToCenterChild(child, worldRotation, rotationFilter);

            // === Move the offsetToMove. ===
            parent.TranslateParentToCenterChild(child, worldPosition, translationFilter);
        }
    }
}