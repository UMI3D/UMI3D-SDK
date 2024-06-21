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

using NUnit.Framework;
using UnityEngine;

namespace TestUtils
{
    public static class AssertUnityStruct
    {
        private const float DEFAULT_PRECISION = 0.005f;

        public static void AreEqual(Vector2 expected, Vector2 actual, float delta = DEFAULT_PRECISION, string message = "")
        {
            Assert.AreEqual(expected.x, actual.x, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate X is not the same.");
            Assert.AreEqual(expected.y, actual.y, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Y is not the same.");
        }

        public static void AreEqual(Vector3 expected, Vector3 actual, float delta = DEFAULT_PRECISION, string message = "")
        {
            Assert.AreEqual(expected.x, actual.x, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate X is not the same.");
            Assert.AreEqual(expected.y, actual.y, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Y is not the same.");
            Assert.AreEqual(expected.z, actual.z, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Z is not the same.");
        }

        public static void AreEqual(Vector4 expected, Vector4 actual, float delta = DEFAULT_PRECISION, string message = "")
        {
            Assert.AreEqual(expected.x, actual.x, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate X is not the same.");
            Assert.AreEqual(expected.y, actual.y, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Y is not the same.");
            Assert.AreEqual(expected.z, actual.z, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Z is not the same.");
            Assert.AreEqual(expected.w, actual.w, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Z is not the same.");
        }

        public static void AreEqual(Quaternion expected, Quaternion actual, float delta = DEFAULT_PRECISION, string message = "")
        {
            Assert.AreEqual(expected.x, actual.x, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate X is not the same.");
            Assert.AreEqual(expected.y, actual.y, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Y is not the same.");
            Assert.AreEqual(expected.z, actual.z, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Z is not the same.");
            Assert.AreEqual(expected.w, actual.w, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Z is not the same.");
        }

        /// <summary>
        /// Check on rotation meaning not on actual values.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        public static void AreRotationsEqual(Quaternion expected, Quaternion actual, float delta = DEFAULT_PRECISION, string message = "")
        {
            Vector3 expectedForward = expected * Vector3.forward;
            Vector3 expectedUpward = expected * Vector3.up;

            Vector3 actualForward = actual * Vector3.forward;
            Vector3 actualUpward = actual * Vector3.up;

            AreEqual(expectedForward, actualForward, message: $"Rotations {expected.eulerAngles} and {actual.eulerAngles} are not equal. Forward vectors differs.\n{message}");
            AreEqual(expectedUpward, actualUpward, message: $"Rotations {expected.eulerAngles} and {actual.eulerAngles} are not equal. Upward vectors differs.\n{message}");

            Assert.AreEqual(expected.x, actual.x, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate X is not the same.");
            Assert.AreEqual(expected.y, actual.y, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Y is not the same.");
            Assert.AreEqual(expected.z, actual.z, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Z is not the same.");
            Assert.AreEqual(expected.w, actual.w, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate Z is not the same.");
        }

        public static void AreEqual(Matrix4x4 expected, Matrix4x4 actual, float delta = DEFAULT_PRECISION, string message = "")
        {
            Assert.AreEqual(expected.m00, actual.m00, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m00 is not the same.");
            Assert.AreEqual(expected.m01, actual.m01, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m01 is not the same.");
            Assert.AreEqual(expected.m02, actual.m02, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m02 is not the same.");
            Assert.AreEqual(expected.m03, actual.m03, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m03 is not the same.");

            Assert.AreEqual(expected.m10, actual.m10, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m10 is not the same.");
            Assert.AreEqual(expected.m11, actual.m11, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m11 is not the same.");
            Assert.AreEqual(expected.m12, actual.m12, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m12 is not the same.");
            Assert.AreEqual(expected.m13, actual.m13, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m13 is not the same.");

            Assert.AreEqual(expected.m20, actual.m20, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m20 is not the same.");
            Assert.AreEqual(expected.m21, actual.m21, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m21 is not the same.");
            Assert.AreEqual(expected.m22, actual.m22, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m22 is not the same.");
            Assert.AreEqual(expected.m23, actual.m23, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m23 is not the same.");

            Assert.AreEqual(expected.m30, actual.m30, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m30 is not the same.");
            Assert.AreEqual(expected.m31, actual.m31, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m31 is not the same.");
            Assert.AreEqual(expected.m32, actual.m32, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m32 is not the same.");
            Assert.AreEqual(expected.m33, actual.m33, delta, $"{message}\nExpected: {expected}, Actual: {actual}.\n| Coordinate m33 is not the same.");
        }
    }
}
