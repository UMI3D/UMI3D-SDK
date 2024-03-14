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

namespace umi3d.cdk.interaction
{
    public class UIInputAction : MonoBehaviour
    {
        public event Action<System.Object> performed;

        public System.Object value;
        public T ReadValue<T>()
        {
            if (value is not T)
            {
                throw new Exception($"[UMI3D] Input: input value is not {typeof(T).Name}");
            }
            return (T)value;
        }

        public void Perform(System.Object value)
        {
            this.value = value;
            performed?.Invoke(value);
        }
    }
}