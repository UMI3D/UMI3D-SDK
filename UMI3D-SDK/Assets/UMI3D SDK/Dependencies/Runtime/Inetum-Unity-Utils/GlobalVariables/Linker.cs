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

namespace inetum.unityUtils
{
    public class Linker<T>
    {
        /// <summary>
        /// Event that notifies subscribers when a variable is linked or unlinked
        /// </summary>
        public event Action<T, bool> linked
        {
            add
            {
                _linked += value;

                // If the variable is already set, invoke the event immediately
                if (isSet)
                {
                    _linked?.Invoke(variable, isSet);
                }
            }
            remove
            {
                _linked -= value;
            }
        }

        event Action<T, bool> _linked;

        /// <summary>
        /// The variable that is being linked/
        /// </summary>
        T variable;

        /// <summary>
        /// A boolean value that indicates whether the variable is set or not.
        /// </summary>
        bool isSet;

        /// <summary>
        /// Links or unlinks a variable and invokes the "linked" event
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="isSet"></param>
        public void Link(T variable, bool isSet = true)
        {
            this.variable = variable;
            this.isSet = isSet;
            _linked?.Invoke(variable, isSet); 
        }
    }

    public static class Linker
    {
        /// <summary>
        /// Static method that returns an instance of the <see cref="Linker{T}"/> class with the type parameter <typeparamref name="T"/>.<br/>
        /// <br/>
        /// If the instance has already been created return it else create it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Linker<T> Get<T>(string key)
        {
            // Create a string that is used as the key to access the global dictionary.
            string _key = $"{key}/{typeof(Linker<T>).FullName}";

            // Try to get an instance of the "Linker<T>" class from the global dictionary.
            if (!Global.TryGet(_key, out Linker<T> link, debug: false))
            {
                // If an instance doesn't exist, create a new instance and add it to the global dictionary.
                link = new();
                Global.Add(_key, link);
            }
            return link;
        }
    }
}