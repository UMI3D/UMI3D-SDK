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

namespace inetum.unityUtils.editor
{

    using UnityEditor;
    using UnityEngine;

    public interface ILogScrollViewData
    {
        string text { get; set; }
    }

    public class LogScrollView
    {
        ILogScrollViewData data;
        public string text { 
            get => data.text;
            set
            {
                data.text = value;
                if (data is Object obj)
                    EditorUtility.SetDirty(obj);
            }
        }
        Vector2 ScrollPos;

        readonly bool expand;
        public LogScrollView(ILogScrollViewData data,bool expand = true)
        {
            this.data = data;
            text = "";
            ScrollPos = Vector2.zero;
            this.expand = expand;
        }

        public void Clear()
        {
            text = "";
        }

        public void NewLine(string line)
        {
            text += line + "\n";
        }

        public void NewError(string line)
        {
            text += $"<color=red>Error : {line}</color>\n";
        }

        public void NewTitle(string line)
        {
            text += "\n";
            text += "-------------------------------------\n";
            text += line + "\n";
            text += "-------------------------------------\n";
            text += "\n";
        }

        public void Draw()
        {

            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, false, true, GUILayout.ExpandHeight(expand));


            GUI.enabled = false;
            EditorGUILayout.TextArea(text, GUILayout.ExpandHeight(true));
            GUI.enabled = true;
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif