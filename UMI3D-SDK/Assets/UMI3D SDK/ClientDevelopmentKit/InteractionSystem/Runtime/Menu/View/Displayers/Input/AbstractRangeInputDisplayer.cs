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
using umi3d.common;
using UnityEngine.Events;

namespace umi3d.cdk.menu.view
{
    /// <summary>
    /// Base class for range input display.
    /// </summary>
    public abstract class AbstractRangeInputDisplayer : AbstractInputMenuItemDisplayer<float>, IObservable<float>
    {
        /// <summary>
        /// Menu item displayed.
        /// </summary>
        protected FloatRangeInputMenuItem menuItem;

        /// <summary>
        /// IObservable subscribers.
        /// </summary>
        /// <see cref="IObservable{T}"/>
        private readonly List<UnityAction<float>> subscribers = new List<UnityAction<float>>();

        /// <summary>
        /// Get displayed value.
        /// </summary>
        public float GetValue()
        {
            return menuItem.GetValue();
        }

        /// <summary>
        /// Notify a value change.
        /// </summary>
        /// <param name="newValue">New value</param>
        public void NotifyValueChange(float newValue)
        {
            menuItem.NotifyValueChange(newValue);
            foreach (UnityAction<float> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
        }

        /// <summary>
        /// Set menu item to display and initialise the display.
        /// </summary>
        /// <param name="item">Item to display</param>
        /// <returns></returns>
        public override void SetMenuItem(AbstractMenuItem item)
        {
            if (item is FloatRangeInputMenuItem)
            {
                menuItem = item as FloatRangeInputMenuItem;
            }
            else
            {
                throw new System.Exception("MenuItem must be a Range Input");
            }
        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(UnityAction{bool})"/>
        public void Subscribe(UnityAction<float> callback)
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
        /// <see cref="Subscribe(UnityAction{bool})"/>
        public void UnSubscribe(UnityAction<float> callback)
        {
            subscribers.Remove(callback);
        }

        /// <summary>
        /// State is a the displayer is suitable for an AbstractMenuItem
        /// </summary>
        /// <param name="menu">The Menu to evaluate</param>
        /// <returns>Return a int. 0 and negative is not suitable, and int.MaxValue is the most suitable. </returns>
        public override int IsSuitableFor(umi3d.cdk.menu.AbstractMenuItem menu)
        {
            return (menu is FloatRangeInputMenuItem) ? 2 : 0;
        }
    }
}