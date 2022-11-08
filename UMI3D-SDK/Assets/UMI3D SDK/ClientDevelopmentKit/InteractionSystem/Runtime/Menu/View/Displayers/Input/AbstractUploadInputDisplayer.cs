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
using umi3d.common;
using UnityEngine.Events;

namespace umi3d.cdk.menu.view
{
    /// <summary>
    /// Base class for <see cref="UploadInputMenuItem"/> display.
    /// </summary>
    public abstract class AbstractUploadInputDisplayer : AbstractInputMenuItemDisplayer<string>, common.IObservable<string>
    {
        /// <summary>
        /// Menu item to display.
        /// </summary>
        protected UploadInputMenuItem menuItem;

        /// <summary>
        ///  Only these extensions could be upload by the client. Enpty list = allow all, the extensions contain a dot (".obj" for exemple)
        /// </summary>
        public List<string> authorizedExtensions = new List<string>();

        /// <summary>
        /// IObservable subscribers.
        /// </summary>
        /// <see cref="IObservable{T}"/>
        private readonly List<Action<string>> subscribers = new List<Action<string>>();

        /// <summary>
        /// Get displayed value.
        /// </summary>
        public string GetValue()
        {
            return menuItem.GetValue();
        }

        /// <summary>
        /// Notify a value change.
        /// </summary>
        /// <param name="newValue">New value</param>
        public void NotifyValueChange(string newValue)
        {
            menuItem.NotifyValueChange(newValue);
            foreach (Action<string> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(Action{string})"/>
        public bool Subscribe(Action<string> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(Action{string})"/>
        public bool UnSubscribe(Action<string> callback)
        {
            return subscribers.Remove(callback);
        }

        /// <summary>
        /// Set menu item to display and initialise the display.
        /// </summary>
        /// <param name="item">Item to display</param>
        /// <returns></returns>
        public override void SetMenuItem(AbstractMenuItem item)
        {
            if (item is UploadInputMenuItem)
            {
                menuItem = item as UploadInputMenuItem;
            }
            else
            {
                throw new System.Exception("MenuItem must be an UploadInput");
            }
        }
    }
}