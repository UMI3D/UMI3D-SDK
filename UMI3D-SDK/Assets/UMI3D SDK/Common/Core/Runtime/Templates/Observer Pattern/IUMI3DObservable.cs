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

namespace umi3d.common
{
    public interface IUMI3DUnsubscriberObservable<Key, Act> : IDisposable
    {
        /// <summary>
        /// The base Observable.
        /// </summary>
        IUMI3DBaseObservable<Key, Act> baseObservable { get; }

        /// <summary>
        /// The key that will be used to unsubscribe.
        /// </summary>
        Key key { get; }
    }

    /// <summary>
    /// Base interface to describe an observable pattern template.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    public interface IUMI3DBaseObservable<Key, Act>
    {
        /// <summary>
        /// Store the priority of an action uniquely identifiable by its observer type and purpose.
        /// </summary>
        IDictionary<Key, int> observersAndPurposeToPriorities { get; }
        /// <summary>
        /// Store all the action info sorted by their priorities.
        /// </summary>
        IDictionary<int, List<(Key key, Act action)>> prioritiesToActions { get; }

        /// <summary>
        /// The list of priorities.
        /// </summary>
        ICollection<int> priorities { get; }

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
        /// <param name="unsubscriber"></param>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        bool Subscribe(out IDisposable unsubscriber, Key key, Act action, int priority = 0);

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
    }

    /// <summary>
    /// Interface to describe an observable pattern template.
    /// 
    /// <para>
    /// The action take no argument here.
    /// </para>
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    public interface IUMI3DObservable<Key> : IUMI3DBaseObservable<Key, Action>
    {
        /// <summary>
        /// Dependency injection of an <see cref="IUMI3DBaseObservable{Key, Act}"/>.
        /// </summary>
        IUMI3DBaseObservable<Key, Action> baseObservable { get; }

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
    public interface IUMI3DObservable<Key, Arg> : IUMI3DBaseObservable<Key, Action<Arg>>
    {
        /// <summary>
        /// Dependency injection of an <see cref="IUMI3DBaseObservable{Key, Act}"/>.
        /// </summary>
        IUMI3DBaseObservable<Key, Action> baseObservable { get; }

        /// <summary>
        /// Notify all the observer.
        /// </summary>
        void Notify(Arg value);
    }
}
