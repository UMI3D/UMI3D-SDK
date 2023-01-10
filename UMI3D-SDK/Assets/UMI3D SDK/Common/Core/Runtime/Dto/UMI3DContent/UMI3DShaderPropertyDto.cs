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
    /// <summary>
    /// Representation of a shader's property.
    /// </summary>
    /// Shader property are of unkown types, thus types are identified in <see cref="UMI3DShaderPropertyType"/>.
    [System.Serializable]
    public class UMI3DShaderPropertyDto : UMI3DDto, IBytable
    {
        /// <summary>
        /// Shader property collection type in <see cref="UMI3DShaderPropertyType"/> if the property is a collection.
        /// </summary>
        private readonly byte CollectionType;
        /// <summary>
        /// Size of the colleciton if the property is a collection.
        /// </summary>
        private int size;
        /// <summary>
        /// Shader property type in <see cref="UMI3DShaderPropertyType"/>.
        /// </summary>
        private readonly byte Type;
        /// <summary>
        /// Shader property value.
        /// </summary>
        public object value;

        public UMI3DShaderPropertyDto(object value)
        {
            this.value = value;
            (this.CollectionType, this.Type) = GetType(value);
        }

        /// <inheritdoc/>
        public bool IsCountable()
        {
            return true;
        }

        /// <inheritdoc/>
        public Bytable ToBytableArray(params object[] parameters)
        {
            if (CollectionType != 0)
            {
                return UMI3DSerializer.Write(CollectionType)
                    + UMI3DSerializer.Write(size)
                    + UMI3DSerializer.Write(Type)
                    + UMI3DSerializer.Write(value);
            }
            else
            {
                return UMI3DSerializer.Write(Type)
                    + UMI3DSerializer.Write(value);
            }
        }


        public static UMI3DShaderPropertyDto FromByte(ByteContainer container)
        {
            return new UMI3DShaderPropertyDto(_FromByte(container));
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

        /// <summary>
        /// Get the right (collectionType, valueType) couple in the <see cref="UMI3DShaderPropertyType"/> index.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private (byte, byte) GetType(object value)
        {
            (byte, byte) result = (0, 0);
            switch (value)
            {
                case Array array:
                    size = array.Length;
                    System.Collections.IEnumerator ea = array.GetEnumerator();
                    if (ea.MoveNext())
                        result = GetType(ea.Current);
                    result.Item1 = UMI3DShaderPropertyType.Array;
                    break;
                case List<object> l:
                    size = l.Count;
                    result = GetType(l.FirstOrDefault());
                    result.Item1 = UMI3DShaderPropertyType.List;
                    break;
                case bool b:
                    result.Item2 = UMI3DShaderPropertyType.Bool;
                    break;
                case double b:
                    result.Item2 = UMI3DShaderPropertyType.Double;
                    break;
                case float b:
                    result.Item2 = UMI3DShaderPropertyType.Float;
                    break;
                case int b:
                    result.Item2 = UMI3DShaderPropertyType.Int;
                    break;
                case SerializableVector2 v:
                case Vector2 b:
                    result.Item2 = UMI3DShaderPropertyType.Vector2;
                    break;
                case SerializableVector3 v:
                case Vector3 b:
                    result.Item2 = UMI3DShaderPropertyType.Vector3;
                    break;
                case Quaternion q:
                case SerializableVector4 v:
                case Vector4 b:
                    result.Item2 = UMI3DShaderPropertyType.Vector4;
                    break;
                case SerializableColor c:
                case Color b:
                    result.Item2 = UMI3DShaderPropertyType.Color;
                    break;
                case TextureDto b:
                    result.Item2 = UMI3DShaderPropertyType.Texture;
                    break;
            }
            return result;
        }
    }
}