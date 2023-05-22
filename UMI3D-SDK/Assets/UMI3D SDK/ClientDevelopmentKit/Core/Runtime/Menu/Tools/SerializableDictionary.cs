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
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk.menu.view
{
    /// <summary>
    /// Serializable implementation of a enumerable collection of key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">type of the keys.</typeparam>
    /// <typeparam name="TValue">type of the values.</typeparam>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// List of keys.
        /// </summary>
        [SerializeField, Tooltip("List of keys.")]
        private List<TKey> keys = new List<TKey>();
        /// <summary>
        /// List of values.
        /// </summary>
        [SerializeField, Tooltip("List of values.")]
        private List<TValue> values = new List<TValue>();


        public int Count => keys.Count;


        /// <summary>
        /// Add a key-value entry.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        public void Add(TKey key, TValue value)
        {
            keys.Add(key);
            values.Add(value);
        }


        /// <summary>
        /// Remove an entry.
        /// </summary>
        /// <param name="key">Key to remove</param>
        public void Remove(TKey key)
        {
            if (keys.Contains(key))
            {
                int index = keys.IndexOf(key);
                keys.RemoveAt(index);
                values.RemoveAt(index);
            }
        }

        /// <summary>
        /// Try to get a key-value entry and sets the value parameter if the key has been found.
        /// </summary>
        /// <param name="key">Key to find</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if the key has been found, false otherwise</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (keys.Contains(key))
            {
                value = values[keys.IndexOf(key)];
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var dic = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dic.Add(keys[i], values[i]);
            }

            return dic.GetEnumerator();
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var dic = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dic.Add(keys[i], values[i]);
            }

            return dic.GetEnumerator();
        }
    }

    [System.Serializable]
    public class ContainerDictionary : SerializableDictionary<string, AbstractMenuDisplayContainer> { }
}