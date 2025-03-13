/*
Copyright 2019 - 2025 Inetum

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

using UnityEngine;

namespace inetum.unityUtils.ui.canvas
{
    public interface IView 
    {
        bool Set<T>(ref T component, bool setEvenIfNotNull = false) where T : Component
        {
            if (this is not MonoBehaviour monoBehaviour)
            {
                UnityEngine.Debug.Log($"[IView] Error: this is not a MonoBehaviour");
                return false;
            }

            if (component == null || setEvenIfNotNull)
            {
                component = monoBehaviour.GetComponent<T>();
            }

            return component != null;
        }

        bool Set<T>(ref T component, int childIndex, bool setEvenIfNotNull = false) where T : Component
        {
            if (this is not MonoBehaviour monoBehaviour)
            {
                UnityEngine.Debug.Log($"[IView] Error: this is not a MonoBehaviour");
                return false;
            }

            if (component == null || setEvenIfNotNull)
            {
                component = monoBehaviour.transform.GetChild(childIndex).GetComponent<T>();
            }

            return component != null;
        }
    }
}