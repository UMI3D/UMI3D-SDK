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

namespace umi3d.common
{
    [UMI3DSerializerOrder(1001)]
    public class UMI3DSerializerBasicEnumModules : UMI3DSerializerModule
    {
        private static bool IsEnumType(object obj, out Type underlyingType) => IsEnumType(obj?.GetType(), out underlyingType);

        private static bool IsEnumType(Type type, out Type underlyingType)
        {
            if (type == null || !type.IsEnum)
            {
                underlyingType = null;
                return false;
            }

            underlyingType = Enum.GetUnderlyingType(type);
            return true;
        }

#if UNITY_EDITOR
        public static bool EDITOR_ONLY_IsEnumType(object obj, out Type underlyingType) => IsEnumType(obj, out underlyingType);
        public static bool EDITOR_ONLY_IsEnumType(Type type, out Type underlyingType) => IsEnumType(type, out underlyingType);
#endif


        public bool? IsCountable<T>() => typeof(T).IsEnum ? true : null;

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            if (IsEnumType(typeof(T), out Type underlyingType))
            {
                Read(underlyingType, container, out readable, out object result2);
                result = (T)result2;
                return true;
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (IsEnumType(typeof(T), out Type underlyingType))
                return Write(value, underlyingType, out bytable, parameters);

            bytable = null;
            return false;
        }



        bool Read(Type type, ByteContainer container, out bool readable, out object result)
        {
            readable = true;
            switch (true)
            {
                case true when type == typeof(byte):
                    result = UMI3DSerializer.Read<byte>(container);
                    return true;
                case true when type == typeof(short):
                    result = UMI3DSerializer.Read<short>(container);
                    return true;
                case true when type == typeof(ushort):
                    result = UMI3DSerializer.Read<ushort>(container);
                    return true;
                case true when type == typeof(int):
                    result = UMI3DSerializer.Read<int>(container);
                    return true;
                case true when type == typeof(uint):
                    result = UMI3DSerializer.Read<uint>(container);
                    return true;
                case true when type == typeof(float):
                    result = UMI3DSerializer.Read<float>(container);
                    return true;
                case true when type == typeof(long):
                    result = UMI3DSerializer.Read<long>(container);
                    return true;
                case true when type == typeof(ulong):
                    result = UMI3DSerializer.Read<ulong>(container);
                    return true;
            }

            result = null;
            readable = false;
            return false;
        }

        bool Write(object value, Type type, out Bytable bytable, params object[] parameters)
        {
            switch (true)
            {
                case true when type == typeof(byte):
                    bytable = UMI3DSerializer.Write((byte)value, parameters);
                    return true;
                case true when type == typeof(short):
                    bytable = UMI3DSerializer.Write((short)value, parameters);
                    return true;
                case true when type == typeof(ushort):
                    bytable = UMI3DSerializer.Write((ushort)value, parameters);
                    return true;
                case true when type == typeof(int):
                    bytable = UMI3DSerializer.Write((int)value, parameters);
                    return true;
                case true when type == typeof(uint):
                    bytable = UMI3DSerializer.Write((uint)value, parameters);
                    return true;
                case true when type == typeof(float):
                    bytable = UMI3DSerializer.Write((float)value, parameters);
                    return true;
                case true when type == typeof(long):
                    bytable = UMI3DSerializer.Write((long)value, parameters);
                    return true;
                case true when type == typeof(ulong):
                    bytable = UMI3DSerializer.Write((ulong)value, parameters);
                    return true;
            }

            bytable = null;
            return false;
        }

    }
}