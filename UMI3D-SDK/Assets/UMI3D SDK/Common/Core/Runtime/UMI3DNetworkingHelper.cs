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

using BeardedManStudios.Forge.Networking.Frame;
using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Helper class for byte serialization.
    /// </summary>
    public static class UMI3DNetworkingHelper
    {
        private const DebugScope scope = DebugScope.Common | DebugScope.Core | DebugScope.Bytes;

        /// <summary>
        /// Networking helpers from other modules.
        /// </summary>
        private static readonly List<Umi3dNetworkingHelperModule> modules = new List<Umi3dNetworkingHelperModule>();

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
            foreach (Umi3dNetworkingHelperModule module in moduleList)
                modules.Add(module);
        }

        /// <summary>
        /// Remove a list of module
        /// </summary>
        /// <param name="moduleList"></param>
        public static void RemoveModule(List<Umi3dNetworkingHelperModule> moduleList)
        {
            foreach (Umi3dNetworkingHelperModule module in moduleList)
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
            TryRead<T>(container, out T result);
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
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        result = (T)Convert.ChangeType(new SerializableVector2(x, y), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector2):
                    if (container.length >= 2 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        result = (T)Convert.ChangeType(new Vector2(x, y), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector3):
                    if (container.length >= 3 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        TryRead(container, out float z);
                        result = (T)Convert.ChangeType(new SerializableVector3(x, y, z), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector3):
                    if (container.length >= 3 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        TryRead(container, out float z);
                        result = (T)Convert.ChangeType(new Vector3(x, y, z), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Quaternion):
                    if (container.length >= 4 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        TryRead(container, out float z);
                        TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new Quaternion(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableColor):
                    if (container.length >= 4 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        TryRead(container, out float z);
                        TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new SerializableColor(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Color):
                    if (container.length >= 4 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        TryRead(container, out float z);
                        TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new Color(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector4):
                    if (container.length >= 4 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        TryRead(container, out float z);
                        TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new SerializableVector4(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector4):
                    if (container.length >= 4 * sizeof(float))
                    {
                        TryRead(container, out float x);
                        TryRead(container, out float y);
                        TryRead(container, out float z);
                        TryRead(container, out float w);
                        result = (T)Convert.ChangeType(new Vector4(x, y, z, w), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableMatrix4x4):
                    if (container.length >= 4 * 4 * sizeof(float))
                    {

                        TryRead(container, out Vector4 c0);
                        TryRead(container, out Vector4 c1);
                        TryRead(container, out Vector4 c2);
                        TryRead(container, out Vector4 c3);

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
                            if (TryRead<char>(container, out char c))
                            {
                                r += c;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                    result = (T)Convert.ChangeType(r, typeof(T));
                    return true;
                case true when typeof(T).IsSubclassOf(typeof(TypedDictionaryEntry)):
                    result = default(T);
                    var entry = (TypedDictionaryEntry)Activator.CreateInstance(typeof(T));
                    if (entry.Read(container))
                    {
                        result = (T)(object)entry;
                        return true;
                    }
                    return false;
                default:
                    bool read;
                    foreach (Umi3dNetworkingHelperModule module in modules)
                    {
                        if (module.Read<T>(container, out read, out result))
                            return read;
                    }

                    throw new Exception($"Missing case [{typeof(T)} was not catched]");
            }
            result = default(T);
            return false;
        }

        /// <summary>
        /// Read a byteContainer known to be a collection of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns>A deserialized array of the elements.</returns>
        public static T[] ReadArray<T>(ByteContainer container)
        {
            return ReadList<T>(container).ToArray();
        }

        /// <summary>
        /// Read a byteContainer known to be a collection of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns>A deserialized list of the elements.</returns>
        /// <exception cref="Exception">Not a known collection type.</exception>
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

        /// <summary>
        /// Applies an operation to a <paramref name="list"/> based on the value in a byte <paramref name="container"/>.
        /// </summary>
        /// Passing an invalid or no operation id will clear the list and fill it again with the container values.
        /// <typeparam name="T"></typeparam>
        /// <param name="operationId">Operation to apply UMI3D key</param>
        /// <param name="container">Byte container with the interesting value (index or index+value)</param>
        /// <param name="list">List to apply the changes</param>
        public static void ReadList<T>(uint operationId, ByteContainer container, List<T> list)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    int ind = UMI3DNetworkingHelper.Read<int>(container);
                    T value = UMI3DNetworkingHelper.Read<T>(container);
                    if (ind == list.Count)
                        list.Add(value);
                    else if (ind < list.Count && ind >= 0)
                        list.Insert(ind, value);
                    else
                        UMI3DLogger.LogWarning($"Add value ignore for {ind} in collection of size {list.Count}", scope);
                    break;
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    list.RemoveAt(UMI3DNetworkingHelper.Read<int>(container));
                    break;
                case UMI3DOperationKeys.SetEntityListProperty:
                    int index = UMI3DNetworkingHelper.Read<int>(container);
                    T v = UMI3DNetworkingHelper.Read<T>(container);
                    list[index] = v;
                    break;
                default:
                    list.Clear();
                    list.AddRange(UMI3DNetworkingHelper.ReadList<T>(container));
                    break;
            }
        }

        /// <summary>
        /// Generic class to describe a Dictionary entry that can be read from a ByteContainer
        /// </summary>
        private abstract class TypedDictionaryEntry
        {
            /// <summary>
            /// Read the entry.
            /// </summary>
            /// <param name="container"></param>
            /// <returns></returns>
            public abstract bool Read(ByteContainer container);
        }

        /// <summary>
        /// Utility class to describe a Dictionary<K,V> entry that can be read from a ByteContainer
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <typeparam name="V">Value Type</typeparam>
        private class TypedDictionaryEntry<K, V> : TypedDictionaryEntry
        {
            public V value;
            public K key;

            public KeyValuePair<K, V> keyValuePair => new KeyValuePair<K, V>(key, value);

            /// <summary>
            /// Read both the key and value stored in the container.
            /// </summary>
            /// <param name="container"></param>
            /// <returns></returns>
            public override bool Read(ByteContainer container)
            {
                return TryRead(container, out key) && TryRead(container, out value);
            }
        }

        /// <summary>
        /// Read a Dictionary<K,V> From a ByteContainer
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Dictionary<K, V> ReadDictionary<K, V>(ByteContainer container)
        {
            return ReadList<TypedDictionaryEntry<K, V>>(container).Select(k => k.keyValuePair).ToDictionary();
        }

        /// <summary>
        /// Read a List from a container where starting indexes are given for each values at the begining of the list. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        private static List<T> ReadIndexesList<T>(ByteContainer container)
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
                var SubContainer = new ByteContainer(container.timeStep, container.bytes) { position = valueIndex, length = nopIndex - valueIndex };
                if (!TryRead(SubContainer, out T v)) break;
                result.Add(v);
                valueIndex = nopIndex;
            }
            {
                var SubContainer = new ByteContainer(container.timeStep, container.bytes) { position = valueIndex, length = maxLength - valueIndex };
                if (TryRead(SubContainer, out T v))
                    result.Add(v);
            }
            return result;
        }

        /// <summary>
        /// Read a collection in way that cannot exceed the indicated collection size in the <paramref name="container"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        private static List<T> ReadCountList<T>(ByteContainer container)
        {
            int count = UMI3DNetworkingHelper.Read<int>(container);
            var res = new List<T>();
            int Length = container.bytes.Length;
            for (int i = 0; container.position < Length && container.length > 0 && i < count; i++)
            {
                if (TryRead<T>(container, out T result))
                    res.Add(result);
                else
                    break;
            }
            return res;
        }

        /// <summary>
        /// Read an array of bytes from a <paramref name="container"/>.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static byte[] ReadByteArray(ByteContainer container)
        {
            byte type = UMI3DNetworkingHelper.Read<byte>(container);
            int count = UMI3DNetworkingHelper.Read<int>(container);
            byte[] res = new byte[count];
            container.bytes.CopyRangeTo(res, 0, container.position, container.position + count - 1);
            return res;
        }

        /// <summary>
        /// Get an enumerable collection of <see cref="ByteContainer"/> 
        /// from a <paramref name="container"/> that is an indexed array.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
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
                var SubContainer = new ByteContainer(container.timeStep, container.bytes) { position = valueIndex, length = nopIndex - valueIndex };
                yield return SubContainer;
                valueIndex = nopIndex;
            }
            {
                var SubContainer = new ByteContainer(container.timeStep, container.bytes) { position = valueIndex, length = maxLength - valueIndex };
                yield return SubContainer;
            }
            yield break;
        }

        /// <summary>
        /// Get the right <see cref="UMI3DObjectKeys"/> for a value of a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Bytable GetType<T>(T value)
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

        /// <summary>
        /// Get a bytable from a enumerable set of values of unknown type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Bytable Write(IEnumerable value)
        {
            return WriteCollection(value.Cast<object>());
        }

        /// <summary>
        /// Get a bytable from a key-value set of values of unknown type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Bytable Write(IDictionary value)
        {
            return WriteCollection(value);
        }

        /// <summary>
        /// Get a bytable from any <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
                        int s = sizeof(char);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(char), f);
                case bool b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(bool);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(bool), f);
                case byte b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(byte);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(byte), f);
                case short b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(short);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(short), f);
                case ushort b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(ushort);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(ushort), f);
                case int b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(int);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(int), f);
                case uint b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(uint);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(uint), f);
                case float b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(float);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(float), f);
                case long b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(long);
                        return (i + s, bs + s);
                    };
                    return new Bytable(sizeof(long), f);
                case ulong b:
                    f = (by, i, bs) =>
                    {
                        BitConverter.GetBytes(b).CopyTo(by, i);
                        int s = sizeof(ulong);
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
                case IDictionary Id:
                    return Write(Id);
                case IEnumerable Ie:
                    return Write(Ie);
                case DictionaryEntry De:
                    return Write(De.Key) + Write(De.Value);
                default:
                    if (typeof(T) == typeof(string))
                        return Write((uint)0);
                    foreach (Umi3dNetworkingHelperModule module in modules)
                    {
                        if (module.Write<T>(value, out bc))
                            return bc;
                    }

                    break;
            }

            throw new Exception($"Missing case [{typeof(T)}:{value} was not catched]");
        }

        /// <summary>
        /// Get a bytable from a enumerable set of values of unknown type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Bytable WriteCollection<T>(IEnumerable<T> value)
        {
            if (typeof(IBytable).IsAssignableFrom(typeof(T)) || (value.Count() > 0 && !value.Any(e => !typeof(IBytable).IsAssignableFrom(e.GetType()))))
            {
                return WriteIBytableCollection(value.Select((e) => e as IBytable));
            }
            Bytable b = Write(UMI3DObjectKeys.CountArray) + Write(value.Count());
            foreach (T v in value)
                b += Write(v);
            return b;
        }

        /// <summary>
        /// Get a bytable from a enumerable set of key-value pairs of unknown types.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Bytable WriteCollection(IDictionary value)
        {
            if (value.Count > 0 && !value.Entries().Any(e => !typeof(IBytable).IsAssignableFrom(e.Value.GetType())))
            {
                return WriteIBytableCollection(value.Entries().Select((e) => new DictionaryEntryBytable(e)));
            }
            Bytable b = Write(UMI3DObjectKeys.CountArray) + Write(value.Count);
            foreach (object v in value)
                b += Write(v);
            return b;
        }

        /// <summary>
        /// Utility class for dictionnary serialization of key-value pairs.
        /// </summary>
        private class DictionaryEntryBytable : IBytable
        {
            private readonly object key;
            private readonly IBytable value;

            public DictionaryEntryBytable(DictionaryEntry entry)
            {
                this.key = entry.Key;
                this.value = entry.Value as IBytable;
            }
            
            /// <inheritdoc/>
            public bool IsCountable()
            {
                return value.IsCountable();
            }

            /// <inheritdoc/>
            public Bytable ToBytableArray(params object[] parameters)
            {
                return Write(key) + Write(value);
            }
        }

        /// <summary>
        /// Get a <see cref="Bytable"/> from an enumerable set of bytes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Bytable WriteCollection(IEnumerable<byte> value)
        {
            int count = value.Count();
            Bytable b = Write(UMI3DObjectKeys.CountArray) + Write(count);
            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                value.ToArray().CopyTo(by, i);
                return (i + count, bs + count);
            };
            return b + new Bytable(count, f);
        }

        /// <summary>
        /// Get a <see cref="Bytable"/> from an enumerable set of adequate values and <paramref name="parameters"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Bytable WriteIBytableCollection(IEnumerable<IBytable> ibytes, params object[] parameters)
        {
            if (ibytes.Count() > 0)
            {
                if (ibytes.First().IsCountable()) return ListToCountBytable(ibytes, parameters);
                else return ListToIndexesBytable(ibytes, parameters);
            }
            return Write(UMI3DObjectKeys.CountArray)
                + Write(0);
        }

        /// <summary>
        /// Get a <see cref="Bytable"/> from an enumerable set of <paramref name="operations"/> to apply and their <paramref name="parameters"/>.
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static Bytable ListToIndexesBytable(IEnumerable<IBytable> operations, params object[] parameters)
        {
            Bytable ret = Write(UMI3DObjectKeys.IndexesArray);

            Func<byte[], int, int, (int, int, int)> f3 = (byte[] by, int i, int j) =>
            {
                return (0, i, j);
            };
            if (operations.Count() > 0)
            {
                int size = operations.Count() * sizeof(int);
                (int, Func<byte[], int, int, (int, int, int)> f3) func = operations
                    .Select(o => o.ToBytableArray(parameters))
                    .Select(c =>
                    {
                        Func<byte[], int, int, (int, int, int)> f1 = (byte[] by, int i, int j) => { (int, int) cr = c.function(by, i, 0); return (cr.Item1, i, j); };
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
                int length = size + func.Item1;

                Func<byte[], int, int, (int, int)> f5 = (byte[] by, int i, int bs) =>
                {
                    (int, int, int) couple = func.Item2(by, i + size, i);
                    return (couple.Item1, couple.Item2);
                };
                return ret + new Bytable(length, f5);
            }
            return ret;
        }

        /// <summary>
        /// Get a bytable with a known size from a list of <paramref name="operations"/> to apply and their <paramref name="parameters"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Bytable ListToCountBytable(IEnumerable<IBytable> operations, params object[] parameters)
        {
            return Write(UMI3DObjectKeys.CountArray)
                + Write(operations.Count())
                + operations.Select(op => op.ToBytableArray(parameters));
        }
    }

    /// <summary>
    /// Object that could be converted to an array of bytes.
    /// </summary>
    public interface IBytable
    {
        /// <summary>
        /// Could the number of elements be deduced from a size parameter?
        /// </summary>
        /// <returns></returns>
        bool IsCountable();

        /// <summary>
        /// Convert the parameters to an array of <see cref="Bytable"/>.
        /// </summary>
        /// This method is used in the bytes networking system.
        /// <param name="parameters"></param>
        /// <returns></returns>
        Bytable ToBytableArray(params object[] parameters);
    }

    /// <summary>
    /// Bytes section in an array of bytes.
    /// </summary>
    public class ByteContainer
    {
        public ulong timeStep { get; private set; }
        
        /// <summary>
        /// The byte array that the container section belongs to.
        /// </summary>
        public byte[] bytes { get; private set; }

        /// <summary>
        /// Starting index of the section in the <see cref="bytes"/> array.
        /// </summary>
        public int position;

        /// <summary>
        /// Number of byte in the section.
        /// </summary>
        public int length;

        public ByteContainer(Binary frame) : this(frame.TimeStep, frame.StreamData.byteArr)
        {
        }

        public ByteContainer(ulong timeStep, byte[] bytes)
        {
            this.timeStep = timeStep;
            this.bytes = bytes;
            position = 0;
            length = bytes.Length;
        }

        public ByteContainer(ByteContainer container)
        {

            this.bytes = container.bytes;
            position = container.position;
            length = container.length;
            timeStep = container.timeStep;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{bytes.ToString<byte>()} [{position} : {length}]";
        }
    }

    /// <summary>
    /// Intermediary class used for serialization of objects into an array of bytes.
    /// </summary>
    /// it is possible to sum 2 bytables to obtain a larger bytable.
    public class Bytable
    {
        private const DebugScope scope = DebugScope.Common | DebugScope.Core | DebugScope.Bytes;

        /// <summary>
        /// Size of the array of byte to reserve.
        /// </summary>
        public int size { get; private set; }
        /// <summary>
        /// Function that take an array of byte to fill up, an index of a cell to write, and an already been used size. 
        /// This function shoudl return a couple ((position+newly reserved size),(current total size + newly reserved size)).
        /// </summary>
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
            byte[] b = new byte[size];
            (int, int) c = function(b, 0, 0);
            if (c.Item2 != size) UMI3DLogger.LogError($"Size requested [{size}] and size used [{c.Item2}] have a different value. Last position is {c.Item1}. {b.ToString<byte>()}", scope);
            return b;
        }

        public byte[] ToBytes(byte[] bytes, int position = 0)
        {
            (int, int) c = function(bytes, position, 0);
            if (c.Item2 != size) UMI3DLogger.LogError($"Size requested [{size}] and size used [{c.Item2}] have a different value. Last position is {c.Item1}. {bytes.ToString<byte>()}", scope);
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


            Bytable b2 = b.Aggregate((c, d) => c + d);

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

            Bytable a2 = a.Aggregate((c, d) => c + d);

            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                (i, bs) = a2.function(by, i, bs);
                return b.function(by, i, bs);
            };
            return new Bytable(a2.size + b.size, f);
        }
    }

    /// <summary>
    /// Helper class to serialize objects.
    /// </summary>
    /// Typically used to serialize objects that are not defined in the UMI3D core.
    public abstract class Umi3dNetworkingHelperModule
    {
        /// <summary>
        /// Write the object as a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize.</typeparam>
        /// <param name="value">Object to serialize.</param>
        /// <param name="bytable">Object as a bytable.</param>
        /// <returns></returns>
        public abstract bool Write<T>(T value, out Bytable bytable);

        /// <summary>
        /// Retrieve an object from a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">Type of the object to deserialize.</typeparam>
        /// <param name="container">Byte container containing the object.</param>
        /// <param name="readable">has the containr successfully been read?</param>
        /// <param name="result">Deserialized object.</param>
        /// <returns></returns>
        public abstract bool Read<T>(ByteContainer container, out bool readable, out T result);
    }
}