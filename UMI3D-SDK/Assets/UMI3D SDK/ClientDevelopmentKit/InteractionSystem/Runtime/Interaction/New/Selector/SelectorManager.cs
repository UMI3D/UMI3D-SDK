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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace umi3d.cdk.interaction
{
    public sealed class SelectorManager 
    {
        #region Initialize

        static Lazy<SelectorManager> _default = new(() => new());
        public static SelectorManager @default => _default.Value;

        SelectorManager()
        {
        }

        #endregion

        public ISelector serverSelector;

        public Selector lastSelectorUsed { get; internal set; }
        public Selector lastSelectorSelected { get; internal set; }
        public Selector lastSelectorDeselected { get; internal set; }

        List<Selector> _selectors = new List<Selector>();
        public ReadOnlyCollection<Selector> selectors => _selectors.AsReadOnly();

        /// <summary>
        /// Searches for an existing selector by its unique identifier. If a selector with the specified ID exists, 
        /// it is returned. Otherwise, a new selector is created, added to the internal list, and returned.<br/>
        /// <br/>
        /// <example>
        /// Given a selector ID, when calling this method, it will either return an existing selector 
        /// or create a new one and add it to the internal list.<br/>
        /// <br/>
        /// <code>
        /// bool isNew = selectorManager.InstantiateOrGet(out Selector selector, "selector_1");
        /// if (isNew)
        /// {
        ///     Console.WriteLine("A new selector was created.");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("An existing selector was returned.");
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="selector">
        /// The output parameter that will hold the existing or newly created selector.
        /// </param>
        /// <param name="id">
        /// The unique identifier of the selector to search for or associate with a new selector.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether a new selector was created (`true`) or an existing selector was found (`false`).
        /// </returns>
        public bool InstantiateOrGet(out Selector selector, string id)
        {
            selector = _selectors.Find(selector => selector.id == id);
            if (selector != null)
            {
                UnityEngine.Debug.Log($"[SelectorManager] Notice: selector for id: '{id}' already exist.");
                return false;
            }

            UnityEngine.Debug.Log($"[SelectorManager] Notice: selector for id: '{id}' created.");
            selector = new(id);
            _selectors.Add(selector);
            return true;
        }
    }
}