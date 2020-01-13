/*
Copyright 2019 Gfi Informatique

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
using System.Reflection;
using UnityEngine;

namespace umi3d.common
{
    public static class DtoUtility
    {

        //Binding flags to iterate over UMI3D Data Transfer Object fields.
        static BindingFlags fieldFlags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;

        //
        //  SERIALIZE
        #region serialize

        // Should be used to serialize UMI3D Data Transfer Object to json.
        /// <param name="dto">UMI3D Data Transfer Object to serialize.</param>
        public static string Serialize(UMI3DDto dto)
        {
            return PreSerialize(dto).ToString();
        }

        // Map a UMI3D Data Transfer Object to a serializable JSONObject.
        /// <param name="dto">UMI3D Data Transfer Object to serialize.</param>
        static JSONObject PreSerialize(UMI3DDto dto)
        {
            FieldInfo[] fields = dto.GetType().GetFields(fieldFlags);
            var jobj = new JSONObject(JSONObject.Type.OBJECT);
            foreach (var field in fields)
            {
                JSONObject jfield = PreSerializeField(field.GetValue(dto));
                jobj.AddField(field.Name, jfield);
            }
            return jobj;
        }

        // Map a field value of a UMI3D Data Transfer Object to a serializable JSONObject.
        /// <param name="dto">Field value. supports Umi3DDto, int, enums, float, double, decimal, string, IEnumerable</param>
        static JSONObject PreSerializeField(object fieldValue)
        {
            JSONObject jfield = null;
            if (fieldValue is UMI3DDto)
            {
                jfield = PreSerialize(fieldValue as UMI3DDto);
            }
            else if (fieldValue is int || fieldValue is Enum)
            {
                jfield = JSONObject.Create((int)fieldValue);
            }
            else if (fieldValue is float)
            {
                jfield = JSONObject.Create((float)fieldValue);
            }
            else if (fieldValue is double)
            {
                var d = (double)fieldValue;
                jfield = JSONObject.Create((float)d);
            }
            else if (fieldValue is bool)
            {
                jfield = JSONObject.Create((bool)fieldValue);
            }
            else if (fieldValue is string)
            {
                jfield = new JSONObject(JSONObject.Type.STRING);
                jfield.str = (string)fieldValue;
            }
            else if (fieldValue is IEnumerable)
            {
                jfield = PreSerializeIEnumerable(fieldValue as IEnumerable);
            }
            return jfield;
        }

        // Map a collection to a serializable JSONObject Array.
        /// <param name="dto">Field value. supports Umi3DDto, int, enums, float, double, decimal, string</param>
        static JSONObject PreSerializeIEnumerable(IEnumerable enumerable)
        {
            var jfield = new JSONObject(JSONObject.Type.ARRAY);
            foreach (var subentity in enumerable as IEnumerable)
            {
                if (subentity is UMI3DDto)
                {
                    jfield.Add(PreSerialize(subentity as UMI3DDto));
                }
                else if (subentity is int || subentity is Enum)
                {
                    jfield.Add((int)subentity);
                }
                else if (subentity is float || subentity is double || subentity is Decimal)
                {
                    jfield.Add((float)subentity);
                }
                else if (subentity is bool)
                {
                    jfield.Add((bool)subentity);
                }
                else if (subentity is string || subentity is String)
                {
                    jfield.Add((string)subentity);
                }
            }
            return jfield;
        }

        #endregion

        //
        //      DESERIALIZE
        #region deserialize

        // Should be used to deserialize a UMI3D Data Transfer Object from json.
        /// <param name="dto">a json serialized UMI3D Data Transfer Object.</param>
        public static object Deserialize(string dto)
        {
            var jobj = JSONObject.Create(dto);
            return Deserialize(jobj);
        }


        public static object Deserialize(JSONObject jobj)
        {
            var type = Type.GetType(jobj.GetField("Dtype").str);
            return Deserialize(jobj, type);
        }



        // deserialize a UMI3D Data Transfer Object from JSONObject.
        /// <param name="jobj">UMI3D Data Transfer Object to serialize.</param>
        public static object Deserialize(JSONObject jobj, Type type)
        {
            Type[] argumentTypes = new Type[] { typeof(JSONObject) };
            FieldInfo[] fields = type.GetFields(fieldFlags);
            ConstructorInfo stringConstructor = type.GetConstructor(new Type[] { });
            var res = stringConstructor.Invoke(new object[] { });
            foreach (var field in fields)
            {
                var arr = field.GetCustomAttributes(typeof(UMI3DDto.CustomProperty), false);
                UMI3DDto.CustomProperty customProperty = (arr == null || arr.Length == 0) ? null : (UMI3DDto.CustomProperty)arr[0];
                string fname = customProperty == null || customProperty.name == null || customProperty.name.Length == 0 ? field.Name : customProperty.name;

                if (!jobj.HasField(fname))
                    continue;
                var fieldValue = jobj.GetField(fname);
                if (customProperty != null && customProperty.type == UMI3DDto.CustomProperty.Type.DATE)
                {
                    var val = DateTime.Parse(fieldValue.str);
                    field.SetValue(res, val);
                }
                else if (typeof(UMI3DDto).IsAssignableFrom(field.FieldType))
                {
                    var subentity = Deserialize(fieldValue);
                    field.SetValue(res, subentity);
                }
                else if ((typeof(int).IsAssignableFrom(field.FieldType))
                    || (typeof(Enum).IsAssignableFrom(field.FieldType)))
                {
                    field.SetValue(res, ToInt(fieldValue));
                }
                else if ((typeof(float).IsAssignableFrom(field.FieldType))
                    || (typeof(double).IsAssignableFrom(field.FieldType))
                    || (typeof(Decimal).IsAssignableFrom(field.FieldType)))
                {
                    field.SetValue(res, fieldValue.f);
                }
                else if (typeof(bool).IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(res, fieldValue.b);
                }
                else if ((typeof(string).IsAssignableFrom(field.FieldType))
                    || (typeof(String).IsAssignableFrom(field.FieldType)))
                {
                    field.SetValue(res, fieldValue.str);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(res, DeserializeIEnumerable(fieldValue.list, field.FieldType));
                }
                else if (typeof(object) == field.FieldType)
                {
                    field.SetValue(res, fieldValue);
                }

                //ConstructorInfo stringConstructor = type.GetConstructor(argumentTypes);
                //return stringConstructor.Invoke(new object[] { jobj });
                //return stringConstructor.Invoke(new object[] { jobj });
            }
            return res;
        }

        // deserialize a JSONObject list.
        /// <param name="jobjs">list to deserialize.</param>
        /// <param name="type">result type. request a parameter A, and a Add(A entity) method.</param>
        static IEnumerable DeserializeIEnumerable(List<JSONObject> jobjs, Type type)
        {
            ConstructorInfo stringConstructor = type.GetConstructor(new Type[] { });
            var list = stringConstructor.Invoke(new object[] { });
            var add = type.GetMethod("Add");
            var entityType = type.GetGenericArguments()[0];
            foreach (var subentity in jobjs)
            {
                object res = null;

                if (subentity.IsObject)
                {
                    res = Deserialize(subentity);
                }
                else if (subentity.IsNumber)
                {
                    if (typeof(int).IsAssignableFrom(entityType))
                        res = (int)subentity.f;
                    else if (typeof(double).IsAssignableFrom(entityType))
                        res = (double)subentity.f;
                    else
                        res = subentity.f;
                }
                else if (subentity.IsBool)
                {
                    res = subentity.b;
                }
                else if (subentity.IsString)
                {
                    res = subentity.str;
                }
                add.Invoke(list, new object[] { res });
            }
            return list as IEnumerable;
        }

        // convert a JSONObject to integer
        /// <param name="field">The JSONObject to convert.</param>
        /// <param name="defaultValue">default value if the field is null or impossible to cast.</param>
        public static int ToInt(JSONObject field, int defaultValue = 0)
        {
            if (field.IsNumber)
                return (int)field.f;
            else if (field.IsString)
            {
                int res = defaultValue;
                bool success = int.TryParse(field.str, out res);
                if (success)
                    return res;
                else
                    return defaultValue;
            }
            else
                return defaultValue;
        }

        #endregion

    }
}
