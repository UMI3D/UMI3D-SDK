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
using UnityEngine;

namespace inetum.unityUtils
{
    /// <summary>
    /// An implementation of the singleton template for MonoBehaviour.
    /// </summary>
    /// <typeparam name="T">A Type.</typeparam>
    /// <seealso cref="PersistentSingleBehaviour{T}"/>>
    [DisallowMultipleComponent]
    public class SingleBehaviour<T> : MonoBehaviour where T : SingleBehaviour<T>
    {

        protected static T instance;

        /// <summary>
        /// State if the application is currently Quitting. 
        /// This is a direct reference to QuittingManager.ApplicationIsQuitting.
        /// </summary>
        /// <seealso cref="QuittingManager.ApplicationIsQuitting"/>>
        public static bool ApplicationIsQuitting => QuittingManager.ApplicationIsQuitting;

        /// <summary>
        /// State if an instance of <typeparamref name="T"/> exist.
        /// </summary>
        public static bool Exists => !ApplicationIsQuitting && instance != null;

        /// <summary>
        /// Static reference to the only instance of <typeparamref name="T"/>
        /// This will call MonoBehaviour method when Exist is false and therefore should not be called in an other thread in that case.
        /// It is safe to call it from an other thread in all other cases.
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
                else ReportLogError("Instance of " + typeof(T) + " already exist, Instance could not be set");
            }
        }

        /// <summary>
        /// Unity function call when a MonoBehaviour is created. Make sure to always call base.Awake() when overriding.
        /// </summary>
        protected virtual void Awake()
        {
            _Awake();
        }


        /// <summary>
        /// This Method is initializing the singleton. This should be overrided with care.
        /// </summary>
        protected virtual bool _Awake()
        {
            if (instance != null && instance != this)
            {
                if (instance.gameObject.name == gameObject.name)
                    ReportLogWarning("There is already a Singleton<" + typeof(T) + "> , instance on " + gameObject.name + " will be exterminated. This could occur after reloaded a scene with a PersistentSingleton in it");
                else
                    ReportLogError("There is already a Singleton<" + typeof(T) + "> , instance on " + gameObject.name + " will be exterminated.");
                Destroy(this);
                return false;
            }
            else
            {
                instance = this as T;
                return true;
            }
        }


        /// <summary>
        /// An override of StartCoroutine to make it static.
        /// This will call Instance.StartCoroutine when Exist is true.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return Exists ? (Instance as MonoBehaviour).StartCoroutine(enumerator) : null;
        }

        /// <summary>
        /// An override of StopCoroutine to make it static.
        /// This will call Instance.StopCoroutine when Exist is true.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static new void StopCoroutine(Coroutine coroutine)
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

        public static void Destroy()
        {
            if (Exists)
            {
                Instance._Destroy();
                GameObject.Destroy(Instance);
            }
            instance = null;
        }

        /// <summary>
        /// Called before setting the instance to null and destroying it;
        /// </summary>
        protected virtual void _Destroy()
        {
        }


        private static void ReportLogError(string s) { if (Exists) instance._LogError(s); }
        private static void ReportLogWarning(string s) { if (Exists) instance._LogWarning(s); }

        /// <summary>
        /// This is a call to UnityEngine.Debug.LogError that can be overrided.
        /// </summary>
        /// <param name="s"></param>
        protected virtual void _LogError(string s) { UnityEngine.Debug.LogError(s); }

        /// <summary>
        /// This is a call to UnityEngine.Debug.LogWarning that can be overrided.
        /// </summary>
        /// <param name="s"></param>
        protected virtual void _LogWarning(string s) { UnityEngine.Debug.LogWarning(s); }
    }
}