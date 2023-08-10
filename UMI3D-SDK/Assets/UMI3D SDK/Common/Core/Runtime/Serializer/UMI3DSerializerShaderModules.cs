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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.common
{
    public class UMI3DSerializerShaderModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            if (
                typeof(T) == typeof(UMI3DShaderPropertyDto)
                )
                return true;
            return null;
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(UMI3DShaderPropertyDto):
                    var shader = FromByte(container);
                    result = (T)Convert.ChangeType(shader, typeof(T));
                    return true;

            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case UMI3DShaderPropertyDto s:
                    if (s.collectionType != 0)
                    {
                        bytable = UMI3DSerializer.Write(s.collectionType)
                            + UMI3DSerializer.Write(s.size)
                            + UMI3DSerializer.Write(s.type)
                            + UMI3DSerializer.Write(s.value);
                    }
                    else
                    {
                        bytable = UMI3DSerializer.Write(s.type)
                            + UMI3DSerializer.Write(s.value);
                    }
                    return true;
            }

            bytable = null;
            return false;
        }



        /// <summary>
        /// Get the right (collectionType, valueType) couple in the <see cref="UMI3DShaderPropertyType"/> index.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static (byte collection, byte type, int size) GetType(object value)
        {
            (byte collection, byte type, int size) result = (0, 0, 0);
            switch (value)
            {
                case Array array:
                    result.size = array.Length;
                    System.Collections.IEnumerator ea = array.GetEnumerator();
                    if (ea.MoveNext())
                        result = GetType(ea.Current);
                    result.collection = UMI3DShaderPropertyType.Array;
                    break;
                case List<object> l:
                    result.size = l.Count;
                    result = GetType(l.FirstOrDefault());
                    result.collection = UMI3DShaderPropertyType.List;
                    break;
                case bool b:
                    result.type = UMI3DShaderPropertyType.Bool;
                    break;
                case double b:
                    result.type = UMI3DShaderPropertyType.Double;
                    break;
                case float b:
                    result.type = UMI3DShaderPropertyType.Float;
                    break;
                case int b:
                    result.type = UMI3DShaderPropertyType.Int;
                    break;
                case Vector2Dto v:
                case Vector2 b:
                    result.type = UMI3DShaderPropertyType.Vector2;
                    break;
                case Vector3Dto v:
                case Vector3 b:
                    result.type = UMI3DShaderPropertyType.Vector3;
                    break;
                case Quaternion q:
                case Vector4Dto v:
                case Vector4 b:
                    result.type = UMI3DShaderPropertyType.Vector4;
                    break;
                case ColorDto c:
                case Color b:
                    result.type = UMI3DShaderPropertyType.Color;
                    break;
                case TextureDto b:
                    result.type = UMI3DShaderPropertyType.Texture;
                    break;
            }
            return result;
        }


        public static UMI3DShaderPropertyDto Create(object value)
        {
            var result = GetType(value);
            return new UMI3DShaderPropertyDto()
            {
                collectionType = result.type,
                value = value,
                type = result.type,
                size = result.size
            };
        }

        public static UMI3DShaderPropertyDto FromByte(ByteContainer container)
        {
            var value = _FromByte(container);
            var result = GetType(value);
            return new UMI3DShaderPropertyDto()
            {
                collectionType = result.type,
                value = value,
                type = result.type,
                size = result.size
            };
        }

        private static object _FromByte(ByteContainer container)
        {
            byte Type = UMI3DSerializer.Read<byte>(container);
            return _FromByte(container, Type);
        }

        private static object _FromByte(ByteContainer container, byte Type)
        {
            switch (Type)
            {
                case UMI3DShaderPropertyType.Array:
                    {
                        int size = UMI3DSerializer.Read<int>(container);
                        byte contentType = UMI3DSerializer.Read<byte>(container);
                        var result = new List<object>();
                        for (int i = 0; i < size; i++)
                        {
                            result.Add(_FromByte(container, contentType));
                        }
                        return result.ToArray();
                    }
                case UMI3DShaderPropertyType.List:
                    {
                        int size = UMI3DSerializer.Read<int>(container);
                        byte contentType = UMI3DSerializer.Read<byte>(container);
                        var result = new List<object>();
                        for (int i = 0; i < size; i++)
                        {
                            result.Add(_FromByte(container, contentType));
                        }
                        return result;
                    }
                case UMI3DShaderPropertyType.Bool:
                    return UMI3DSerializer.Read<bool>(container);
                case UMI3DShaderPropertyType.Double: return UMI3DSerializer.Read<double>(container);
                case UMI3DShaderPropertyType.Float: return UMI3DSerializer.Read<float>(container);
                case UMI3DShaderPropertyType.Int: return UMI3DSerializer.Read<int>(container);

                case UMI3DShaderPropertyType.Vector2: return UMI3DSerializer.Read<Vector2>(container);
                case UMI3DShaderPropertyType.Vector3: return UMI3DSerializer.Read<Vector3>(container);
                case UMI3DShaderPropertyType.Vector4: return UMI3DSerializer.Read<Vector4>(container);
                case UMI3DShaderPropertyType.Color: return UMI3DSerializer.Read<Color>(container);

                case UMI3DShaderPropertyType.Texture: return UMI3DSerializer.Read<TextureDto>(container);
            }
            return null;
        }
    }
}