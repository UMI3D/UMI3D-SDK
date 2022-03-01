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

using System.Collections.Generic;
using System.Linq;
using umi3d.edk.interaction;
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(Toolbox), true)]
    public class UMI3DToolboxEditor : UMI3DAbstractToolEditor
    {
        private SerializedProperty subTools;
        private ListDisplayer<GlobalTool> ListDisplayer = new ListDisplayer<GlobalTool>();

        protected override void OnEnable()
        {
            base.OnEnable();
            subTools = _target.FindProperty("tools");
        }

        private static bool showList = true;
        protected override void _OnInspectorGUI()
        {
            base._OnInspectorGUI();
            ListDisplayer.Display(ref showList, subTools, ((Toolbox)target).tools,
                t =>
                {
                    switch (t)
                    {
                        case GlobalTool i:
                            i.parent = (Toolbox)target;
                            return new List<GlobalTool>() { i };
                        case GameObject g:
                            List<GlobalTool> tools = g.GetComponents<GlobalTool>().ToList();
                            tools.ForEach(tool => tool.parent = (Toolbox)target);
                            return tools;
                        default:
                            return null;
                    }
                });
        }
    }
}

#endif