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
using System.Reflection;

namespace umi3d.common
{
    /// <summary>
    /// Helper class to serialize objects.
    /// </summary>
    /// Typically used to serialize objects that are not defined in the UMI3D core.
    public abstract class UMI3DSerializerModule
    {
        /// <summary>
        /// Write the object as a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">type of the object to serialize.</typeparam>
        /// <param name="value">Object to serialize.</param>
        /// <param name="bytable">Object as a bytable.</param>
        /// <returns></returns>
        public abstract bool Write<T>(T value, out Bytable bytable, params object[] parameters);

        /// <summary>
        /// Retrieve an object from a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">type of the object to deserialize.</typeparam>
        /// <param name="container">Byte container containing the object.</param>
        /// <param name="readable">has the containr successfully been read?</param>
        /// <param name="result">Deserialized object.</param>
        /// <returns></returns>
        public abstract bool Read<T>(ByteContainer container, out bool readable, out T result);


        public abstract bool IsCountable<T>();

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
                .Where(type => !type.GetTypeInfo().IsAbstract && type.IsSubclassOf(typeof(UMI3DSerializerModule)))
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

        public static IEnumerable<UMI3DSerializerModule> GetModules(Assembly assembly = null)
        {
            return GetModulesType(assembly).Select(t => Activator.CreateInstance(t) as UMI3DSerializerModule);
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
}