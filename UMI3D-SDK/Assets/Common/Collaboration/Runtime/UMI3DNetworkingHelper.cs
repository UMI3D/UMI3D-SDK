using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class UMI3DNetworkingHelper
{

    public static T Read<T>(byte[] array, ulong position, ulong length = 0)
    {
        return Read<T>(array, ref position, length);
    }

    public static T Read<T>(byte[] array, ref ulong position, ulong length = 0)
    {
        T result;
        switch (true)
        {
            case true when typeof(T) == typeof(bool):
                result = (T)Convert.ChangeType(BitConverter.ToBoolean(array, (int)position), typeof(T));
                position += sizeof(bool);
                break;
            case true when typeof(T) == typeof(byte):
                result =  (T)Convert.ChangeType(array[position], typeof(T));
                position += 1;
                break;
            case true when typeof(T) == typeof(short):
                result =  (T)Convert.ChangeType(BitConverter.ToInt16(array, (int)position), typeof(T));
                position += sizeof(short);
                break;
            case true when typeof(T) == typeof(ushort):
                result =  (T)Convert.ChangeType(BitConverter.ToUInt16(array, (int)position), typeof(T));
                position += sizeof(ushort);
                break;
            case true when typeof(T) == typeof(int):
                result =  (T)Convert.ChangeType(BitConverter.ToInt32(array, (int)position), typeof(T));
                position += sizeof(int);
                break;
            case true when typeof(T) == typeof(uint):
                result =  (T)Convert.ChangeType(BitConverter.ToUInt32(array, (int)position), typeof(T));
                position += sizeof(uint);
                break;
            case true when typeof(T) == typeof(float):
                result =  (T)Convert.ChangeType(BitConverter.ToSingle(array, (int)position), typeof(T));
                position += sizeof(float);
                break;
            case true when typeof(T) == typeof(long):
                result =  (T)Convert.ChangeType(BitConverter.ToInt64(array, (int)position), typeof(T));
                position += sizeof(long);
                break;
            case true when typeof(T) == typeof(ulong):
                result =  (T)Convert.ChangeType(BitConverter.ToUInt64(array, (int)position), typeof(T));
                position += sizeof(ulong);
                break;
            case true when typeof(T) == typeof(Vector2):
                result = (T)Convert.ChangeType(new Vector2(BitConverter.ToSingle(array, (int)position), BitConverter.ToSingle(array, (int)position + sizeof(float))), typeof(T));
                position += 2 * sizeof(float);
                break;
            case true when typeof(T) == typeof(Vector3):
                result = (T)Convert.ChangeType(new Vector3(BitConverter.ToSingle(array, (int)position), BitConverter.ToSingle(array, (int)position + sizeof(float)), BitConverter.ToSingle(array, (int)position + 2 * sizeof(float))), typeof(T));
                position += 3 * sizeof(float);
                break;
            case true when typeof(T) == typeof(Vector4):
                result = (T)Convert.ChangeType(new Vector4(BitConverter.ToSingle(array, (int)position), BitConverter.ToSingle(array, (int)position + sizeof(float)), BitConverter.ToSingle(array, (int)position + 2 * sizeof(float)), BitConverter.ToSingle(array, (int)position + 3 * sizeof(float))), typeof(T));
                position += 4 * sizeof(float);
                break;
            case true when typeof(T) == typeof(string):
                if(length == 0) throw new Exception($"String length should not be 0");
                result = (T)Convert.ChangeType(BitConverter.ToString(array, (int)position, (int)length), typeof(T));
                position += length;
                break;
            default:
                throw new Exception($"Missing case [{typeof(T)} was not catched]");
        }
        return result;
    }

    public static T[] ReadArray<T>(byte[] array, ref ulong position, ulong length) {
        var res = new T[length];
        for(ulong i = 0; i < length; i++)
        {
            res[i] = Read<T>(array, ref position);
        }
        return res;
    }

    public static ulong GetSize<T>(T value) {
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
            case Vector2 v2:
                return 2*sizeof(float);
            case Vector3 v3:
                return 3 * sizeof(float);
            case Vector4 v4:
                return 4 * sizeof(float);
            case string str:
                return sizeof(uint) + (ulong)str.Length * sizeof(Char);
        }
        throw new Exception($"Missing case [{typeof(T)} was not catched]");
    }
    public static ulong GetSizeArray<T>(T[] value) {
        return value.Select(v => GetSize(v)).Aggregate((a, b) => (a + b));
    }

    public static ulong Write<T>(T value, byte[] array, ulong position) {
        var pos = (int)position;
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
            case Vector2 v2:
                BitConverter.GetBytes(v2.x).CopyTo(array, pos);
                BitConverter.GetBytes(v2.y).CopyTo(array, pos + sizeof(float));
                return 2 * sizeof(float);
            case Vector3 v3:
                BitConverter.GetBytes(v3.x).CopyTo(array, pos);
                BitConverter.GetBytes(v3.y).CopyTo(array, pos + sizeof(float));
                BitConverter.GetBytes(v3.z).CopyTo(array, pos + 2 * sizeof(float));
                return 3 * sizeof(float);
            case Vector4 v4:
                BitConverter.GetBytes(v4.x).CopyTo(array, pos);
                BitConverter.GetBytes(v4.y).CopyTo(array, pos + sizeof(float));
                BitConverter.GetBytes(v4.z).CopyTo(array, pos + 2 * sizeof(float));
                BitConverter.GetBytes(v4.w).CopyTo(array, pos + 3 * sizeof(float));
                return 4 * sizeof(float);
            case string str:
                return sizeof(uint) + (ulong)str.Length * sizeof(Char);
        }
        throw new Exception($"Missing case [{typeof(T)} was not catched]");
    }

    public static ulong WritteArray<T>(T[] value, byte[] array, ulong position) {
        ulong count = 0;
        foreach(var v in value)
        {
            count += Write(v, array, position + count);
        }
        return count;
    }

}
