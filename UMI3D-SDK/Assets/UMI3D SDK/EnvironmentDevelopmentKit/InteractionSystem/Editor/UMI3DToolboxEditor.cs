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
        /// <summary>
        /// Serialized property which referes to <see cref="Toolbox.tools"/>.
        /// </summary>
        private SerializedProperty subTools;

        /// <summary>
        /// Inspector displayer for <see cref="subTools"/>.
        /// </summary>
        private readonly ListDisplayer<GlobalTool> ListDisplayer = new ListDisplayer<GlobalTool>();


        private static bool showList = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            subTools = _target.FindProperty("tools");

            //ListDisplayer.onObjectRemoved.AddListener(RemoveParent);
        }

        private void OnDisable()
        {
            //ListDisplayer.onObjectRemoved.RemoveListener(RemoveParent);
        }

        ///// <summary>
        ///// If a <see cref="GlobalTool"/> is removed from the list, resets its parent.
        ///// </summary>
        ///// <param name="tool"></param>
        //private void RemoveParent(object tool)
        //{
        //    if (tool is GlobalTool globalTool && globalTool.objectparent.GetValue() == target)
        //    {
        //        globalTool.objectparent.SetValue(target);
        //    }
        //}

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void _OnInspectorGUI()
        {
            base._OnInspectorGUI();
            if (target is Toolbox toolbox)
                ListDisplayer.Display(ref showList, subTools, toolbox.editorTools,
                    t =>
                    {
                        switch (t)
                        {
                            case GlobalTool i:
                                //i.Setparent(toolbox);
                                return new List<GlobalTool>() { i };
                            case GameObject g:
                                var tools = g.GetComponents<GlobalTool>().ToList();
                                //tools.ForEach(tool => tool.Setparent(toolbox));
                                return tools;
                            default:
                                return null;
                        }
                    });
        }
    }
}

#endif