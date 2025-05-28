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

using System;

namespace umi3d.cdk.interaction
{
    public interface IUploadFileParameterInputSystem : IInputSystem
    {
        event Action<(string url, string id)> performed;

        void Perform(string url, string id);

        /// <summary>
        /// Clear the events.
        /// </summary>
        void Clear();
    }

    public struct NullObjectUploadFileParameterInput : IUploadFileParameterInputSystem
    {
        public string id => nameof(NullObjectUploadFileParameterInput);

        public event Action<(string url, string id)> performed
        {
            add
            {
                UnityEngine.Debug.Log($"[NullObjectUploadFileParameterInput] Warning: you are trying to register to an event.");
            }
            remove
            {
                UnityEngine.Debug.Log($"[NullObjectUploadFileParameterInput] Warning: you are trying to register to an event.");
            }
        }

        public void Perform(string url, string id)
        {
            UnityEngine.Debug.Log($"[NullObjectUploadFileParameterInput] Warning: you are trying to perform on an event.");
        }

        public void Clear()
        {
        }
    }
}