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
    public interface IParameterInputSystem<Value> : IInputSystem
    {
        event Action<Value> performed;

        void Perform(Value value);

        /// <summary>
        /// Clear the events.
        /// </summary>
        void Clear();
    }

    public struct NullObjectParameterInput<Value> : IParameterInputSystem<Value>
    {
        public string id => nameof(NullObjectParameterInput<Value>);

        public event Action<Value> performed
        {
            add
            {
                UnityEngine.Debug.Log($"[NullObjectParameterInput] Warning: you are trying to register to an event for type: {typeof(Value)}.");
            }
            remove
            {
                UnityEngine.Debug.Log($"[NullObjectParameterInput] Warning: you are trying to unregister to an event for type: {typeof(Value)}.");
            }
        }

        public void Perform(Value value)
        {
            UnityEngine.Debug.Log($"[NullObjectParameterInput] Warning: you are trying to perform on an event for type: {typeof(Value)}.");
        }

        public void Clear()
        {
        }
    }
}