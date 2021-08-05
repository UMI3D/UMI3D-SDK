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
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace inetum.unityUtils
{
    public class ConstEnumAttribute : PropertyAttribute
    {
        public Type type;
        public string[] options;
        public object[] values;
        public Action<object, object> writer;
        public Func<object, object> reader;

        //public ConstEnumAttribute(Type type)
        //{


        //    this.type = type;
        //    values = options = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        //                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
        //                    .Select(fi => fi.GetValue(null) as string).ToArray();
        //}

        public ConstEnumAttribute(Type type, Type valueType)
        {
            switch (true)
            {
                case true when valueType == typeof(uint):
                    this.writer = UintWriter;
                    this.reader = UintReader;
                    break;
                case true when valueType == typeof(string):
                    this.writer = stringWriter;
                    this.reader = stringReader;
                    break;
                default:
                    throw new Exception($"{valueType} is not a valid type");
            }

            this.type = type;
            var val = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .Where(fi => fi.IsLiteral && !fi.IsInitOnly);
            values = val.Select(fi => fi.GetValue(null)).ToArray();
            options = val.Select(fi => fi.Name).ToArray();
        }

        static Action<object, object> UintWriter
        {
            get =>
            (a, b) =>
            {
#if UNITY_EDITOR
                if (a is SerializedProperty property && b is uint value)
                {
                    property.intValue = (int)value;
                }
#endif
            };
        }

        static Func<object, object> UintReader
        {
            get =>
            (a) =>
            {
#if UNITY_EDITOR
                if (a is SerializedProperty property)
                {
                    return (uint)property.intValue;
                }
#endif
                return null;
            };
        }

        static Action<object, object> stringWriter
        {
            get =>
            (a, b) =>
            {
#if UNITY_EDITOR
                if (a is SerializedProperty property && b is string value)
                {
                    property.stringValue = value;
                }
#endif
            };
        }

        static Func<object, object> stringReader
        {
            get =>
            (a) =>
            {
#if UNITY_EDITOR
                if (a is SerializedProperty property)
                {
                    return property.stringValue;
                }
#endif
                return null;
            };
        }

    }
}