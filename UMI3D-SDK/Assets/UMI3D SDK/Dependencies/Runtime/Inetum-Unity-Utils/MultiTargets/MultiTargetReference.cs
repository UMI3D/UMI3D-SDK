/*
Copyright 2019 - 2024 Inetum

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

using System;
using UnityEngine;

namespace inetum.unityUtils.multiTarget
{
    [Serializable]
    public struct MultiTargetReference<T>
    {
#if UNITY_ANDROID || UNITY_EDITOR
        [SerializeField] T androidRef;
#endif
#if UNITY_IOS || UNITY_EDITOR                             
        [SerializeField] T iOSRef;
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR                                     
        [SerializeField] T windowsRef;
#endif
#if UNITY_STANDALONE_OSX || UNITY_EDITOR                                     
        [SerializeField] T macOSRef;
#endif
#if UNITY_WEBGL || UNITY_EDITOR                            
        [SerializeField] T WebglRef;
#endif

        public T Reference
        {
            get
            {
#if UNITY_ANDROID
                return androidRef;
#elif UNITY_IOS
                return iOSRef;
#elif UNITY_STANDALONE_WIN
                return windowsRef;
#elif UNITY_STANDALONE_OSX
                return macOSRef;
#elif UNITY_WEBGL
                return WebglRef;
#endif
            }
            set
            {
#if UNITY_ANDROID
                androidRef = value;
#elif UNITY_IOS
                iOSRef = value;
#elif UNITY_STANDALONE_WIN
                windowsRef = value;
#elif UNITY_STANDALONE_OSX
                macOSRef = value;
#elif UNITY_WEBGL
                WebglRef = value;
#endif
            }
        }
    }
}