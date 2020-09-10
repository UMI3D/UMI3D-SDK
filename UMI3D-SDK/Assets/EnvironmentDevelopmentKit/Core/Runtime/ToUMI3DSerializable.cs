/*
Copyright 2019 Gfi Informatique

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
    public static class ToUMI3DSerializable
    {
        static public SerializableColor ToSerializableColor(Color color, UMI3DUser user) { return color; }
        static public SerializableVector2 ToSerializableVector2(Vector2 vector, UMI3DUser user) { return vector; }
        static public SerializableVector3 ToSerializableVector3(Vector3 vector, UMI3DUser user) { return vector; }
        static public SerializableVector4 ToSerializableVector4(Vector4 vector, UMI3DUser user) { return vector; }
        static public SerializableVector4 ToSerializableVector4(Quaternion quaternion, UMI3DUser user) { return quaternion; }
        static public SerializableMatrix4x4 ToSerializableMatrix4x4(Matrix4x4 matrix, UMI3DUser user) { return matrix; }
    }
}