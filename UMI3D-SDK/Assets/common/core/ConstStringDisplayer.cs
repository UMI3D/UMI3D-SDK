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

#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace umi3d.common.editor
{
    public class ConstStringDisplayer
    {
        string label;
        int index;
        string[] options;

        public ConstStringDisplayer(string label, Type type, string value = null)
        {
            this.label = label;
            options = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetValue(null) as string).ToArray();
            if (value == null) index = 0;
            else
            {
                index = Array.IndexOf(options, value);
                if (index == -1) index = 0;
            }
        }

        public string display()
        {
            index = EditorGUILayout.Popup(label,index, options);
            if (index >= options.Length)
                return "none";
            return options[index];
        }

    }
}
#endif