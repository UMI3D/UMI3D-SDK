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
    public class LocalInfoRequestInputMenuItem : AbstractInputMenuItem<(bool,bool)> // (read,write)
    {
        /// <summary>
        /// Input read authorization value.
        /// </summary>
        private bool readValue = false;

        /// <summary>
        /// Input write authorization value.
        /// </summary>
        private bool writeValue = false;


        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private List<UnityAction<bool>> readSubscribers = new List<UnityAction<bool>>();

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private List<UnityAction<(bool,bool)>> subscribers = new List<UnityAction<(bool,bool)>>();

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private List<UnityAction<bool>> writeSubscribers = new List<UnityAction<bool>>();


        /// <summary>
        /// Get displayed value.
        /// </summary>
        public override (bool,bool) GetValue()
        {
            return (readValue,writeValue);
        }

        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange((bool,bool) newValue)
        {
            if(readValue != newValue.Item1)
            {
                foreach (UnityAction<bool> sub in readSubscribers)
                {
                    sub.Invoke(newValue.Item1);
                }
            }
            if (writeValue != newValue.Item2)
            {
                foreach (UnityAction<bool> sub in writeSubscribers)
                {
                    sub.Invoke(newValue.Item2);
                }
            }
            foreach (UnityAction<(bool,bool)> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
            (readValue, writeValue) = newValue;
            
        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(UnityAction{bool})"/>
        public override void Subscribe(UnityAction<(bool,bool)> callback)
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
        public override void UnSubscribe(UnityAction<(bool,bool)> callback)
        {
            subscribers.Remove(callback);
        }
    }
}