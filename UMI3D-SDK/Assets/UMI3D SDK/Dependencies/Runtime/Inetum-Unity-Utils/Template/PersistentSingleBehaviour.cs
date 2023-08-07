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

namespace inetum.unityUtils
{
    /// <summary>
    /// An implementation of the singleton template for MonoBehaviour.
    /// It is marked as persistent and therefore won't be destroyed on scene switching. 
    /// </summary>
    /// <typeparam name="T">A Type.</typeparam>
    /// <seealso cref="SingleBehaviour{T}"/>>
    public class PersistentSingleBehaviour<T> : SingleBehaviour<T> where T : PersistentSingleBehaviour<T>
    {
        /// <summary>
        /// This Method is initializing the singleton. This should be overrided with care.
        /// </summary>
        protected override bool _Awake()
        {
            if (base._Awake())
            {
                transform.SetParent(null);
                DontDestroyOnLoad(this);
                return true;
            }
            return false;
        }
    }
}