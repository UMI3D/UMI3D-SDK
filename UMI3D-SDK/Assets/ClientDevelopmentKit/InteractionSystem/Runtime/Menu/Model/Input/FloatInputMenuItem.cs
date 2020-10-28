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
    public class FloatInputMenuItem : AbstractInputMenuItem<float>
    {

        /// <summary>
        /// Range current value.
        /// </summary>
        protected float value = 0;

        /// <summary>
        /// Subscribers on value change.
        /// </summary>
        protected List<UnityAction<float, string>> subscribers = new List<UnityAction<float, string>>();

        /// <summary>
        /// Get input value.
        /// </summary>
        /// <returns></returns>
        public override float GetValue()
        {
            return value;
        }

        /// <summary>
        /// Subscribe a callback for input value change.
        /// </summary>
        /// <param name="callback">Callback to invoke on input value change (argument is the new value and the hoveredObjectId)</param>
        public override void Subscribe(UnityAction<float, string> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }

        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(float newValue, string hoveredObjectId)
        {
            value = newValue;

            foreach (UnityAction<float, string> sub in subscribers)
            {
                sub.Invoke(value, hoveredObjectId);
            }
        }

        /// <summary>
        /// Remove an action from the subscribers
        /// </summary>
        /// <param name="callback"></param>
        public override void UnSubscribe(UnityAction<float, string> callback)
        {
            subscribers.Remove(callback);
        }

        /// <summary>
        /// Get the name of the MenuItem.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}