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

using System;
using UnityEngine;

namespace umi3d.common
{
    public class UMI3DSerializerVectorModules : UMI3DSerializerModule
    {
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(SerializableVector2):
                    if (container.length >= 2 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        result = (T)Convert.ChangeType(new SerializableVector2(x, y), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector2):
                    if (container.length >= 2 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        result = (T)Convert.ChangeType(new Vector2(x, y), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector3):
                    if (container.length >= 3 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        UMI3DSerializer.TryRead(container, out float z);
                        result = (T)Convert.ChangeType(new SerializableVector3(x, y, z), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector3):
                    if (container.length >= 3 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        UMI3DSerializer.TryRead(container, out float z);
                        result = (T)Convert.ChangeType(new Vector3(x, y, z), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Quaternion):
                    if (container.length >= 4 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        UMI3DSerializer.TryRead(container, out float z);
                        UMI3DSerializer.TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new Quaternion(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableColor):
                    if (container.length >= 4 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        UMI3DSerializer.TryRead(container, out float z);
                        UMI3DSerializer.TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new SerializableColor(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Color):
                    if (container.length >= 4 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        UMI3DSerializer.TryRead(container, out float z);
                        UMI3DSerializer.TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new Color(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector4):
                    if (container.length >= 4 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        UMI3DSerializer.TryRead(container, out float z);
                        UMI3DSerializer.TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new SerializableVector4(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector4):
                    if (container.length >= 4 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out float x);
                        UMI3DSerializer.TryRead(container, out float y);
                        UMI3DSerializer.TryRead(container, out float z);
                        UMI3DSerializer.TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new Vector4(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableMatrix4x4):
                    if (container.length >= 4 * 4 * sizeof(float))
                    {

                        UMI3DSerializer.TryRead(container, out Vector4 c0);
                        UMI3DSerializer.TryRead(container, out Vector4 c1);
                        UMI3DSerializer.TryRead(container, out Vector4 c2);
                        UMI3DSerializer.TryRead(container, out Vector4 c3);

                        result = (T)Convert.ChangeType(new SerializableMatrix4x4(c0, c1, c2, c3), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Matrix4x4):
                    if (container.length >= 4 * 4 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out Vector4 c0);
                        UMI3DSerializer.TryRead(container, out Vector4 c1);
                        UMI3DSerializer.TryRead(container, out Vector4 c2);
                        UMI3DSerializer.TryRead(container, out Vector4 c3);

                        result = (T)Convert.ChangeType(new Matrix4x4(c0, c1, c2, c3), typeof(T));
                        return true;
                    }
                    break;

            }

            result = default(T);
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable)
        {
            if (value == null)
            {
                if (typeof(T) == typeof(SerializableVector2))
                {
                    value = (T)(object)new SerializableVector2();
                }
                else if (typeof(T) == typeof(SerializableVector3))
                {
                    value = (T)(object)new SerializableVector3();
                }
                else if (typeof(T) == typeof(SerializableVector4))
                {
                    value = (T)(object)new SerializableVector4();
                }
                else if (typeof(T) == typeof(SerializableColor))
                {
                    value = (T)(object)new SerializableColor();
                }
                else if (typeof(T) == typeof(SerializableMatrix4x4))
                {
                    value = (T)(object)new SerializableMatrix4x4();
                }
            }

            switch (value)
            {
                case SerializableVector2 v2:
                    bytable = UMI3DSerializer.Write(v2.X);
                    bytable += UMI3DSerializer.Write(v2.Y);
                    return true;
                case Vector2 v2:
                    bytable = UMI3DSerializer.Write(v2.x);
                    bytable += UMI3DSerializer.Write(v2.y);
                    return true;
                case SerializableVector3 v3:
                    bytable = UMI3DSerializer.Write(v3.X);
                    bytable += UMI3DSerializer.Write(v3.Y);
                    bytable += UMI3DSerializer.Write(v3.Z);
                    return true;
                case Vector3 v3:
                    bytable = UMI3DSerializer.Write(v3.x);
                    bytable += UMI3DSerializer.Write(v3.y);
                    bytable += UMI3DSerializer.Write(v3.z);
                    return true;
                case SerializableVector4 v4:
                    bytable = UMI3DSerializer.Write(v4.X);
                    bytable += UMI3DSerializer.Write(v4.Y);
                    bytable += UMI3DSerializer.Write(v4.Z);
                    bytable += UMI3DSerializer.Write(v4.W);
                    return true;
                case Vector4 v4:
                    bytable = UMI3DSerializer.Write(v4.x);
                    bytable += UMI3DSerializer.Write(v4.y);
                    bytable += UMI3DSerializer.Write(v4.z);
                    bytable += UMI3DSerializer.Write(v4.w);
                    return true;
                case Quaternion q:
                    bytable = UMI3DSerializer.Write(q.x);
                    bytable += UMI3DSerializer.Write(q.y);
                    bytable += UMI3DSerializer.Write(q.z);
                    bytable += UMI3DSerializer.Write(q.w);
                    return true;
                case Color q:
                    bytable = UMI3DSerializer.Write(q.r);
                    bytable += UMI3DSerializer.Write(q.g);
                    bytable += UMI3DSerializer.Write(q.b);
                    bytable += UMI3DSerializer.Write(q.a);
                    return true;
                case SerializableColor q:
                    bytable = UMI3DSerializer.Write(q.R);
                    bytable += UMI3DSerializer.Write(q.G);
                    bytable += UMI3DSerializer.Write(q.B);
                    bytable += UMI3DSerializer.Write(q.A);
                    return true;
                case SerializableMatrix4x4 v4:
                    bytable = UMI3DSerializer.Write(v4.c0);
                    bytable += UMI3DSerializer.Write(v4.c1);
                    bytable += UMI3DSerializer.Write(v4.c2);
                    bytable += UMI3DSerializer.Write(v4.c3);
                    return true;
                case Matrix4x4 v4:
                    bytable = UMI3DSerializer.Write((SerializableMatrix4x4)v4);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}