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

namespace umi3d.cdk.menu.interaction
{
    public class EventMenuItem : InteractionMenuItem
    {
        /// <summary>
        /// If true, represents an holdable event.
        /// </summary>
        public bool hold = false;

        private bool value = false;

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<UnityAction<bool>> subscribers = new List<UnityAction<bool>>();



        /// <summary>
        /// Subscribe a callback for button press.
        /// </summary>
        /// <param name="callback">Callback to invoke on button press</param>
        public virtual void Subscribe(UnityAction<bool> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback"></param>
        public virtual void UnSubscribe(UnityAction<bool> callback)
        {
            subscribers.Remove(callback);
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual bool GetValue()
        {
            return value;
        }

        /// <summary>
        /// Setter for <see cref="value"/>.
        /// </summary>
        /// <param name="newValue">New value</param>
        /// <param name="forceRaiseEvent">If true, will notify every suscriber, even if <see cref="value"/> is already equal to <paramref name="newValue"/>.</param>
        public virtual void NotifyValueChange(bool newValue)
        {
            value = newValue;

            foreach (UnityAction<bool> sub in subscribers)
            {
                sub.Invoke(value);
            }
        }
    }
}