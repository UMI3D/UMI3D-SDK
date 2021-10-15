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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ClassDisplayer
{
    public static void Display(object obj)
    {
        if (obj == null) return;

        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            if (value == obj) continue;
            switch (value)
            {
                case UnityEngine.Object _Obj:
                    field.SetValue(obj, EditorGUILayout.ObjectField(field.Name, _Obj, field.FieldType, true));
                    break;
                case Bounds v:
                    field.SetValue(obj, EditorGUILayout.BoundsField(field.Name, v));
                    break;
                case BoundsInt v:
                    field.SetValue(obj, EditorGUILayout.BoundsIntField(field.Name, v));
                    break;
                case Color v:
                    field.SetValue(obj, EditorGUILayout.ColorField(field.Name, v));
                    break;
                case Color32 v:
                    field.SetValue(obj, EditorGUILayout.ColorField(field.Name, v));
                    break;
                case AnimationCurve v:
                    field.SetValue(obj, EditorGUILayout.CurveField(field.Name, v));
                    break;
                case double v:
                    field.SetValue(obj, EditorGUILayout.DoubleField(field.Name, v));
                    break;
                case System.Enum v:
                    field.SetValue(obj, EditorGUILayout.EnumFlagsField(field.Name, v));
                    break;
                case float v:
                    field.SetValue(obj, EditorGUILayout.FloatField(field.Name, v));
                    break;
                case Gradient v:
                    field.SetValue(obj, EditorGUILayout.GradientField(field.Name, v));
                    break;
                case int v:
                    field.SetValue(obj, EditorGUILayout.IntField(field.Name, v));
                    break;
                case LayerMask v:
                    field.SetValue(obj, EditorGUILayout.LayerField(field.Name, v));
                    break;
                case Rect v:
                    field.SetValue(obj, EditorGUILayout.RectField(field.Name, v));
                    break;
                case RectInt v:
                    field.SetValue(obj, EditorGUILayout.RectIntField(field.Name, v));
                    break;
                case string v:
                    field.SetValue(obj, EditorGUILayout.TextField(field.Name, v));
                    break;
                case Vector2 v:
                    field.SetValue(obj, EditorGUILayout.Vector2Field(field.Name, v));
                    break;
                case Vector2Int v:
                    field.SetValue(obj, EditorGUILayout.Vector2IntField(field.Name, v));
                    break;
                case Vector3 v:
                    field.SetValue(obj, EditorGUILayout.Vector3Field(field.Name, v));
                    break;
                case Vector3Int v:
                    field.SetValue(obj, EditorGUILayout.Vector3IntField(field.Name, v));
                    break;
                case Vector4 v:
                    field.SetValue(obj, EditorGUILayout.Vector4Field(field.Name, v));
                    break;
                case Quaternion v:
                    field.SetValue(obj, Quaternion.Euler(EditorGUILayout.Vector3Field(field.Name, v.eulerAngles)));
                    break;
                default:
                    if(value != null)
                    {
                        EditorGUI.indentLevel++;
                        Display(value);
                        EditorGUI.indentLevel--;
                    }
                    else
                        EditorGUILayout.LabelField(field.Name, obj.ToString());
                    break;
            }
        }

    }

}
#endif