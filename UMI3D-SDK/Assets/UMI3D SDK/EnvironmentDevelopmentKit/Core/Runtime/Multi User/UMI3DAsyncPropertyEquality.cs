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

using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Collection of Equality functions for floats and structs containing float, using an epsilon threshold.
    /// </summary>
    public class UMI3DAsyncPropertyEquality
    {
        /// <summary>
        /// Epsilon threshold used for Equality test.
        /// A == B is true if A in ]B - epsilon; B + epsilon[.
        /// </summary>
        /// Default value for epsilon is 10E-6.
        public float epsilon = 0.000001f;

        /// <summary>
        /// Vector Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool Vector3Equality(Vector3 a, Vector3 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y) && InRange(a.z - b.z);
        }

        /// <summary>
        /// Vector Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool Vector2Equality(Vector2 a, Vector2 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y);
        }

        /// <summary>
        /// Vector Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool Vector4Equality(Vector4 a, Vector4 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y) && InRange(a.z - b.z) && InRange(a.w - b.w);
        }

        /// <summary>
        /// Color Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool ColorEquality(Color a, Color b)
        {
            return InRange(a.a - b.a) && InRange(a.r - b.r) && InRange(a.b - b.b) && InRange(a.g - b.g);
        }

        /// <summary>
        /// Quaternion Equality test by angle using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if the angle between the two quaternion is small enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool QuaternionEquality(Quaternion a, Quaternion b)
        {
            return InRange(Quaternion.Angle(a, b));
        }

        /// <summary>
        /// Float Equality test using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if the difference between the two float is small enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool FloatEquality(float a, float b)
        {
            return InRange(a - b);
        }

        /// <summary>
        /// Check if a float is in epsilon range
        /// </summary>
        /// <param name="d"></param>
        /// <returns>return true if <paramref name="d"/> is in ]-epsilon,epsilon[</returns>
        private bool InRange(float d)
        {
            return d < epsilon && d > -epsilon;
        }
    }
}
