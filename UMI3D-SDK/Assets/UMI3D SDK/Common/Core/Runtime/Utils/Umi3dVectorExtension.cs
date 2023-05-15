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

using inetum.unityUtils;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Helper serializer for vectors.
    /// </summary>
    [System.Obsolete]
    public static class Umi3dVectorExtension
    {
        public static Vector2Dto Unscaled(this Vector2Dto v2, Vector2Dto scale)
        {
            return ((v2.Struct()).Unscaled(scale.Struct())).Dto();
        }

        public static Vector2Dto Scaled(this Vector2Dto v2, Vector2Dto scale)
        {
            return (v2.Struct()).Scaled(scale.Struct()).Dto();
        }

        public static Vector3Dto Unscaled(this Vector3Dto v3, Vector3Dto scale)
        {
            return (v3.Struct()).Unscaled(scale.Struct()).Dto();
        }

        public static Vector3Dto Scaled(this Vector3Dto v3, Vector3Dto scale)
        {
            return (v3.Struct()).Scaled(scale.Struct()).Dto();
        }

        public static Vector4Dto Unscaled(this Vector4Dto v4, Vector4Dto scale)
        {
            return ((v4.Struct()).Unscaled(scale.Struct())).Dto();
        }

        public static Vector4Dto Scaled(this Vector4Dto v4, Vector4Dto scale)
        {
            return ((v4.Struct()).Scaled(scale.Struct()).Dto());
        }
    }
}