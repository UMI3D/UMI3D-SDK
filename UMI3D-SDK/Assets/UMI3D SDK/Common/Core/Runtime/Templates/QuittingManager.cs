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

using UnityEngine;
using UnityEngine.Events;

namespace umi3d.common
{
    public class QuittingManager : MonoBehaviour
    {
        private static bool applicationIsQuitting = false;
        /// <summary>
        /// Should be set to true when the application is quitting.
        /// </summary>
        public static bool ApplicationIsQuitting
        {
            get => applicationIsQuitting;
            set
            {
                if (applicationIsQuitting != value)
                {
                    applicationIsQuitting = value;
                    if (value)
                        OnApplicationIsQuitting.Invoke();
                }
            }
        }

        /// <summary>
        /// Should be set to true in the client and the server when they deal with quitting the application.
        /// </summary>
        public static bool ShouldWaitForApplicationToQuit = false;

        /// <summary>
        /// Raised when ApplicationIsQuitting is set to true.
        /// </summary>
        public static UnityEvent OnApplicationIsQuitting = new UnityEvent();

        protected virtual void OnApplicationQuit()
        {
            if (!ShouldWaitForApplicationToQuit)
                ApplicationIsQuitting = true;
        }
    }
}
