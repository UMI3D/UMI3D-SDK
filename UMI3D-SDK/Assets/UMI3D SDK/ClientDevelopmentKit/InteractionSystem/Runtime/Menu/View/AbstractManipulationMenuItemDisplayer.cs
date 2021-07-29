﻿/*
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
using umi3d.cdk.menu.interaction;
using umi3d.common;
using UnityEngine.Events;

namespace umi3d.cdk.menu.view
{
    /// <summary>
    /// Base class for manipulation menu item display.
    /// </summary>
    public abstract class AbstractManipulationMenuItemDisplayer : AbstractDisplayer, ITogglable
    {
        /// <summary>
        /// Manipulation menu item to display.
        /// </summary>
        public ManipulationMenuItem manipulationMenuItem { get { return menu as ManipulationMenuItem; } }
        UnityAction<bool> itemSubscriber;

        /// <summary>
        /// Set menu item to display and initialise the display.
        /// </summary>
        /// <param name="item">Item to display</param>
        /// <returns></returns>
        public override void SetMenuItem(AbstractMenuItem item)
        {
            if (item is ManipulationMenuItem)
            {
                if (manipulationMenuItem != null)
                    UnSubscribe(itemSubscriber);

                menu = item;
                itemSubscriber = x =>
                {
                    if (x)
                        manipulationMenuItem.Select();
                    else
                        manipulationMenuItem.Deselect();
                };
                Subscribe(itemSubscriber);
            }
            else
            {
                throw new System.Exception("Argument should be an instance of MediaMenuItem.");
            }
        }



        /// <summary>
        /// Selection event subscribers.
        /// </summary>
        private List<UnityAction<bool>> subscribers = new List<UnityAction<bool>>();

        /// <summary>
        /// Raise selection event.
        /// </summary>
        public override void Select()
        {
            foreach (UnityAction<bool> sub in subscribers)
                sub.Invoke(true);
            base.Select();
        }

        /// <summary>
        /// Raise Deselection event.
        /// </summary>
        public virtual void Deselect()
        {
            foreach (UnityAction<bool> sub in subscribers)
                sub.Invoke(false);
        }

        /// <summary>
        /// Subscribe a callback to the selection event.
        /// </summary>
        /// <param name="callback">Callback to raise on selection</param>
        /// <see cref="UnSubscribe(UnityAction)"/>
        public virtual void Subscribe(UnityAction<bool> callback)
        {
            subscribers.Add(callback);
        }

        /// <summary>
        /// Unsubscribe a callback from the selection event.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction)"/>
        public virtual void UnSubscribe(UnityAction<bool> callback)
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
            return (menu is ManipulationMenuItem) ? 1 : 0;
        }
    }
}