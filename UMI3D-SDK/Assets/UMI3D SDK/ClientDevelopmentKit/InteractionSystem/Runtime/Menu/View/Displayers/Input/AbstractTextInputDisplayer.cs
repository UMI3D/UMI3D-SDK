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
    public abstract class AbstractTextInputDisplayer : AbstractInputMenuItemDisplayer<string>, IObservable<string>
    {

        protected TextInputMenuItem menuItem;

        /// <summary>
        /// IObservable subscribers.
        /// </summary>
        /// <see cref="IObservable{T}"/>
        private readonly List<UnityAction<string>> subscribers = new List<UnityAction<string>>();
        /// <summary>
        /// Get displayed value.
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            return menuItem.GetValue();
        }
        /// <summary>
        /// Notify a value change.
        /// </summary>
        /// <param name="newValue"></param>
        public void NotifyValueChange(string newValue)
        {

            menuItem.NotifyValueChange(newValue);
            foreach (UnityAction<string> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
        }

        /// <summary>
        /// Set menu item to display and initialise the display.
        /// </summary>
        /// <param name="item"></param>
        public override void SetMenuItem(AbstractMenuItem item)
        {
            if (item is TextInputMenuItem)
            {
                menuItem = item as TextInputMenuItem;
            }
            else
            {
                throw new System.Exception("MenuItem must be a TextInput");
            }
        }
        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback"></param>
        public void Subscribe(UnityAction<string> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }
        /// <summary>
        /// nsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback"></param>
        public void UnSubscribe(UnityAction<string> callback)
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
            return (menu is TextInputMenuItem) ? 2 : 0;
        }
    }
}