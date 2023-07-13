/*
Copyright 2019 - 2023 Inetum

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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace inetum.unityUtils.editor
{
    public abstract class UMI3DSpecialPropertyDrawer
    {
        public void OnGUI(Rect rect, SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            OnGUI_Internal(rect, property, new GUIContent(property.displayName));

            if (EditorGUI.EndChangeCheck())
            {
                // TODO : On value changed callback
            }

        }

        public float GetPropertyHeight(SerializedProperty property)
        {
            return GetPropertyHeight_Internal(property);
        }

        protected abstract void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label);
        protected abstract float GetPropertyHeight_Internal(SerializedProperty property);
    }

    public static class UMI3DSpecialDrawerAttributeExtensions
    {
        private static Dictionary<Type, UMI3DSpecialPropertyDrawer> _drawersByAttributeType;

        static UMI3DSpecialDrawerAttributeExtensions()
        {
            _drawersByAttributeType = new Dictionary<Type, UMI3DSpecialPropertyDrawer>();
            _drawersByAttributeType[typeof(TableListAttribute)] = TableListDrawer.Instance;
        }

        public static UMI3DSpecialPropertyDrawer GetDrawer(this UMI3DSpecialAttribute attr)
        {
            UMI3DSpecialPropertyDrawer drawer;
            if (_drawersByAttributeType.TryGetValue(attr.GetType(), out drawer))
            {
                return drawer;
            }
            else
            {
                return null;
            }
        }
    }
}
#endif