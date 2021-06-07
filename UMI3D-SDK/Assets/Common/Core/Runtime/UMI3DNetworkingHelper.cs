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

        public static void AddModule(List<Umi3dNetworkingHelperModule> moduleList)
        {
            foreach(var module in moduleList)
                modules.Add(module);
        }

        public static void RemoveModule(List<Umi3dNetworkingHelperModule> moduleList)
        {
            foreach (var module in moduleList)
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
                case true when typeof(T) == typeof(char):
                    if (length >= sizeof(char))
                    {
                        result = (T)Convert.ChangeType(BitConverter.ToChar(array, (int)position), typeof(T));
                        position += sizeof(char);
                        length -= sizeof(char);
                        return true;
                    }
                    break;
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
                case true when typeof(T) == typeof(SerializableVector2):
                case true when typeof(T) == typeof(Vector2):
                    if (length >= 2 * sizeof(float))
                    {
                        result = (T)Convert.ChangeType(new Vector2(BitConverter.ToSingle(array, (int)position), BitConverter.ToSingle(array, (int)position + sizeof(float))), typeof(T));
                        position += 2 * sizeof(float);
                        length -= 2 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector3):
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
                    if (length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        x = BitConverter.ToSingle(array, (int)position);
                        y = BitConverter.ToSingle(array, (int)position + sizeof(float));
                        z = BitConverter.ToSingle(array, (int)position + 2 * sizeof(float));
                        w = BitConverter.ToSingle(array, (int)position + 3 * sizeof(float));
                        result = (T)Convert.ChangeType(new Quaternion(x, y, z, w), typeof(T));
                        position += 4 * sizeof(float);
                        length -= 4 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableColor):
                case true when typeof(T) == typeof(Color):
                    if (length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        x = BitConverter.ToSingle(array, (int)position);
                        y = BitConverter.ToSingle(array, (int)position + sizeof(float));
                        z = BitConverter.ToSingle(array, (int)position + 2 * sizeof(float));
                        w = BitConverter.ToSingle(array, (int)position + 3 * sizeof(float));
                        result = (T)Convert.ChangeType(new Color(x, y, z, w), typeof(T));
                        position += 4 * sizeof(float);
                        length -= 4 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableVector4):
                case true when typeof(T) == typeof(Vector4):
                    if (length >= 4 * sizeof(float))
                    {
                        float x, y, z, w;
                        x = BitConverter.ToSingle(array, (int)position);
                        y = BitConverter.ToSingle(array, (int)position + sizeof(float));
                        z = BitConverter.ToSingle(array, (int)position + 2 * sizeof(float));
                        w = BitConverter.ToSingle(array, (int)position + 3 * sizeof(float));
                        result = (T)Convert.ChangeType(new Vector4(x, y, z, w), typeof(T));
                        position += 4 * sizeof(float);
                        length -= 4 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableMatrix4x4):
                    if (length >= 4 * 4 * sizeof(float))
                    {
                        Vector4 c0, c1, c2, c3;

                        TryRead<Vector4>(array, ref position, ref length, out c0);
                        TryRead<Vector4>(array, ref position, ref length, out c1);
                        TryRead<Vector4>(array, ref position, ref length, out c2);
                        TryRead<Vector4>(array, ref position, ref length, out c3);

                        result = (T)Convert.ChangeType(new SerializableMatrix4x4(c0, c1, c2, c3), typeof(T));
                        position += 4 * 4 * sizeof(float);
                        length -= 4 * 4 * sizeof(float);
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(string):
                    if (length == 0) throw new Exception($"String length should not be 0");
                    result = default(T);
                    uint s;
                    string r = "";
                    if (TryRead<uint>(array, ref position, ref length, out s))
                    {
                        for (uint i = 0; i < s; i++)
                        {
                            char c;
                            if (TryRead<char>(array, ref position, ref length, out c))
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
                        if (module.Read<T>(array, ref position, ref length, out read, out result))
                            return read;
                    throw new Exception($"Missing case [{typeof(T)} was not catched]");
            }
            result = default(T);
            return false;
        }

        public static T[] ReadArray<T>(byte[] array, ref int position, ref int length)
        {
            return ReadList<T>(array, ref position, ref length).ToArray();
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
            for (int i = 0; position < Length && length > 0 && i < count; i++)
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
            return ReadArray<T>(array, ref position, ref length);
        }
        public static List<T> ReadList<T>(byte[] array, int position, int length)
        {
            return ReadList<T>(array, ref position, ref length);
        }

        public static int GetSize<T>(T value)
        {
            switch (value)
            {
                case char b:
                    return sizeof(char);
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
                case SerializableColor serializableColor:
                case Color color:
                case SerializableVector4 V4:
                case Quaternion q:
                case Vector4 v4:
                    return 4 * sizeof(float);
                case SerializableMatrix4x4 v4:
                    return 4 * 4 * sizeof(float);
                case string str:
                    return sizeof(uint) + str.Length * sizeof(char);
                case T t when typeof(T) == typeof(string):
                    return sizeof(uint);
                default:
                    int result;
                    foreach (var module in modules)
                        if (module.GetSize<T>(value, out result))
                            return result;
                    break;
            }

            throw new Exception($"Missing case [{typeof(T)}||{value?.GetType()} was not catched : value {value}]");
        }
        public static int GetSizeArray<T>(IEnumerable<T> value)
        {
            return value.Select(v => GetSize(v)).Aggregate((a, b) => a + b);
        }

        public static int Write<T>(T value, byte[] array, ref int position)
        {
            int ret;
            Write<T>(value, array, ref position, out ret);
            return ret;
        }

        public static void Write<T>(T value, byte[] array, ref int position, out int size)
        {
            switch (value)
            {
                case char c:
                    BitConverter.GetBytes(c).CopyTo(array, (int)position);
                    size = sizeof(char);
                    position += size;
                    return;
                case bool b:
                    BitConverter.GetBytes(b).CopyTo(array, (int)position);
                    size = sizeof(bool);
                    position += size;
                    return;
                case byte by:
                    BitConverter.GetBytes(by).CopyTo(array, (int)position);
                    size = sizeof(byte);
                    position += size;
                    return;
                case short s:
                    BitConverter.GetBytes(s).CopyTo(array, (int)position);
                    size = sizeof(short);
                    position += size;
                    return;
                case ushort us:
                    BitConverter.GetBytes(us).CopyTo(array, (int)position);
                    size = sizeof(ushort);
                    position += size;
                    return;
                case int i:
                    BitConverter.GetBytes(i).CopyTo(array, (int)position);
                    size = sizeof(int);
                    position += size;
                    return;
                case uint ui:
                    BitConverter.GetBytes(ui).CopyTo(array, (int)position);
                    size = sizeof(uint);
                    position += size;
                    return;
                case float f:
                    BitConverter.GetBytes(f).CopyTo(array, (int)position);
                    size = sizeof(float);
                    position += size;
                    return;
                case long l:
                    BitConverter.GetBytes(l).CopyTo(array, (int)position);
                    size = sizeof(long);
                    position += size;
                    return;
                case ulong ul:
                    BitConverter.GetBytes(ul).CopyTo(array, (int)position);
                    size = sizeof(ulong);
                    position += size;
                    return;
                case SerializableVector2 v2:
                    size = Write(v2.X, array, ref position);
                    size += Write(v2.Y, array, ref position);
                    return;
                case Vector2 v2:
                    size = Write(v2.x, array, ref position);
                    size += Write(v2.y, array, ref position);
                    return;
                case SerializableVector3 v3:
                    size = Write(v3.X, array, ref position);
                    size += Write(v3.Y, array, ref position);
                    size += Write(v3.Z, array, ref position);
                    return;
                case Vector3 v3:
                    size = Write(v3.x, array, ref position);
                    size += Write(v3.y, array, ref position);
                    size += Write(v3.z, array, ref position);
                    return;
                case SerializableVector4 v4:
                    size = Write(v4.X, array, ref position);
                    size += Write(v4.Y, array, ref position);
                    size += Write(v4.Z, array, ref position);
                    size += Write(v4.W, array, ref position);
                    return;
                case Vector4 v4:
                    size = Write(v4.x, array, ref position);
                    size += Write(v4.y, array, ref position);
                    size += Write(v4.z, array, ref position);
                    size += Write(v4.w, array, ref position);
                    return;
                case Quaternion q:
                    size = Write(q.x, array, ref position);
                    size += Write(q.y, array, ref position);
                    size += Write(q.z, array, ref position);
                    size += Write(q.w, array, ref position);
                    return;
                case Color q:
                    size = Write(q.r, array, ref position);
                    size += Write(q.g, array, ref position);
                    size += Write(q.b, array, ref position);
                    size += Write(q.a, array, ref position);
                    return;
                case SerializableColor q:
                    size = Write(q.R, array, ref position);
                    size += Write(q.G, array, ref position);
                    size += Write(q.B, array, ref position);
                    size += Write(q.A, array, ref position);
                    return;
                case SerializableMatrix4x4 v4:
                    size = Write(v4.c0, array, ref position);
                    size += Write(v4.c1, array, ref position);
                    size += Write(v4.c2, array, ref position);
                    size += Write(v4.c3, array, ref position);
                    return;
                case Matrix4x4 v4:
                    Write((SerializableMatrix4x4)v4, array,ref position,out size);
                    return;
                case string str:
                    Write((uint)str.Length, array, ref position, out size);
                    foreach (char ch in str)
                    {
                        size += Write(ch, array, ref position);
                    }
                    return;
                case T t when typeof(T) == typeof(string):
                    Write((uint)0, array, ref position, out size);
                    return;
                default:
                    foreach (var module in modules)
                        if (module.Write<T>(value, array, ref position, out size))
                            return;
                    break;
            }
            throw new Exception($"Missing case [{typeof(T)} was not catched]");
        }

        public static int WriteArray<T>(IEnumerable<T> value, byte[] array, ref int position)
        {
            int size;
            WriteArray<T>(value, array, ref position, out size);
            return size;
        }

        public static void WriteArray<T>(IEnumerable<T> value, byte[] array, ref int position, out int size)
        {
            size = 0;
            int c;
            foreach (var v in value)
            {
                Write(v, array, ref position, out c);
                size += c;
            }
        }

        public static (int, Func<byte[], int, int, (int, int)>) ToBytes(IEnumerable<IByte> operations, int baseSize, params object[] parameters)
        {
            Func<byte[], int, int, (int, int, int)> f3 = (byte[] by, int i, int j) =>
            {
                return (0, i, j);
            };
            if (operations.Count() > 0)
            {
                int size = operations.Count() * sizeof(int);
                var func = operations
                    .Select(o => o.ToByteArray(0, parameters))
                    .Select(c =>
                    {
                        Func<byte[], int, int, (int, int, int)> f1 = (byte[] by, int i, int j) => { var cr = c.Item2(by, i,0); return (cr.Item1, i, j); };
                        return (c.Item1, f1);
                    })
                    .Aggregate((0, f3)
                    , (a, b) =>
                    {
                        Func<byte[], int, int, (int, int, int)> f2 = (byte[] by, int i, int j) =>
                        {
                            int i2,sj;
                            (i2, i, j) = a.Item2(by, i, j);
                            (i2, i, j) = b.Item2(by, i, j);
                            UMI3DNetworkingHelper.Write(i, by, ref j, out sj);
                            i = i2;
                            return (i2, i, j);
                        };
                        return (a.Item1 + b.Item1, f2);
                    });
                var length = size + func.Item1;

                Func<byte[], int, int, (int, int)> f5 = (byte[] by, int i,int bs) =>
                {
                    var couple = func.Item2(by, i + size, i);
                    return (i + bs + couple.Item2,bs + couple.Item2);
                };
                return (length + baseSize, f5);
            }
            Func<byte[], int, int, (int, int)> f4 = (byte[] by, int i,int bs) =>
            {
                return (i,bs);
            };
            return (baseSize, f4);
        }


    }

    public interface IByte
    {
        (int, Func<byte[], int, int, (int,int)>) ToByteArray(int baseSize, params object[] parameters);
    }

    public abstract class Umi3dNetworkingHelperModule
    {
        public abstract bool Write<T>(T value, byte[] array, ref int position, out int size);

        public abstract bool Read<T>(byte[] array, ref int position, ref int length, out bool readable, out T result);

        public abstract bool GetSize<T>(T value, out int size);
    }
}