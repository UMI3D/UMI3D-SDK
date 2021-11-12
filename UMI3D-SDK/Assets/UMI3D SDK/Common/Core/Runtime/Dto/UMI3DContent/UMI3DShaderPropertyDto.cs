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
    [System.Serializable]
    public class UMI3DShaderPropertyDto : UMI3DDto, IBytable
    {
        private byte CollectionType;
        private int size;
        private byte Type;
        public object value;

        public UMI3DShaderPropertyDto(object value)
        {
            this.value = value;
            (this.CollectionType, this.Type) = GetType(value);
        }

        public bool IsCountable()
        {
            return true;
        }

        public Bytable ToBytableArray(params object[] parameters)
        {
            if (CollectionType != 0)
            {
                return UMI3DNetworkingHelper.Write(CollectionType)
                    + UMI3DNetworkingHelper.Write(size)
                    + UMI3DNetworkingHelper.Write(Type)
                    + UMI3DNetworkingHelper.Write(value);
            }
            else
            {
                return UMI3DNetworkingHelper.Write(Type)
                    + UMI3DNetworkingHelper.Write(value);
            }
        }


        public static UMI3DShaderPropertyDto FromByte(ByteContainer container)
        {
            return new UMI3DShaderPropertyDto(_FromByte(container));
        }

        private static object _FromByte(ByteContainer container)
        {
            byte Type = UMI3DNetworkingHelper.Read<byte>(container);
            return _FromByte(container, Type);
        }

        private static object _FromByte(ByteContainer container, byte Type)
        {
            switch (Type)
            {
                case UMI3DShaderPropertyType.Array:
                    {
                        int size = UMI3DNetworkingHelper.Read<int>(container);
                        byte contentType = UMI3DNetworkingHelper.Read<byte>(container);
                        var result = new List<object>();
                        for (int i = 0; i < size; i++)
                        {
                            result.Add(_FromByte(container, contentType));
                        }
                        return result.ToArray();
                    }
                case UMI3DShaderPropertyType.List:
                    {
                        int size = UMI3DNetworkingHelper.Read<int>(container);
                        byte contentType = UMI3DNetworkingHelper.Read<byte>(container);
                        var result = new List<object>();
                        for (int i = 0; i < size; i++)
                        {
                            result.Add(_FromByte(container, contentType));
                        }
                        return result;
                    }
                case UMI3DShaderPropertyType.Bool:
                    return UMI3DNetworkingHelper.Read<bool>(container);
                case UMI3DShaderPropertyType.Double: return UMI3DNetworkingHelper.Read<double>(container);
                case UMI3DShaderPropertyType.Float: return UMI3DNetworkingHelper.Read<float>(container);
                case UMI3DShaderPropertyType.Int: return UMI3DNetworkingHelper.Read<int>(container);

                case UMI3DShaderPropertyType.Vector2: return UMI3DNetworkingHelper.Read<Vector2>(container);
                case UMI3DShaderPropertyType.Vector3: return UMI3DNetworkingHelper.Read<Vector3>(container);
                case UMI3DShaderPropertyType.Vector4: return UMI3DNetworkingHelper.Read<Vector4>(container);
                case UMI3DShaderPropertyType.Color: return UMI3DNetworkingHelper.Read<Color>(container);

                case UMI3DShaderPropertyType.Texture: return UMI3DNetworkingHelper.Read<TextureDto>(container);
            }
            return null;
        }

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