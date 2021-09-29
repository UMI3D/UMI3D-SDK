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
    /// Boolean input menu item.
    /// </summary>
    public class BooleanInputMenuItem : AbstractInputMenuItem<bool>
    {
        /// <summary>
        /// Input value.
        /// </summary>
        private bool value = false;


        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private List<UnityAction<bool>> subscribers = new List<UnityAction<bool>>();


        /// <summary>
        /// Get displayed value.
        /// </summary>
        public override bool GetValue()
        {
            return value;
        }

        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(bool newValue)
        {
            value = newValue;
            foreach (UnityAction<bool> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(UnityAction{bool})"/>
        public override void Subscribe(UnityAction<bool> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction{bool})"/>
        public override void UnSubscribe(UnityAction<bool> callback)
        {
            subscribers.Remove(callback);
        }
    }
}