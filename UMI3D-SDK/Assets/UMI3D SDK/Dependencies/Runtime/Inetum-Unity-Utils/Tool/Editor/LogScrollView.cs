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

namespace inetum.unityUtils
{

    using UnityEditor;
    using UnityEngine;

    public class LogScrollView
    {
        string info = "";
        Vector2 ScrollPos;
        bool lockDown;
        float? lastValue = null;
        public LogScrollView()
        {
            info = "";
            ScrollPos = Vector2.zero;
            lockDown = true;
        }

        public void Clear()
        {
            info = "";
        }

        public void NewLine(string line)
        {
            info += line + "\n";
        }

        public void NewError(string line)
        {
            info += $"<color=red>Error : {line}</color>\n";
        }

        public void NewTitle(string line)
        {
            info += "\n";
            info += "-------------------------------------\n";
            info += line + "\n";
            info += "-------------------------------------\n";
            info += "\n";
        }

        public void Draw()
        {
            //if (true )//&& lastValue != null && lockDown)
            //    ScrollPos.y = float.MaxValue;

            //Debug.Log($"A {ScrollPos.x} {ScrollPos.y} {lastValue} {lockDown}");

            //GUI.enabled = true;
            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

            //if (lastValue == null)
            //    lastValue = ScrollPos.y;
            //else if(lockDown)
            //{
            //    if (ScrollPos.y < lastValue)
            //        lockDown = false;
            //    lastValue = ScrollPos.y;
            //}

            //Debug.Log($"B {ScrollPos.x} {ScrollPos.y} {lastValue} {lockDown}");

            GUI.enabled = false;
            EditorGUILayout.TextArea(info);
            GUI.enabled = true;
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif