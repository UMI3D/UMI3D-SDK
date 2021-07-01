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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.common
{
    public static class UMI3DNetworkingHelper
    {

        static List<Umi3dNetworkingHelperModule> modules = new List<Umi3dNetworkingHelperModule>();

        /// <summary>
        /// Add a networking module.
        /// </summary>
        /// <param name="module"></param>
        public static void AddModule(Umi3dNetworkingHelperModule module)
        {
            modules.Add(module);
        }

        /// <summary>
        /// Remove a networking module
        /// </summary>
        /// <param name="module"></param>
        public static void RemoveModule(Umi3dNetworkingHelperModule module)
        {
            modules.Remove(module);
        }

        /// <summary>
        /// Add a list of module
        /// </summary>
        /// <param name="moduleList"></param>
        public static void AddModule(List<Umi3dNetworkingHelperModule> moduleList)
        {
            foreach (var module in moduleList)
                modules.Add(module);
        }

        /// <summary>
        /// Remove a list of module
        /// </summary>
        /// <param name="moduleList"></param>
        public static void RemoveModule(List<Umi3dNetworkingHelperModule> moduleList)
        {
            foreach (var module in moduleList)
                modules.Remove(module);
        }

        /// <summary>
        /// Read a value from a ByteContainer and update it
        /// </summary>
        /// <typeparam name="T">Type to read</typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static T Read<T>(ByteContainer container)
        {
            T result;
            TryRead<T>(container, out result);
            return result;
        }

        /// <summary>
        /// Try to read a value from a ByteContainer and update it.
        /// </summary>
        /// <typeparam name="T">Type to read</typeparam>
        /// <param name="container"></param>
        /// <param name="result">result if readable</param>
        /// <returns>state if the value is readable from this byte container</returns>
        public static bool TryRead<T>(ByteContainer container, out T result)
        {
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
                case true when typeof(T) == typeof(SerializableVector2):
                    if (container.length >= 2 * sizeof(float))
                    {
                        float x, y;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        result = (T)Convert.ChangeType(new SerializableVector2(x, y), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector2):
                    if (container.length >= 2 * sizeof(float))
                    {
                        float x, y;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        result = (T)Convert.ChangeType(new Vector2(x, y), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector3):
                    if (container.length >= 3 * sizeof(float))
                    {
                        float x, y, z;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        TryRead(container, out z);
                        result = (T)Convert.ChangeType(new SerializableVector3(x, y, z), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector3):
                    if (container.length >= 3 * sizeof(float))
                    {
                        float x, y, z;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        TryRead(container, out z);
                        result = (T)Convert.ChangeType(new Vector3(x, y, z), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Quaternion):
                    if (container.length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        TryRead(container, out z);
                        TryRead(container, out w);
                        result = (T)Convert.ChangeType(new Quaternion(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableColor):
                    if (container.length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        TryRead(container, out z);
                        TryRead(container, out w);
                        result = (T)Convert.ChangeType(new SerializableColor(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Color):
                    if (container.length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        TryRead(container, out z);
                        TryRead(container, out w);
                        result = (T)Convert.ChangeType(new Color(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector4):
                    if (container.length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        TryRead(container, out z);
                        TryRead(container, out w);
                        result = (T)Convert.ChangeType(new SerializableVector4(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector4):
                    if (container.length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        TryRead(container, out x);
                        TryRead(container, out y);
                        TryRead(container, out z);
                        TryRead(container, out w);
                        result = (T)Convert.ChangeType(new Vector4(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableMatrix4x4):
                    if (container.length >= 4 * 4 * sizeof(float))
                    {
                        Vector4 c0, c1, c2, c3;

                        TryRead(container, out c0);
                        TryRead(container, out c1);
                        TryRead(container, out c2);
                        TryRead(container, out c3);

                        result = (T)Convert.ChangeType(new SerializableMatrix4x4(c0, c1, c2, c3), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(UMI3DShaderPropertyDto):
                    var shader = UMI3DShaderPropertyDto.FromByte(container);
                    result = (T)Convert.ChangeType(shader, typeof(T));
                    return true;
                case true when typeof(T) == typeof(string):
                    result = default(T);
                    if (container.length == 0) return false;
                    uint s;
                    string r = "";
                    if (TryRead<uint>(container, out s))
                    {
                        for (uint i = 0; i < s; i++)
                        {
                            char c;
                            if (TryRead<char>(container, out c))
                            {
                                r += c;
                            }
                            else return false;
                        }
                    }
                    else return false;
                    result = (T)Convert.ChangeType(r, typeof(T));
                    return true;
                default:
                    bool read;
                    foreach (var module in modules)
                        if (module.Read<T>(container, out read, out result))
                            return read;
                    throw new Exception($"Missing case [{typeof(T)} was not catched]");
            }
            result = default(T);
            return false;
        }

        public static T[] ReadArray<T>(ByteContainer container)
        {
            return ReadList<T>(container).ToArray();
        }

        public static List<T> ReadList<T>(ByteContainer container)
        {
            byte listType = UMI3DNetworkingHelper.Read<byte>(container);
            switch (listType)
            {
                case UMI3DObjectKeys.CountArray:
                    return ReadCountList<T>(container);
                case UMI3DObjectKeys.IndexesArray:
                    return ReadIndexesList<T>(container);
                default:
                    throw new Exception($"Not a known collection type {container}");
            }
        }

        public static Dictionary<T,K> ReadDictionary<T,K>(ByteContainer container)
        {
            return ReadList<KeyValuePair<T,K>>(container).ToDictionary();
        }

        static List<T> ReadIndexesList<T>(ByteContainer container)
        {
            var result = new List<T>();
            int indexMaxPos = -1;
            int maxLength = container.bytes.Length;
            int valueIndex = -1;
            for (; container.position < indexMaxPos || indexMaxPos == -1;)
            {
                int nopIndex = UMI3DNetworkingHelper.Read<int>(container);

                if (indexMaxPos == -1)
                {
                    indexMaxPos = valueIndex = nopIndex;
                    continue;
                }
                var SubContainer = new ByteContainer(container.bytes) { position = valueIndex, length = nopIndex - valueIndex };
                T v;
                if (!TryRead(SubContainer, out v)) break;
                result.Add(v);
                valueIndex = nopIndex;
            }
            {
                var SubContainer = new ByteContainer(container.bytes) { position = valueIndex, length = maxLength - valueIndex };
                T v;
                if (TryRead(SubContainer, out v))
                    result.Add(v);
            }
            return result;
        }

        static List<T> ReadCountList<T>(ByteContainer container)
        {
            int count = UMI3DNetworkingHelper.Read<int>(container);
            var res = new List<T>();
            var Length = container.bytes.Length;
            for (int i = 0; container.position < Length && container.length > 0 && i < count; i++)
            {
                T result;
                if (TryRead<T>(container, out result))
                    res.Add(result);
                else
                    break;
            }
            return res;
        }

        static public byte[] ReadByteArray(ByteContainer container)
        {
            byte type = UMI3DNetworkingHelper.Read<byte>(container);
            int count = UMI3DNetworkingHelper.Read<int>(container);
            var res = new byte[count];
            container.bytes.CopyRangeTo(res, 0, container.position, container.position + count - 1);
            return res;
        }

        public static IEnumerable<ByteContainer> ReadIndexesList(ByteContainer container)
        {
            byte listType = UMI3DNetworkingHelper.Read<byte>(container);
            if (listType != UMI3DObjectKeys.IndexesArray)
                yield break;
            int indexMaxPos = -1;
            int maxLength = container.bytes.Length;
            int valueIndex = -1;
            for (; container.position < indexMaxPos || indexMaxPos == -1;)
            {
                int nopIndex = UMI3DNetworkingHelper.Read<int>(container);

                if (indexMaxPos == -1)
                {
                    indexMaxPos = valueIndex = nopIndex;
                    continue;
                }
                var SubContainer = new ByteContainer(container.bytes) { position = valueIndex, length = nopIndex - valueIndex };
                yield return SubContainer;
                valueIndex = nopIndex;
            }
            {
                var SubContainer = new ByteContainer(container.bytes) { position = valueIndex, length = maxLength - valueIndex };
                yield return SubContainer;
            }
            yield break;
        }

        static Bytable GetType<T>(T value)
        {
            switch (value)
            {
                case bool b:
                    return Write(UMI3DObjectKeys.Bool);
                case double b:
                    return Write(UMI3DObjectKeys.Double);
                case float b:
                    return Write(UMI3DObjectKeys.Float);
                case int b:
                    return Write(UMI3DObjectKeys.Int);
                case SerializableVector2 v:
                case Vector2 b:
                    return Write(UMI3DObjectKeys.Vector2);
                case SerializableVector3 v:
                case Vector3 b:
                    return Write(UMI3DObjectKeys.Vector3);
                case Quaternion q:
                case SerializableVector4 v:
                case Vector4 b:
                    return Write(UMI3DObjectKeys.Vector4);
                case Color b:
                    return Write(UMI3DObjectKeys.Color);
                case TextureDto b:
                    return Write(UMI3DObjectKeys.TextureDto);
                default:
                    return new Bytable();
            }
        }

        public static Bytable Write<T>(IEnumerable<T> value)
        {
            return WriteCollection(value);
        }

        public static Bytable Write<T,K>(KeyValuePair<T,K> value)
        {
            return Write(value.Key) + Write(value.Value);
        }

        public static Bytable Write<T>(T value)
        {
            Func<byte[], int, int, (int, int)> f;
            Bytable bc;
            switch (value)
            {
                case IBytable b:
                    return b.ToBytableArray();
                case char c:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(c).CopyTo(by, i);
                        var s = sizeof(char);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(char), f);
                case bool b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(bool);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(bool), f);
                case byte b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(byte);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(byte), f);
                case short b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(short);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(short), f);
                case ushort b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(ushort);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(ushort), f);
                case int b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(int);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(int), f);
                case uint b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(uint);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(uint), f);
                case float b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(float);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(float), f);
                case long b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(long);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(long), f);
                case ulong b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        var s = sizeof(ulong);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(ulong), f);
                case SerializableVector2 v2:
                    bc = Write(v2.X);
                    bc += Write(v2.Y);
                    return bc;
                case Vector2 v2:
                    bc = Write(v2.x);
                    bc += Write(v2.y);
                    return bc;
                case SerializableVector3 v3:
                    bc = Write(v3.X);
                    bc += Write(v3.Y);
                    bc += Write(v3.Z);
                    return bc;
                case Vector3 v3:
                    bc = Write(v3.x);
                    bc += Write(v3.y);
                    bc += Write(v3.z);
                    return bc;
                case SerializableVector4 v4:
                    bc = Write(v4.X);
                    bc += Write(v4.Y);
                    bc += Write(v4.Z);
                    bc += Write(v4.W);
                    return bc;
                case Vector4 v4:
                    bc = Write(v4.x);
                    bc += Write(v4.y);
                    bc += Write(v4.z);
                    bc += Write(v4.w);
                    return bc;
                case Quaternion q:
                    bc = Write(q.x);
                    bc += Write(q.y);
                    bc += Write(q.z);
                    bc += Write(q.w);
                    return bc;
                case Color q:
                    bc = Write(q.r);
                    bc += Write(q.g);
                    bc += Write(q.b);
                    bc += Write(q.a);
                    return bc;
                case SerializableColor q:
                    bc = Write(q.R);
                    bc += Write(q.G);
                    bc += Write(q.B);
                    bc += Write(q.A);
                    return bc;
                case SerializableMatrix4x4 v4:
                    bc = Write(v4.c0);
                    bc += Write(v4.c1);
                    bc += Write(v4.c2);
                    bc += Write(v4.c3);
                    return bc;
                case Matrix4x4 v4:
                    return Write((SerializableMatrix4x4)v4);
                case string str:
                    bc = Write((uint)str.Length);
                    foreach (char ch in str)
                    {
                        bc += Write(ch);
                    }
                    return bc;
                case T t when typeof(T) == typeof(string):
                    return Write((uint)0);
                default:
                    foreach (var module in modules)
                        if (module.Write<T>(value, out bc))
                            return bc;
                    break;
            }
            throw new Exception($"Missing case [{typeof(T)}:{value} was not catched]");
        }

        public static Bytable WriteCollection<T>(IEnumerable<T> value)
        {
            Bytable b = Write(UMI3DObjectKeys.CountArray) + Write(value.Count());
            foreach (var v in value)
                b += Write(v);
            return b;
        }

        public static Bytable WriteCollection<T,K>(IEnumerable<KeyValuePair<T,K>> value)
        {
            Bytable b = Write(UMI3DObjectKeys.CountArray) + Write(value.Count());
            foreach (var v in value)
                b += Write(v);
            return b;
        }

        public static Bytable WriteCollection(IEnumerable<byte> value)
        {
            var count = value.Count();
            Bytable b = Write(UMI3DObjectKeys.CountArray) + Write(count);
            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                value.ToArray().CopyTo(by, i);
                return (i + count, bs + count);
            };
            return b + new Bytable(count, f);
        }

        public static Bytable WriteIBytableCollection(IEnumerable<IBytable> ibytes, params object[] parameters)
        {
            if (ibytes.Count() > 0)
            {
                if (ibytes.First().IsCountable()) return ListToCountBytable(ibytes, parameters);
                else return ListToIndexesBytable(ibytes, parameters);
            }
            Debug.LogWarning("Empty IEnumerable");
            return Write(UMI3DObjectKeys.CountArray)
                + Write(0);
        }

        static Bytable ListToIndexesBytable(IEnumerable<IBytable> operations, params object[] parameters)
        {
            var ret = Write(UMI3DObjectKeys.IndexesArray);

            Func<byte[], int, int, (int, int, int)> f3 = (byte[] by, int i, int j) =>
            {
                return (0, i, j);
            };
            if (operations.Count() > 0)
            {
                int size = operations.Count() * sizeof(int);
                var func = operations
                    .Select(o => o.ToBytableArray(parameters))
                    .Select(c =>
                    {
                        Func<byte[], int, int, (int, int, int)> f1 = (byte[] by, int i, int j) => { var cr = c.function(by, i, 0); return (cr.Item1, i, j); };
                        return (c.size, f1);
                    })
                    .Aggregate((0, f3)
                    , (a, b) =>
                    {
                        Func<byte[], int, int, (int, int, int)> f2 = (byte[] by, int i, int j) =>
                        {
                            int i2, sj;
                            (i2, i, j) = a.Item2(by, i, j);
                            (i2, i, j) = b.Item2(by, i, j);
                            (j, sj) = UMI3DNetworkingHelper.Write(i).function(by, j, 0);
                            i = i2;
                            return (i2, i, j);
                        };
                        return (a.Item1 + b.Item1, f2);
                    });
                var length = size + func.Item1;

                Func<byte[], int, int, (int, int)> f5 = (byte[] by, int i, int bs) =>
                {
                    var couple = func.Item2(by, i + size, i);
                    return (couple.Item1, couple.Item2);
                };
                return ret + new Bytable(length, f5);
            }
            return ret;
        }

        static Bytable ListToCountBytable(IEnumerable<IBytable> operations, params object[] parameters)
        {
            return Write(UMI3DObjectKeys.CountArray)
                + Write(operations.Count())
                + operations.Select(op => op.ToBytableArray(parameters));
        }
    }

    public interface IBytable
    {
        bool IsCountable();
        Bytable ToBytableArray(params object[] parameters);
    }

    public class ByteContainer
    {
        public byte[] bytes { get; private set; }
        public int position;
        public int length;

        public ByteContainer(byte[] bytes)
        {
            this.bytes = bytes;
            position = 0;
            length = bytes.Length;
        }

        public override string ToString()
        {
            return $"{bytes.ToString<byte>()} [{position} : {length}]";
        }
    }

    public class Bytable
    {
        public int size { get; private set; }
        public Func<byte[], int, int, (int, int)> function { get; private set; }

        public Bytable(int size, Func<byte[], int, int, (int, int)> function)
        {
            this.size = size;
            this.function = function;
        }

        public Bytable()
        {
            this.size = 0;
            this.function = (by, i, bs) => (i, bs);
        }

        public byte[] ToBytes()
        {
            var b = new byte[size];
            var c = function(b, 0, 0);
            if (c.Item2 != size) Debug.LogError($"Size requested [{size}] and size used [{c.Item2}] have a different value. Last position is {c.Item1}. {b.ToString<byte>()}");
            return b;
        }

        public byte[] ToBytes(byte[] bytes, int position = 0)
        {
            var c = function(bytes, position, 0);
            if (c.Item2 != size) Debug.LogError($"Size requested [{size}] and size used [{c.Item2}] have a different value. Last position is {c.Item1}. {bytes.ToString<byte>()}");
            return bytes;
        }

        public static Bytable operator +(Bytable a, Bytable b)
        {
            if (a == null) return b;
            if (b == null) return a;

            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                (i, bs) = a.function(by, i, bs);
                return b.function(by, i, bs);
            };
            return new Bytable(a.size + b.size, f);
        }

        public static Bytable operator +(Bytable a, IEnumerable<Bytable> b)
        {
            if (b == null || b.Count() == 0) return a;
            if (a == null) return b.Aggregate((c, d) => c + d);


            var b2 = b.Aggregate((c, d) => c + d);

            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                (i, bs) = a.function(by, i, bs);
                return b2.function(by, i, bs);
            };
            return new Bytable(a.size + b2.size, f);
        }

        public static Bytable operator +(IEnumerable<Bytable> a, Bytable b)
        {
            if (a == null || a.Count() == 0) return b;
            if (b == null) return a.Aggregate((c, d) => c + d);

            var a2 = a.Aggregate((c, d) => c + d);

            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                (i, bs) = a2.function(by, i, bs);
                return b.function(by, i, bs);
            };
            return new Bytable(a2.size + b.size, f);
        }
    }

    public abstract class Umi3dNetworkingHelperModule
    {
        public abstract bool Write<T>(T value, out Bytable bytable);

        public abstract bool Read<T>(ByteContainer container, out bool readable, out T result);
    }
}