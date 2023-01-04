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

    public abstract class InitedWindow<T> : EditorWindow where T : InitedWindow<T>
    {
        const string filename = "InitedWindowData";
        private ScriptableLoader<InitedWindowData> data;

        /// <summary>
        /// Open T window.
        /// </summary>
        static protected void OpenWindow(string name = null,bool canReload = true)
        {
            T window = (T)EditorWindow.GetWindow(typeof(T), false, name ?? typeof(T).Name);
            window.data = new ScriptableLoader<InitedWindowData>(filename);

            var type = typeof(T).FullName;
            (bool canReload, bool showMessage, bool lastValue) memory = (true, true, true);
            if (window.data.data.dontShowCantreloadMessage.ContainsKey(type))
            {
                memory = window.data.data.dontShowCantreloadMessage[type];
            }
            memory.lastValue = memory.showMessage;
            memory.canReload = canReload;
            window.data.data.dontShowCantreloadMessage[type] = memory;

            window.Show();
        }

        /// <summary>
        /// value is reset to false when editor is recompiling.
        /// </summary>
        bool inited = false;


        /// <summary>
        /// force call of Init.
        /// </summary>
        public bool ReInit()
        {
            inited = false;
            return _Init();
        }

        /// <summary>
        /// call init if inited is false.
        /// </summary>
        bool _Init()
        {
            if (!inited || data == null)
            {
                data = new ScriptableLoader<InitedWindowData>(filename);
                var type = typeof(T).FullName;
                if (data.data.dontShowCantreloadMessage.ContainsKey(type))
                {
                    if (!data.data.dontShowCantreloadMessage[type].canReload)
                    {
                        if (data.data.dontShowCantreloadMessage[type].lastShowMessageValue)
                        {
                            EditorGUILayout.LabelField("This window cannot be reloaded. This may be due to a recompilation of the scripts.");
                            var memo = data.data.dontShowCantreloadMessage[type];
                            memo.showMessage = !EditorGUILayout.Toggle(memo.showMessage, "don't show again");
                            data.data.dontShowCantreloadMessage[type] = memo;
                            if (GUILayout.Button("Close"))
                                Close();
                        }
                        else
                            Close();
                        return false;
                    }
                }
                Init();
                inited = true;
            }
            return true;
        }

        /// <summary>
        /// Called by unity to draw ui.
        /// </summary>
        protected void OnGUI()
        {
            if (_Init())
                Draw();
        }

        /// <summary>
        /// Called when the windows need to be inited or the editor recompiled.
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// Draw the GUI of the window.
        /// This is called by OnGUI.
        /// </summary>
        protected abstract void Draw();
    }
}
#endif