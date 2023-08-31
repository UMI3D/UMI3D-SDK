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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace inetum.unityUtils
{
    public class UMI3DObservable : IUMI3DObservable<(Type observer, string purpose)>
    {
        /// <summary>
        /// Structural equality for (<see cref="Type"/>, string).
        /// </summary>
        struct TypeAndPurposeEqualityComparer : IEqualityComparer<(Type observer, string purpose)>
        {
            public bool Equals((Type observer, string purpose) x, (Type observer, string purpose) y)
            {
                return x.observer.Equals(y.observer) && x.purpose == y.purpose;
            }

            public int GetHashCode((Type, string) obj)
            {
                return obj.GetHashCode();
            }
        }

        Dictionary<(Type observer, string purpose), int> observersAndPurposeToPriorities = new(new TypeAndPurposeEqualityComparer());
        SortedList<int, List<((Type observer, string purpose) key, Action action)>> prioritiesToActions = new();

        public IBaseUMI3DObservable<(Type observer, string purpose), Action> baseObservable
        {
            get; protected set;
        }

        public UMI3DObservable()
        {
            baseObservable = new UMI3DBaseObservable<(Type observer, string purpose), Action>(observersAndPurposeToPriorities, prioritiesToActions);
        }

        /// <summary>
        /// <inheritdoc>/>
        /// </summary>
        public IList<int> priorities
        {
            get
            {
                return baseObservable.priorities;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool TryGetPriority((Type observer, string purpose) key, out int priority)
        {
            return baseObservable.TryGetPriority(key, out priority);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool Subscribe((Type observer, string purpose) key, Action action, int priority = 0)
        {
            return baseObservable.Subscribe(key, action, priority);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Unsubscribe((Type observer, string purpose) key)
        {
            return baseObservable.Unsubscribe(key);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Notify()
        {
            for (int i = (this as IUMI3DObservable<(Type observer, string purpose)>).priorities.Count - 1; i >= 0; i--)
            {
                foreach (var item in prioritiesToActions[priorities[i]])
                {
                    try
                    {
                        item.action();
                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                }
            }
        }
    }
}
