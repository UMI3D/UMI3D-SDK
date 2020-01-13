/*
Copyright 2019 Gfi Informatique

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
using UnityEngine.Events;

namespace umi3d
{
    /// <summary>
    /// Interface for observable objects containing a value.
    /// </summary>
    /// <typeparam name="T">Type of the value contained</typeparam>
    public interface IObservable<T> 
    {
        /// <summary>
        /// Get contained value.
        /// </summary>
        T GetValue();

        /// <summary>
        /// Notify a value change.
        /// </summary>
        /// <param name="newValue">New value</param>
        void NotifyValueChange(T newValue);

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(UnityAction{T})"/>
        void Subscribe(UnityAction<T> callback);

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction{T})"/>
        void UnSubscribe(UnityAction<T> callback);
    }

}