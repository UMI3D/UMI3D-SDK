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

using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using inetum.unityUtils;

namespace umi3d.common
{
    [UMI3DSerializerIgnore]
    public interface UMI3DSerializerModule
    {
        /// <summary>
        /// Write the object as a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">argumentType of the object to serialize.</typeparam>
        /// <param name="value">Object to serialize.</param>
        /// <param name="bytable">Object as a bytable.</param>
        /// <returns></returns>
        bool Write<T>(T value, out Bytable bytable, params object[] parameters);

        /// <summary>
        /// Retrieve an object from a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">argumentType of the object to deserialize.</typeparam>
        /// <param name="container">Byte container containing the object.</param>
        /// <param name="readable">has the containr successfully been read?</param>
        /// <param name="result">Deserialized object.</param>
        /// <returns></returns>
        bool Read<T>(ByteContainer container, out bool readable, out T result);

        /// <summary>
        /// state is class if countable or not 
        /// </summary>
        /// <typeparam name="T">Type to test</typeparam>
        /// <returns>True if countable, false if not, null if module doesn'argumentType know</returns>
        bool? IsCountable<T>();
    }

    /// <summary>
    /// Helper class to serialize objects.
    /// </summary>
    /// Typically used to serialize objects that are not defined in the UMI3D core.
    public interface UMI3DSerializerModule<C>
    {

        /// <summary>
        /// Write the object as a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">argumentType of the object to serialize.</typeparam>
        /// <param name="value">Object to serialize.</param>
        /// <param name="bytable">Object as a bytable.</param>
        /// <returns></returns>
        bool Write(C value, out Bytable bytable, params object[] parameters);

        /// <summary>
        /// Retrieve an object from a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">argumentType of the object to deserialize.</typeparam>
        /// <param name="container">Byte container containing the object.</param>
        /// <param name="readable">has the containr successfully been read?</param>
        /// <param name="result">Deserialized object.</param>
        /// <returns></returns>
        bool Read(ByteContainer container, out bool readable, out C result);


        /// <summary>
        /// state is class if countable or not 
        /// </summary>
        /// <typeparam name="T">Type to test</typeparam>
        /// <returns>True if countable, false if not, null if module doesn'argumentType know</returns>
        bool IsCountable();

    }



    ///// <summary>
    ///// Helper class to serialize objects.
    ///// </summary>
    ///// Typically used to serialize objects that are not defined in the UMI3D core.
    //public abstract class UMI3DSerializerModule : IUMI3DSerializerModule
    //{
    //    /// <summary>
    //    /// Write the object as a <see cref="Bytable"/>.
    //    /// </summary>
    //    /// <typeparam name="T">argumentType of the object to serialize.</typeparam>
    //    /// <param name="value">Object to serialize.</param>
    //    /// <param name="bytable">Object as a bytable.</param>
    //    /// <returns></returns>
    //    public abstract bool Write<T>(T value, out Bytable bytable, params object[] parameters);

    //    /// <summary>
    //    /// Retrieve an object from a <see cref="Bytable"/>.
    //    /// </summary>
    //    /// <typeparam name="T">argumentType of the object to deserialize.</typeparam>
    //    /// <param name="container">Byte container containing the object.</param>
    //    /// <param name="readable">has the containr successfully been read?</param>
    //    /// <param name="result">Deserialized object.</param>
    //    /// <returns></returns>
    //    public abstract bool Read<T>(ByteContainer container, out bool readable, out T result);


    //    /// <summary>
    //    /// state is class if countable or not 
    //    /// </summary>
    //    /// <typeparam name="T">Type to test</typeparam>
    //    /// <returns>True if countable, false if not, null if module doesn'argumentType know</returns>
    //    public abstract bool? IsCountable<T>();

