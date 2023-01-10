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
using System.Collections;
using umi3d.common;

namespace umi3d.common
{
    public class UMI3DSerializerBasicModules : UMI3DSerializerModule
    {
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(char):
                    if (container.length >= sizeof(char))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToChar(container.bytes, container.position), typeof(T));
                        container.position += sizeof(char);
                        container.length -= sizeof(char);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(bool):
                    if (container.length >= sizeof(bool))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToBoolean(container.bytes, container.position), typeof(T));
                        container.position += sizeof(bool);
                        container.length -= sizeof(bool);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(byte):
                    if (container.length >= sizeof(byte))
                    {
                        result = (T)Convert.ChangeType(container.bytes[container.position], typeof(T));
                        container.position += 1;
                        container.length -= 1;
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(short):
                    if (container.length >= sizeof(short))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToInt16(container.bytes, container.position), typeof(T));
                        container.position += sizeof(short);
                        container.length -= sizeof(short);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(ushort):
                    if (container.length >= sizeof(ushort))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToUInt16(container.bytes, container.position), typeof(T));
                        container.position += sizeof(ushort);
                        container.length -= sizeof(ushort);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(int):
                    if (container.length >= sizeof(int))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToInt32(container.bytes, container.position), typeof(T));
                        container.position += sizeof(int);
                        container.length -= sizeof(int);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(uint):
                    if (container.length >= sizeof(uint))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToUInt32(container.bytes, container.position), typeof(T));
                        container.position += sizeof(uint);
                        container.length -= sizeof(uint);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(float):
                    if (container.length >= sizeof(float))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToSingle(container.bytes, container.position), typeof(T));
                        container.position += sizeof(float);
                        container.length -= sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(long):
                    if (container.length >= sizeof(long))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToInt64(container.bytes, container.position), typeof(T));
                        container.position += sizeof(long);
                        container.length -= sizeof(long);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(ulong):
                    if (container.length >= sizeof(ulong))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToUInt64(container.bytes, container.position), typeof(T));
                        container.position += sizeof(ulong);
                        container.length -= sizeof(ulong);
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
            Func<byte[], int, int, (int, int)> f;

            switch (value)
            {
                case char c:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(c).CopyTo(by, i);
                        int s = sizeof(char);
                        return (i + s, bs + s);
                    };

                    bytable = new Bytable(sizeof(char), f);
                    return true;
                case bool b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(bool);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(bool), f);
                    return true;
                case byte b:
                    f = (by, i, bs) =>
                    {
                        by[i] = (b);
                        return (i + 1, bs + 1);
                    };
                    bytable = new Bytable(sizeof(byte), f);
                    return true;
                case short b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(short);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(short), f);
                    return true;
                case ushort b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(ushort);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(ushort), f);
                    return true;
                case int b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(int);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(int), f);
                    return true;
                case uint b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(uint);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(uint), f);
                    return true;
                case float b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(float);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(float), f);
                    return true;
                case long b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(long);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(long), f);
                    return true;
                case ulong b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(ulong);
                        return (i + s, bs + s);
                    };
                    bytable = new Bytable(sizeof(ulong), f);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}