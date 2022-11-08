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

using System;

namespace umi3d.common
{
    /// <summary>
    /// Interface for objects containing a value that is subscribed upon.
    /// </summary>
    /// <typeparam name="T">Type of the value contained.</typeparam>
    /// <seealso cref="IPublisher"/>
    public interface IPublisher<T>
    {
        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(Action{T})"/>
        bool Subscribe(Action<T> callback);

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(Action{T})"/>
        bool UnSubscribe(Action<T> callback);
    }

    /// <summary>
    /// Interface for objects containing that can be subscribed.
    /// </summary>
    /// <seealso cref="IPublisher{T}"/>
    public interface IPublisher
    {
        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(Action)"/>
        bool Subscribe(Action callback);

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(Action)"/>
        bool UnSubscribe(Action callback);
    }
}
