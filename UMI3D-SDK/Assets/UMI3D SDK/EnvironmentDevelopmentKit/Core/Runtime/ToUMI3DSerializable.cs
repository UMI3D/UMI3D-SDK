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

using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Helper class to serialize common Unity structs.
    /// </summary>
    public static class ToUMI3DSerializable
    {
        public static ColorDto ToSerializableColor(Color color, UMI3DUser user) { return color.Dto(); }
        public static Vector2Dto ToSerializableVector2(Vector2 vector, UMI3DUser user) { return vector.Dto(); }
        public static Vector3Dto ToSerializableVector3(Vector3 vector, UMI3DUser user) { return vector; }
        public static Vector4Dto ToSerializableVector4(Vector4 vector, UMI3DUser user) { return vector; }
        public static Vector4Dto ToSerializableVector4(Quaternion quaternion, UMI3DUser user) { return quaternion; }
        public static Matrix4x4Dto ToSerializableMatrix4x4(Matrix4x4 matrix, UMI3DUser user) { return matrix; }
    }
}