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
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace inetum.unityUtils.editor
{

    public static class SerializedPropertyExtension
    {
        /// <summary>
        /// Return the UnSerialized Class containing this property using reflection.
        /// based on https://answers.unity.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetParent(this SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        /// <summary>
        /// 
        /// 
        ///  based on https://answers.unity.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        /// <summary>
        /// 
        /// 
        ///  based on https://answers.unity.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
    }
}
#endif