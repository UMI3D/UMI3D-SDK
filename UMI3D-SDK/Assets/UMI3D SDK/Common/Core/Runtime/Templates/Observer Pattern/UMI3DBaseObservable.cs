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
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Implementation of a BaseObservable.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Act"></typeparam>
    public class UMI3DBaseObservable<Key, Act> : IUMI3DBaseObservable<Key, Act>
    {
        public class Unsubscriber : IUMI3DUnsubscriberObservable<Key, Act>
        {
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public IUMI3DBaseObservable<Key, Act> baseObservable { get; protected set; }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public Key key { get; protected set; }

            /// <summary>
            /// Create an <see cref="Unsubscriber"/>.
            /// </summary>
            /// <param name="baseObservable"></param>
            /// <param name="key"></param>
            public Unsubscriber(IUMI3DBaseObservable<Key, Act> baseObservable, Key key)
            {
                this.baseObservable = baseObservable;
                this.key = key;
            }

            public void Dispose()
            {
                baseObservable.Unsubscribe(key);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IDictionary<Key, int> observersAndPurposeToPriorities
        {
            get;
            protected set;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IDictionary<int, List<(Key key, Act action)>> prioritiesToActions
        {
            get;
            protected set;
        }

        public UMI3DBaseObservable(IDictionary<Key, int> observersAndPurposeToPriorities, IDictionary<int, List<(Key key, Act action)>> prioritiesToActions)
        {
            this.observersAndPurposeToPriorities = observersAndPurposeToPriorities;
            this.prioritiesToActions = prioritiesToActions;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICollection<int> priorities
        {
            get
            {
                return prioritiesToActions.Keys;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="purpose"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool TryGetPriority(Key key, out int priority)
        {
            if (!observersAndPurposeToPriorities.ContainsKey(key))
            {
                priority = 0;
                return false;
            }
            else
            {
                priority = observersAndPurposeToPriorities[key];
                return true;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="unsubscriber"></param>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool Subscribe(out IDisposable unsubscriber, Key key, Act action, int priority = 0)
        {
            if (TryGetPriority(key, out int _priority))
            {
                // There is already an action registered with the key (observer, purpose).
                unsubscriber = null;
                return false;
            }
            else
            {
                observersAndPurposeToPriorities.Add(key, priority);

                if (!prioritiesToActions.ContainsKey(priority))
                {
                    prioritiesToActions.Add(priority, new() { (key, action) });
                }
                else
                {
                    prioritiesToActions[priority].Add((key, action));
                }

                unsubscriber = new Unsubscriber(this, key);
                return true;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Unsubscribe(Key key)
        {
            if (!TryGetPriority(key, out int priority))
            {
                // There is no actio with the key (observer, purpose) to unregister.
                return false;
            }
            else
            {
                observersAndPurposeToPriorities.Remove(key);

                prioritiesToActions.Remove(priority);

                return true;
            }
        }
    }
}
