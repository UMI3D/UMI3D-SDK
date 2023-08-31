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
    /// <summary>
    /// Base interface to describe an observable pattern template.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    public interface IBaseUMI3DObservable<Key, Act>
    {
        /// <summary>
        /// Store the priority of an action uniquely identifiable by its observer type and purpose.
        /// </summary>
        protected Dictionary<Key, int> observersAndPurposeToPriorities { get; }
        /// <summary>
        /// Store all the action info sorted by their priorities.
        /// </summary>
        protected SortedList<int, List<(Key key, Act action)>> prioritiesToActions { get; }

        /// <summary>
        /// The list of priorities.
        /// </summary>
        IList<int> priorities
        {
            get
            {
                return prioritiesToActions.Keys;
            }
        }

        /// <summary>
        /// Try to get the priority of an action uniquely identifiable by its <paramref name="key"/>.
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="purpose"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        bool TryGetPriority(Key key, out int priority)
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
        /// Register an action uniquely identifiable by its <paramref name="key"/>.
        /// 
        /// <para>
        /// Return true if there is not yet an action registered with that key.
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        bool Subscribe(Key key, Act action, int priority = 0)
        {
            if (TryGetPriority(key, out int _priority))
            {
                // There is already an action registered with the key (observer, purpose).
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

                return true;
            }
        }

        /// <summary>
        /// Unregister an action uniquely identifiable by its <paramref name="key"/>.
        /// 
        /// <para>
        /// Return false if there was no action registered with that key.
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Unsubscribe(Key key)
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

    /// <summary>
    /// Interface to describe an observable pattern template.
    /// 
    /// <para>
    /// The action take no argument here.
    /// </para>
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    public interface IUMI3DObservable<Key>
    {
        IBaseUMI3DObservable<Key, Action> baseObservable { get; }

        /// <summary>
        /// The list of priorities.
        /// </summary>
        IList<int> priorities { get; }

        /// <summary>
        /// Try to get the priority of an action uniquely identifiable by its <paramref name="key"/>.
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="purpose"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        bool TryGetPriority(Key key, out int priority);

        /// <summary>
        /// Register an action uniquely identifiable by its <paramref name="key"/>.
        /// 
        /// <para>
        /// Return true if there is not yet an action registered with that key.
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        bool Subscribe(Key key, Action action, int priority = 0);

        /// <summary>
        /// Unregister an action uniquely identifiable by its <paramref name="key"/>.
        /// 
        /// <para>
        /// Return false if there was no action registered with that key.
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Unsubscribe(Key key);

        /// <summary>
        /// Notify all the observer.
        /// </summary>
        void Notify();
    }

    /// <summary>
    /// Interface to describe an observable pattern template.
    /// 
    /// <para>
    /// The action take <typeparamref name="Arg"/> as argument.
    /// </para>
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    public interface IUMI3DObservable<Key, Arg> : IBaseUMI3DObservable<Key, Action<Arg>>
    {
        /// <summary>
        /// Notify all the observer.
        /// </summary>
        void Notify(Arg value);
    }
}
