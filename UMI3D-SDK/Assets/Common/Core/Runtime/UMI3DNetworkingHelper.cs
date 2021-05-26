using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace umi3d.common
{
    public static class UMI3DNetworkingHelper
    {

        static List<Umi3dNetworkingHelperModule> modules = new List<Umi3dNetworkingHelperModule>();

        public static void AddModule(Umi3dNetworkingHelperModule module)
        {
            modules.Add(module);
        }

        public static void RemoveModule(Umi3dNetworkingHelperModule module)
        {
            modules.Remove(module);
        }

        public static T Read<T>(byte[] array, int position, int length)
        {
            return Read<T>(array, ref position, ref length);
        }

        public static T Read<T>(byte[] array, ref int position, ref int length)
        {
            T result;
            TryRead<T>(array, ref position, ref length, out result);
            return result;
        }

        public static bool TryRead<T>(byte[] array, int position, int length, out T result)
        {
            return TryRead<T>(array, ref position, ref length, out result);
        }

        public static bool TryRead<T>(byte[] array, ref int position, ref int length, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(bool):
                    if (length >= sizeof(bool))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToBoolean(array, (int)position), typeof(T));
                        position += sizeof(bool);
                        length -= sizeof(bool);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(byte):
                    if (length >= sizeof(byte))
                    {
                        result = (T)Convert.ChangeType(array[position], typeof(T));
                        position += 1;
                        length -= 1;
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(short):
                    if (length >= sizeof(short))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToInt16(array, (int)position), typeof(T));
                        position += sizeof(short);
                        length -= sizeof(short);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(ushort):
                    if (length >= sizeof(ushort))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToUInt16(array, (int)position), typeof(T));
                        position += sizeof(ushort);
                        length -= sizeof(ushort);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(int):
                    if (length >= sizeof(int))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToInt32(array, (int)position), typeof(T));
                        position += sizeof(int);
                        length -= sizeof(int);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(uint):
                    if (length >= sizeof(uint))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToUInt32(array, (int)position), typeof(T));
                        position += sizeof(uint);
                        length -= sizeof(uint);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(float):
                    if (length >= sizeof(float))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToSingle(array, (int)position), typeof(T));
                        position += sizeof(float);
                        length -= sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(long):
                    if (length >= sizeof(long))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToInt64(array, (int)position), typeof(T));
                        position += sizeof(long);
                        length -= sizeof(long);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(ulong):
                    if (length >= sizeof(ulong))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToUInt64(array, (int)position), typeof(T));
                        position += sizeof(ulong);
                        length -= sizeof(ulong);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector2):
                    if (length >= 2 * sizeof(float))
                    {
                        result = (T)Convert.ChangeType(new Vector2(BitConverter.ToSingle(array, (int)position), BitConverter.ToSingle(array, (int)position + sizeof(float))), typeof(T));
                        position += 2 * sizeof(float);
                        length -= 2 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Vector3):
                    if (length >= 3 * sizeof(float))
                    {
                        result = (T)Convert.ChangeType(new Vector3(BitConverter.ToSingle(array, (int)position), BitConverter.ToSingle(array, (int)position + sizeof(float)), BitConverter.ToSingle(array, (int)position + 2 * sizeof(float))), typeof(T));
                        position += 3 * sizeof(float);
                        length -= 3 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(Quaternion):
                case true when typeof(T) == typeof(Vector4):
                    if (length >= 4 * sizeof(float))
                    {
                        result = (T)Convert.ChangeType(new Vector4(BitConverter.ToSingle(array, (int)position), BitConverter.ToSingle(array, (int)position + sizeof(float)), BitConverter.ToSingle(array, (int)position + 2 * sizeof(float)), BitConverter.ToSingle(array, (int)position + 3 * sizeof(float))), typeof(T));
                        position += 4 * sizeof(float);
                        length -= 4 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(string):
                    if (length == 0) throw new Exception($"String length should not be 0");
                    result = (T)Convert.ChangeType(BitConverter.ToString(array, (int)position, (int)length), typeof(T));
                    position += length;
                    length = 0;
                    return true;
                default:
                    bool read;
                    foreach (var module in modules)
                        if (module.Read<T>(array, ref position, ref length, out read, out result))
                            return read;
                    throw new Exception($"Missing case [{typeof(T)} was not catched]");
            }
            result = default(T);
            return false;
        }

        public static T[] ReadArray<T>(byte[] array, ref int position, ref int length)
        {
            return ReadList<T>(array, ref position,ref length).ToArray();
        }
        public static List<T> ReadList<T>(byte[] array, ref int position, ref int length)
        {
            var res = new List<T>();
            var Length = array.Length;
            for (; position < Length && length > 0;)
            {
                T result;
                if (TryRead<T>(array, ref position, ref length, out result))
                    res.Add(result);
                else
                    break;
            }
            return res;
        }

        public static List<T> ReadList<T>(byte[] array, ref int position, ref int length, int count)
        {
            var res = new List<T>();
            var Length = array.Length;
            for (int i = 0; position < Length && length > 0 && i < count;i++)
            {
                T result;
                if (TryRead<T>(array, ref position, ref length, out result))
                    res.Add(result);
                else
                    break;
            }
            return res;
        }

        public static T[] ReadArray<T>(byte[] array, int position, int length)
        {
            return ReadArray<T>(array,ref position, ref length);
        }
        public static List<T> ReadList<T>(byte[] array, int position, int length)
        {
            return ReadList<T>(array, ref position, ref length);
        }

        public static int GetSize<T>(T value)
        {
            switch (value)
            {
                case bool b:
                    return sizeof(bool);
                case byte by:
                    return sizeof(byte);
                case short s:
                    return sizeof(short);
                case ushort us:
                    return sizeof(ushort);
                case int i:
                    return sizeof(int);
                case uint ui:
                    return sizeof(uint);
                case float f:
                    return sizeof(float);
                case long l:
                    return sizeof(long);
                case ulong ul:
                    return sizeof(ulong);
                case SerializableVector2 V2:
                case Vector2 v2:
                    return 2 * sizeof(float);
                case SerializableVector3 V3:
                case Vector3 v3:
                    return 3 * sizeof(float);
                case SerializableVector4 V4:
                case Quaternion q:
                case Vector4 v4:
                    return 4 * sizeof(float);
                case string str:
                    return sizeof(uint) + str.Length * sizeof(char);
                default:
                    int result;
                    foreach (var module in modules)
                        if (module.GetSize<T>(value, out result))
                            return result;
                    break;
            }
            throw new Exception($"Missing case [{value.GetType()} was not catched : value {value}]");
        }
        public static int GetSizeArray<T>(IEnumerable<T> value)
        {
            return value.Select(v => GetSize(v)).Aggregate((a, b) => a + b);
        }

        public static int Write<T>(T value, byte[] array, int position)
        {
            var pos = position;
            switch (value)
            {
                case bool b:
                    BitConverter.GetBytes(b).CopyTo(array, pos);
                    return sizeof(bool);
                case byte by:
                    BitConverter.GetBytes(by).CopyTo(array, pos);
                    return sizeof(byte);
                case short s:
                    BitConverter.GetBytes(s).CopyTo(array, pos);
                    return sizeof(short);
                case ushort us:
                    BitConverter.GetBytes(us).CopyTo(array, pos);
                    return sizeof(ushort);
                case int i:
                    BitConverter.GetBytes(i).CopyTo(array, pos);
                    return sizeof(int);
                case uint ui:
                    BitConverter.GetBytes(ui).CopyTo(array, pos);
                    return sizeof(uint);
                case float f:
                    BitConverter.GetBytes(f).CopyTo(array, pos);
                    return sizeof(float);
                case long l:
                    BitConverter.GetBytes(l).CopyTo(array, pos);
                    return sizeof(long);
                case ulong ul:
                    BitConverter.GetBytes(ul).CopyTo(array, pos);
                    return sizeof(ulong);
                case SerializableVector2 v2:
                    BitConverter.GetBytes(v2.X).CopyTo(array, pos);
                    BitConverter.GetBytes(v2.Y).CopyTo(array, pos + sizeof(float));
                    return 2 * sizeof(float);
                case Vector2 v2:
                    BitConverter.GetBytes(v2.x).CopyTo(array, pos);
                    BitConverter.GetBytes(v2.y).CopyTo(array, pos + sizeof(float));
                    return 2 * sizeof(float);
                case SerializableVector3 v3:
                    BitConverter.GetBytes(v3.X).CopyTo(array, pos);
                    BitConverter.GetBytes(v3.Y).CopyTo(array, pos + sizeof(float));
                    BitConverter.GetBytes(v3.Z).CopyTo(array, pos + 2 * sizeof(float));
                    return 3 * sizeof(float);
                case Vector3 v3:
                    BitConverter.GetBytes(v3.x).CopyTo(array, pos);
                    BitConverter.GetBytes(v3.y).CopyTo(array, pos + sizeof(float));
                    BitConverter.GetBytes(v3.z).CopyTo(array, pos + 2 * sizeof(float));
                    return 3 * sizeof(float);
                case SerializableVector4 v4:
                    BitConverter.GetBytes(v4.X).CopyTo(array, pos);
                    BitConverter.GetBytes(v4.Y).CopyTo(array, pos + sizeof(float));
                    BitConverter.GetBytes(v4.Z).CopyTo(array, pos + 2 * sizeof(float));
                    BitConverter.GetBytes(v4.W).CopyTo(array, pos + 3 * sizeof(float));
                    return 4 * sizeof(float);
                case Vector4 v4:
                    BitConverter.GetBytes(v4.x).CopyTo(array, pos);
                    BitConverter.GetBytes(v4.y).CopyTo(array, pos + sizeof(float));
                    BitConverter.GetBytes(v4.z).CopyTo(array, pos + 2 * sizeof(float));
                    BitConverter.GetBytes(v4.w).CopyTo(array, pos + 3 * sizeof(float));
                    return 4 * sizeof(float);
                case Quaternion q:
                    BitConverter.GetBytes(q.x).CopyTo(array, pos);
                    BitConverter.GetBytes(q.y).CopyTo(array, pos + sizeof(float));
                    BitConverter.GetBytes(q.z).CopyTo(array, pos + 2 * sizeof(float));
                    BitConverter.GetBytes(q.w).CopyTo(array, pos + 3 * sizeof(float));
                    return 4 * sizeof(float);
                case string str:
                    return sizeof(uint) + str.Length * sizeof(char);
                default:
                    int result;
                    foreach (var module in modules)
                        if (module.Write<T>(value, array, position, out result))
                            return result;
                    break;
            }
            throw new Exception($"Missing case [{typeof(T)} was not catched]");
        }

        public static int WriteArray<T>(IEnumerable<T> value, byte[] array, int position)
        {
            int count = 0;
            foreach (var v in value)
            {
                count += Write(v, array, position + count);
            }
            return count;
        }

        public static (int, Func<byte[], int, int>) ToBytes(IEnumerable<IByte> operations, params object[] parameters)
        {
            Func<byte[], int, int, (int, int, int)> f3 = (byte[] by, int i, int j) =>
            {
                return (0, i, j);
            };
            if (operations.Count() > 0)
            {
                int size = operations.Count() * sizeof(int);
                var func = operations
                    .Select(o => o.ToByteArray(parameters))
                    .Select(c =>
                    {
                        Func<byte[], int, int, (int, int, int)> f1 = (byte[] by, int i, int j) => (c.Item2(by, i), i, j);
                        return (c.Item1, f1);
                    })
                    .Aggregate((0, f3)
                    , (a, b) =>
                    {
                        Func<byte[], int, int, (int, int, int)> f2 = (byte[] by, int i, int j) =>
                        {
                            int s;
                            (s, i, j) = a.Item2(by, i, j);
                            (s, i, j) = b.Item2(by, i, j);
                            j += UMI3DNetworkingHelper.Write(i, by, j);
                            i += s;
                            return (s, i, j);
                        };
                        return (a.Item1 + b.Item1, f2);
                    });
                var length = size + func.Item1;

                Func<byte[], int, int> f5 = (byte[] by, int i) =>
                {
                    var couple = func.Item2(by, size, i);

                    Debug.Log($"couple.Item2 == length {couple.Item2 == length}");

                    return couple.Item2;
                };

                return (length, f5);
            }
            Func<byte[], int,  int> f4 = (byte[] by, int i) =>
            {
                return 0;
            };
            return (0, f4);
        }


    }

    public interface IByte
    {
        (int, Func<byte[], int, int>) ToByteArray(params object[] parameters);
    }

    public abstract class Umi3dNetworkingHelperModule
    {
        public abstract bool Write<T>(T value, byte[] array, int position, out int size);

        public abstract bool Read<T>(byte[] array, ref int position, ref int length, out bool readable, out T result);

        public abstract bool GetSize<T>(T value, out int size);
    }
}