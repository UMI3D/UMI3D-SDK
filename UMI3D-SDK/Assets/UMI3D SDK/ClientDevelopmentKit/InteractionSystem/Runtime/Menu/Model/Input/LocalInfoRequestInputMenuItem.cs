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
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    /// <summary>
    /// Boolean input <see cref="MenuItem"/>.
    /// </summary>
    public class LocalInfoRequestInputMenuItem : AbstractInputMenuItem<LocalInfoRequestParameterValue> // (read,write)
    {
        /// <summary>
        /// Input read authorization value.
        /// </summary>
        private LocalInfoRequestParameterValue value = new LocalInfoRequestParameterValue(false, false);


        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<Action<bool>> readSubscribers = new List<Action<bool>>();

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<Action<LocalInfoRequestParameterValue>> subscribers = new List<Action<LocalInfoRequestParameterValue>>();

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<Action<bool>> writeSubscribers = new List<Action<bool>>();


        /// <summary>
        /// Get displayed value.
        /// </summary>
        public override LocalInfoRequestParameterValue GetValue()
        {
            return value;
        }

        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(LocalInfoRequestParameterValue newValue)
        {
            if (value.read != newValue.read)
            {
                foreach (Action<bool> sub in readSubscribers)
                {
                    sub.Invoke(newValue.read);
                }
            }
            if (value.write != newValue.write)
            {
                foreach (Action<bool> sub in writeSubscribers)
                {
                    sub.Invoke(newValue.write);
                }
            }
            foreach (Action<LocalInfoRequestParameterValue> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
            value = newValue;

        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(Action{bool})"/>
        public override bool Subscribe(Action<LocalInfoRequestParameterValue> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(Action{bool})"/>
        public override bool UnSubscribe(Action<LocalInfoRequestParameterValue> callback)
        {
            return subscribers.Remove(callback);
        }
    }
}