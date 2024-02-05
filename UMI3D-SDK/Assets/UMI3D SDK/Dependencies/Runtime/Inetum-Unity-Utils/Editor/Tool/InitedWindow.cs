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
            var memory = window.data.data.FromNameOrNew(type, true, true, true, false);
            memory.lastShowMessageValue = memory.showMessage;
            memory.canReload = canReload;
            memory.waitForInit = false;
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
            InitedWindowData.data memory = null;
            if (!inited || data == null)
            {

                data = new ScriptableLoader<InitedWindowData>(filename);
                var type = typeof(T).FullName;

                memory = data.data.FromName(type);

                if (memory != null)
                {
                    if (!memory.canReload)
                    {
                        if (memory.lastShowMessageValue)
                        {
                            EditorGUILayout.LabelField("This window cannot be reloaded. This may be due to a recompilation of the scripts.");
                            memory.showMessage = !EditorGUILayout.Toggle(memory.showMessage, "don't show again");
                            if (GUILayout.Button("Close"))
                                Close();
                        }
                        else
                            Close();
                        return false;
                    }
                }
                Init();
                if(memory != null && memory.waitForInit)
                {
                    memory.waitForInit = false;
                    Reinit();
                }
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
            {
                Draw();
            }
        }

        /// <summary>
        /// Called when the windows need to be inited or the editor recompiled.
        /// </summary>
        protected abstract void Init();


        /// <summary>
        /// Called when the windows need to be inited or the editor recompiled and the SetWaitForReinit is true.
        /// </summary>
        protected virtual void Reinit() { }

        /// <summary>
        /// Draw the GUI of the window.
        /// This is called by OnGUI.
        /// </summary>
        protected abstract void Draw();

        //[UnityEditor.Callbacks.DidReloadScripts]
        //private static void OnScriptsReloaded()
        //{

        //}


        /// <summary>
        /// Ask to call the Reinit method when the windows is inited
        /// </summary>
        /// <param name="value"><see langword="true"/>by default; if false the callback will not be called</param>
        public void SetWaitForReinit(bool value = true)
        {
            if (_Init())
            {
                var type = typeof(T).FullName;
                Debug.Log(type);
                data.data.Debug();
                var memory = data.data.FromName(type);
                if (memory != null)
                {
                    memory.waitForInit = value;
                }
                else
                    Debug.LogError("SetWaitForReinit failed because memory not inited");
            }
            else
                Debug.LogError("SetWaitForReinit failed because init failed");
        }

        /// <summary>
        /// Get the value set for WaitForReinit.
        /// </summary>
        /// <returns></returns>
        public bool GetWaitForReinit()
        {
            if (_Init())
            {
                var type = typeof(T).FullName;
                var memory = data.data.FromName(type);
                if (memory != null)
                {
                    return memory.waitForInit;
                }
            }
            return false;
        }

    }
}
#endif