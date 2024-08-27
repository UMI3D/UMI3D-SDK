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

namespace inetum.unityUtils.debug
{
    public class GizmosDrawer 
    {
        /// <summary>
        /// Draws a wire arc.<br/>
        /// <br/>
        /// The method should be called in <see cref="MonoBehaviour"/>'s OnDrawGizmos or OnDrawGizmosSelected.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction">The direction from which the anglesRange is taken into account</param>
        /// <param name="plane">The plane in which the wire arc will be drawn.</param>
        /// <param name="anglePct">Percentage of a cercle [0,1]</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="color">The color of the wire arc.</param>
        /// <param name="maxSteps">How many steps to use to draw the arc.</param>
        public static void DrawWireArc(
            Vector3 position,
            Vector3 direction,
            Vector3 plane,
            float anglePct,
            float radius,
            Color? color = null,
            float maxSteps = 20
        )
        {
            if (color == null)
            {
                color = Gizmos.color;
            }

            float anglesRange = 360f * anglePct;

            // Get the angles from the position and direction.
            Vector3 forwardLimitPos = position + direction;
            float srcAngles = Mathf.Rad2Deg * Mathf.Atan2(forwardLimitPos.z - position.z, forwardLimitPos.x - position.x);

            // Center of the arc.
            Vector3 center = position;
            // Draw line from posA.
            Vector3 posA = center;
            // Draw line to posB.
            Vector3 posB = center;

            float stepAngles = anglesRange / maxSteps;
            float angle = srcAngles - anglesRange / 2;
            for (int i = 0; i <= maxSteps; i++)
            {
                float rad = Mathf.Deg2Rad * angle;

                posB = center + new Vector3(
                    plane.x * radius * Mathf.Cos(rad),
                    plane.y * radius * Mathf.Cos(rad),
                    plane.z * radius * Mathf.Sin(rad)
                );

                Debug.DrawLine(posA, posB, color.Value);

                angle += stepAngles;
                posA = posB;
            }
            Debug.DrawLine(posA, center, color.Value);
        }

        /// <summary>
        /// Draws a wireframe sphere with center and radius.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void DrawWireSphere(Vector3 center, float radius, Color? color = null)
        {
            Color currentColor = Gizmos.color;
            Gizmos.color = color.HasValue ? color.Value : Gizmos.color;

            Gizmos.DrawWireSphere(center, radius);

            Gizmos.color = currentColor;
        }
    }
}