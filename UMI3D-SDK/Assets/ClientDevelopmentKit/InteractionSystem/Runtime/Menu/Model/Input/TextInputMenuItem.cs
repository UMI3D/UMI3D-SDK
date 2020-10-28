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
using System.Collections.Generic;
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    /// <summary>
    /// Menu item for text input.
    /// </summary>
    public class TextInputMenuItem : AbstractInputMenuItem<string>
    {
        /// <summary>
        /// Input value.
        /// </summary>
        private string value = "";

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private List<UnityAction<string, string>> subscribers = new List<UnityAction<string, string>>();

        public override string GetValue()
        {
            return value;
        }

        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(string newValue, string hoveredObjectId)
        {
            value = newValue;
            foreach (UnityAction<string, string> sub in subscribers)
            {
                sub.Invoke(newValue, hoveredObjectId);
            }
        }



        /// <summary>
        /// Subscribe a callback for input value change.
        /// </summary>
        /// <param name="callback">Callback to invoke on input value change</param>
        public override void Subscribe(UnityAction<string, string> callback)
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
        public override void UnSubscribe(UnityAction<string, string> callback)
        {
            subscribers.Remove(callback);
        }

        public override string ToString()
        {
            return Name;
        }

    }
}