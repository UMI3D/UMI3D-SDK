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
        public static SerializableVector2 Unscaled(this SerializableVector2 v2, SerializableVector2 scale)
        {
            return (SerializableVector2)((Vector2)v2).Unscaled(scale);
        }

        public static SerializableVector2 Scaled(this SerializableVector2 v2, SerializableVector2 scale)
        {
            return (SerializableVector2)((Vector2)v2).Scaled(scale);
        }

        public static SerializableVector3 Unscaled(this SerializableVector3 v3, SerializableVector3 scale)
        {
            return (SerializableVector3)((Vector3)v3).Unscaled(scale);
        }

        public static SerializableVector3 Scaled(this SerializableVector3 v3, SerializableVector3 scale)
        {
            return (SerializableVector3)((Vector3)v3).Scaled(scale);
        }

        public static SerializableVector4 Unscaled(this SerializableVector4 v4, SerializableVector4 scale)
        {
            return (SerializableVector4)((Vector4)v4).Unscaled(scale);
        }

        public static SerializableVector4 Scaled(this SerializableVector4 v4, SerializableVector4 scale)
        {
            return (SerializableVector4)((Vector4)v4).Scaled(scale);
        }
    }
}