    //}
    public static class UMI3DSerializerModuleUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetModulesType(Assembly assembly = null)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => assembly == null || a == assembly)
                .SelectMany(a => a.GetTypes())
                .Where(type => type.IsValidType() && !type.IsIgnoreAttribute())
                .OrderByDescending(type =>
                {
                    var attributes = type.GetCustomAttributes(typeof(UMI3DSerializerOrderAttribute), true);
                    if (type.GetCustomAttributes(typeof(UMI3DSerializerOrderAttribute), true).Length > 0)
                    {
                        return attributes.Select(a => a as UMI3DSerializerOrderAttribute).First().order;
                    }
                    return -1;
                });
        }

        static bool IsValidType(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract
            && type.GetTypeInfo().IsClass
            && (
                type.GetInterfaces().Contains(typeof(UMI3DSerializerModule))
                || type.GetInterfaces()
                        .Any(i =>
                            i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(UMI3DSerializerModule<>
               )));
        }

        static bool IsIgnoreAttribute(this Type type)
        {
            var a = type.GetCustomAttributes(typeof(UMI3DSerializerIgnoreAttribute), false);
            if(a.Length  > 0)
            {
                return a.Any(a => (a as UMI3DSerializerIgnoreAttribute).ignore);
            }
            return false;
        }

        public static IEnumerable<UMI3DSerializerModule> GetModules(Assembly assembly = null)
        {



            return GetModulesType(assembly).SelectMany(Create);
        }

        static IEnumerable<UMI3DSerializerModule> Create(Type type)
        {
            //**/UnityEngine.Debug.Log(type);
            var l = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(UMI3DSerializerModule<>))
                .Select(i => new UMI3DSerializerContainer(type,i, i.GetGenericArguments().First())).Cast<UMI3DSerializerModule>().ToList();

            if (type.GetInterfaces().Contains(typeof(UMI3DSerializerModule)))
                l.Add(Activator.CreateInstance(type) as UMI3DSerializerModule);

            //UnityEngine.Debug.Log(type +" " + type.GetInterfaces().ToString(t => (t == typeof(UMI3DSerializerModule)).ToString()) +" "+ type.GetInterfaces().Contains(typeof(UMI3DSerializerModule)) +" => " + l.ToString<UMI3DSerializerModule>());
            return l;

        }
    }

    [UMI3DSerializerIgnore]
    public class UMI3DSerializerContainer : UMI3DSerializerModule
    {
        public Type type;
        public object module;
        MethodInfo methodIsCountable;
        MethodInfo methodRead;
        MethodInfo methodWrite;

        public UMI3DSerializerContainer(Type type, Type interfaceType, Type argumentType)
        {
            this.type = argumentType;
            //UnityEngine.Debug.Log(type + " " + interfaceType + " " + interfaceType.GetMethods().ToString<MethodInfo>());
            module = Activator.CreateInstance(type);
            methodIsCountable = GetImplementedMethod(type, interfaceType.GetMethod("IsCountable"));
            methodRead = GetImplementedMethod(type, interfaceType.GetMethod("Read"));
            methodWrite = GetImplementedMethod(type, interfaceType.GetMethod("Write"));
        }

        static MethodInfo GetImplementedMethod(Type targetType, MethodInfo interfaceMethod)
        {
            if (targetType is null) throw new ArgumentNullException(nameof(targetType));
            if (interfaceMethod is null) throw new ArgumentNullException(nameof(interfaceMethod));

            var map = targetType.GetInterfaceMap(interfaceMethod.DeclaringType);
            var index = Array.IndexOf(map.InterfaceMethods, interfaceMethod);
            if (index < 0) return null;

            return map.TargetMethods[index];

        }

        public bool? IsCountable<T>()
        {
            if (type.IsAssignableFrom(typeof(T)))
                return (bool?)methodIsCountable.Invoke(module, new object[0]);
            return null;
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = false;
            result = default;
            if (type.IsAssignableFrom(typeof(T)))
            {
                object[] parameters = new object[] { container, null, null };

                var res = (bool)methodRead.Invoke(module, parameters);
                readable = (bool)parameters[1];
                result = (T)parameters[2];
                return res;
            }
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {

            bytable = default;
            if (type.IsAssignableFrom(typeof(T)))
            {
                object[] p = new object[] { value, null, parameters };
                var res = (bool)methodWrite.Invoke(module, p);
                bytable = (Bytable)p[1];
                return res;
            }
            return false;
        }
    }

    public class UMI3DSerializerOrderAttribute : Attribute
    {
        public int order;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order"></param>
        public UMI3DSerializerOrderAttribute(int order = 0)
        {
            this.order = order;
        }
    }

    public class UMI3DSerializerIgnoreAttribute : Attribute
    {
        public bool ignore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order"></param>
        public UMI3DSerializerIgnoreAttribute(bool ignore = true)
        {
            this.ignore = ignore;
        }
    }
}