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
using System.Collections.Generic;
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    /// <summary>
    /// Abstract base class for all <see cref="MenuItem"/> related to enum inputs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractEnumInputMenuItem<T> : AbstractInputMenuItem<T>
    {
        /// <summary>
        /// Input's possible values.
        /// </summary>
        public List<T> options = new List<T>();

        /// <summary>
        /// Input value.
        /// </summary>
        private T value;

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<UnityAction<T>> subscribers = new List<UnityAction<T>>();

        /// <summary>
        /// Get displayed value.
        /// </summary>
        public override T GetValue()
        {
            return value;
        }

        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(T newValue)
        {
            value = newValue;
            foreach (UnityAction<T> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(UnityAction{T})"/>
        public override void Subscribe(UnityAction<T> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }


        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction{T})"/>
        public override void UnSubscribe(UnityAction<T> callback)
        {
            subscribers.Remove(callback);
        }
    }
}