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

using inetum.unityUtils;
using System.Collections;
using UnityEngine;

namespace umi3d.common
{
    public class PersistentSingleton<T> : QuittingManager where T : PersistentSingleton<T>
    {
        const DebugScope scope = DebugScope.Common | DebugScope.Core;

        private static T instance;

        /// <summary>
        /// State if an instance of <typeparamref name="T"/> exist.
        /// </summary>
        public static bool Exists => !ApplicationIsQuitting && instance != null;

        /// <summary>
        /// static rteference to the only instance of <typeparamref name="T"/>
        /// </summary>
        public static T Instance
        {
            get
            {
                if (ApplicationIsQuitting)
                {
                    return null;
                }
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        var g = GameObject.Find(typeof(T).Name);
                        if (g)
                        {
                            instance = g.GetComponent<T>();
                        }
                        else
                        {
                            g = new GameObject
                            {
                                name = typeof(T).Name
                            };
                            instance = g.AddComponent<T>();
                        }
                    }
                }
                return instance;
            }
            set
            {
                if (instance == null) instance = value;
                else UMI3DLogger.LogError("Instance of " + typeof(T) + " already exist, Instance could not be set",scope);
            }
        }

        /// <summary>
        /// Unity function call when a MonoBehaviour is created. Make sure to always call base.Awake() when overriding.
        /// </summary>
        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                if (instance.gameObject.name == gameObject.name)
                    UMI3DLogger.LogWarning("There is already a Singleton<" + typeof(T) + "> , instance on " + gameObject.name + " will be exterminated. This could occur after reloaded a scene with a PersistentSingleton in it",scope);
                else
                    UMI3DLogger.LogError("There is already a Singleton<" + typeof(T) + "> , instance on " + gameObject.name + " will be exterminated.",scope);
                Destroy(this);
            }
            else
            {
                instance = this as T;
                DontDestroyOnLoad(this);
            }
        }

        new public static Coroutine StartCoroutine(IEnumerator enumerator) => Exists ? (Instance as MonoBehaviour).StartCoroutine(enumerator) : null;
        new public static void StopCoroutine(Coroutine coroutine)
        {
            if (Exists)
                (Instance as MonoBehaviour).StopCoroutine(coroutine);
        }


        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}