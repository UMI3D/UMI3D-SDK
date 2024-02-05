/*
Copyright 2019 - 2023 Inetum

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

namespace inetum.unityUtils
{
    /// <summary>
    /// A container that notifies its observers when its value change.
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    [Serializable]
    public class NotifyingVariable<Type>
    {
        /// <summary>
        /// Event raised when the value changed.
        /// </summary>
        public event Action<Type, Type> valueChanged;
        /// <summary>
        /// same as <see cref="valueChanged"/> but call the new registered Action immediately.
        /// </summary>
        public event Action<Type, Type> valueChangedCallImmediately
        {
            add
            { 
                valueChanged += value;
                value?.Invoke(oldField, field);
            }
            remove
            {
                valueChanged -= value;
            }
        }

        [SerializeField]
        protected Type field;
        protected Type oldField;

        /// <summary>
        /// Get or set the value.
        /// </summary>
        public Type value
        {
            get
            {
                return field;
            }
            set
            {
                oldField = field;
                field = value;
                valueChanged?.Invoke(oldField, field);
            }
        }

        /// <summary>
        /// Dereference the old variable.
        /// </summary>
        public void DereferenceOldValue()
        {
            oldField = default;
        }

        public override string ToString()
        {
            return field.ToString();
        }
    }
}
