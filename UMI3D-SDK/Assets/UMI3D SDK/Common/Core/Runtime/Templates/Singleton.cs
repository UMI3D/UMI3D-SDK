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

namespace umi3d.common
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// static reference to the only instance of <typeparamref name="T"/>
        /// </summary>
        static T instance;

        /// <summary>
        /// State if an instance of <typeparamref name="T"/> exist.
        /// </summary>
        public static bool Exists
        {
            get { return instance != null; }
        }

        static bool applicationIsQuitting = false;

        /// <summary>
        /// static reference to the only instance of <typeparamref name="T"/>.
        /// This will instanciate an instance if null.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                    return null;
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject g = GameObject.Find(typeof(T).Name);
                        if (g) instance = g.GetComponent<T>();
                        else
                        {
                            g = new GameObject();
                            g.name = typeof(T).Name;
                            instance = g.AddComponent<T>();
                        }
                    }
                }
                return instance;
            }
            set
            {
                if (instance == null) instance = value;
                else Debug.LogError("Instance of " + typeof(T) + " already exist, Instance could not be set");
            }
        }

        /// <summary>
        /// Unity function call when a MonoBehaviour is created. Make sure to always call base.Awake() when overriding.
        /// </summary>
        protected virtual void Awake()
        {
            applicationIsQuitting = false;
            if (instance != null && instance != this)
            {
                Debug.LogError("There is already a Singleton<" + typeof(T) + "> , instance on " + gameObject.name + " will be exterminated");
                Destroy(this);
            }
            else
            {
                instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

    }
}