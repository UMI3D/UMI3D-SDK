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
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    /// <summary>
    /// Input menu item for float range.
    /// </summary>
    public class FloatRangeInputMenuItem : AbstractRangeInputMenuItem<float>
    {

        /// <summary>
        /// Subscribers on value change.
        /// </summary>
        protected List<UnityAction<float>> subscribers = new List<UnityAction<float>>();


        /// <summary>
        /// Increments in available values (0 means continuous).
        /// </summary>
        public override float increment
        {
            get => inc;
            set => inc = Mathf.Abs(value);
        }

        /// <summary>
        /// Is the range continous (false is discrete) ?
        /// </summary>
        /// <see cref="increment"/>
        public override bool continuousRange => increment == 0;

        /// <summary>
        /// Get the value of the float MenuItem.
        /// </summary>
        /// <returns></returns>
        public override float GetValue()
        {
            return value;
        }



        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(float newValue)
        {
            if (increment > 0)
            {
                float tmp = Mathf.Max(newValue, min);
                tmp = Mathf.Min(tmp, max);
                tmp = min + increment * ((int)((tmp - min) / increment));
                value = tmp;
            }
            else
            {
                value = newValue;
            }

            foreach (UnityAction<float> sub in subscribers)
            {
                sub.Invoke(value);
            }
        }

        /// <summary>
        /// Add an action to the onChanged subscribers 
        /// </summary>
        /// <param name="callback">Action to Add( the Parameter is the value of the MenuItem )</param>
        public override void Subscribe(UnityAction<float> callback)
        {
            if (!subscribers.Contains(callback))
                subscribers.Add(callback);
        }

        /// <summary>
        /// Remove an action to the onChanged subscribers 
        /// </summary>
        /// <param name="callback">Action to Remove</param>
        public override void UnSubscribe(UnityAction<float> callback)
        {
            if (subscribers.Contains(callback))
                subscribers.Remove(callback);
        }
    }
}