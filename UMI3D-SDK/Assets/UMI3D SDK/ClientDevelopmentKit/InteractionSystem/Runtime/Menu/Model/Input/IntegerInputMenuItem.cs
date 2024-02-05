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
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    public class IntegerInputMenuItem : AbstractInputMenuItem<int>
    {
        /// <summary>
        /// Integer value of the input menu item.
        /// </summary>
        private int value = 0;

        /// <summary>
        /// Subscribers on value change.
        /// </summary>
        protected List<Action<int>> subscribers = new List<Action<int>>();

        /// <summary>
        /// Subscribe a callback for input value change.
        /// </summary>
        /// <param name="callback">Callback to invoke on input value change</param>
        public override bool Subscribe(Action<int> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(int newValue)
        {
            value = newValue;

            foreach (Action<int> sub in subscribers)
            {
                sub.Invoke(value);
            }
        }

        /// <inheritdoc/>
        public override bool UnSubscribe(Action<int> callback)
        {
            return subscribers.Remove(callback);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public override int GetValue()
        {
            return value;
        }
    }
}