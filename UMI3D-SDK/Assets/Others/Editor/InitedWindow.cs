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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class InitedWindow<T> : EditorWindow where T : InitedWindow<T>
{
    /// <summary>
    /// Open T window.
    /// </summary>
    static protected void OpenWindow()
    {
        T window = (T)EditorWindow.GetWindow(typeof(T));
        window.Show();
    }

    /// <summary>
    /// value is reset to false when editor is recompiling.
    /// </summary>
    bool inited = false;

    /// <summary>
    /// call init if inited is false.
    /// </summary>
    void _Init()
    {
        if (!inited)
        {
            inited = true;
            Init();
        }
    }

    /// <summary>
    /// Called by unity to draw ui.
    /// </summary>
    protected void OnGUI()
    {
        _Init();
        Draw();
    }

    /// <summary>
    /// Called when the windows need to be inited or the editor recompiled.
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// 
    /// </summary>
    protected abstract void Draw();
}
