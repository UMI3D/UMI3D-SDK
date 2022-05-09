/*
Copyright 2019 - 2022 Inetum

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

namespace inetum.unityUtils
{
    /// <summary>
    /// An implementation of the singleton template.
    /// </summary>
    /// <typeparam name="T">A Type that should have a public parameterless constructor</typeparam>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T instance;

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
                    new T();
                }
                return instance;
            }
        }

        protected Singleton()
        {
            if (instance != null && instance != this)
            {
                throw new System.Exception($"An instance of {typeof(T)} already exist");
            }
            else
            {
                instance = this as T;
            }
        }

        public static void Destroy()
        {
            if(Exists)
                Instance._Destroy();
            instance = null;
            
        }

        /// <summary>
        /// Called before setting the instance to null;
        /// </summary>
        protected virtual void _Destroy()
        {
        }
    }
}