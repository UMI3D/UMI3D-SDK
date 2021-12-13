﻿/*
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

using UnityEngine;
using inetum.unityUtils;
using System.Collections;

namespace umi3d.common
{
    public class Singleton<T> : QuittingManager where T : Singleton<T>
    {
        const DebugScope scope = DebugScope.Common | DebugScope.Core;

        /// <summary>
        /// static reference to the only instance of <typeparamref name="T"/>
        /// </summary>
        private static T instance;

        /// <summary>
        /// State if an instance of <typeparamref name="T"/> exist.
        /// </summary>
        public static bool Exists => !ApplicationIsQuitting && instance != null;

        /// <summary>
        /// static reference to the only instance of <typeparamref name="T"/>.
        /// This will instanciate an instance if null.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (ApplicationIsQuitting)
                    return null;
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
                UMI3DLogger.LogError("There is already a Singleton<" + typeof(T) + "> , instance on " + gameObject.name + " will be exterminated",scope);
                Destroy(this);
            }
            else
            {
                instance = this as T;
                CoroutineManager = new CoroutineManager(this, () => ApplicationIsQuitting);
                Breaker = new CoroutineManager.Breaker(0.1f);
            }
        }

        public static CoroutineManager CoroutineManager { get; protected set; }
        static CoroutineManager.Breaker Breaker { get; set; }
        new public static Coroutine StartCoroutine(IEnumerator enumerator) => CoroutineManager.Start(enumerator);
        new public static void StopCoroutine(Coroutine coroutine) => CoroutineManager.Stop(coroutine);
        public static System.Threading.Tasks.Task BreackTask => Breaker.Task;

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                CoroutineManager.Stop();
            }
        }
    }
}