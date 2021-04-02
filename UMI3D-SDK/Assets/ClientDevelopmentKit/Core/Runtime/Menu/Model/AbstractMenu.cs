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
    public class AbstractMenuItemAdded : UnityEvent<AbstractMenuItem> { }


    /// <summary>
    /// Abstract class for menu.
    /// </summary>
    public abstract class AbstractMenu : AbstractMenuItem
    {


        /// <summary>
        /// Is navigation allowed through this menu.
        /// </summary>
        public bool navigable = true;


        /// <summary>
        /// Event raised when menu content has been changed.
        /// </summary>
        public UnityEvent onContentChange = new UnityEvent();

        /// <summary>
        /// Event raised when menu content has been changed.
        /// </summary>
        public AbstractMenuItemAdded onAbstractMenuItemAdded = new AbstractMenuItemAdded();

        /// <summary>
        /// Submenus contained in this menu.
        /// </summary>
        public abstract IEnumerable<AbstractMenu> GetSubMenu();

        /// <summary>
        /// Menu items contained in this menu.
        /// </summary>
        public abstract IEnumerable<AbstractMenuItem> GetMenuItems();

        /// <summary>
        /// Menu items and SubMenus contained in this menu.
        /// </summary>
        public abstract IEnumerable<AbstractMenuItem> GetItems();


        /// <summary>
        /// Add a menu item to this menu.
        /// </summary>
        /// <param name="menuItem">Menu item to add</param>
        public abstract void Add(AbstractMenuItem menuItem);

        /// <summary>
        /// Remove an item from this menu.
        /// </summary>
        /// <param name="menuItem">item to remove</param>
        public abstract bool Remove(AbstractMenuItem menuItem);

        /// <summary>
        /// Return true if the item is in collection.
        /// </summary>
        /// <param name="menuItem">item to check</param>
        public abstract bool Contains(AbstractMenuItem menuItem);

        /// <summary>
        /// Return true if the item is in collection.
        /// </summary>
        /// <param name="menuItem">item to check</param>
        public abstract int Count { get; }

        /// <summary>
        /// Remove all sub menus from this menu.
        /// </summary>
        public virtual void RemoveAllSubMenu()
        {
            List<AbstractMenu> menus = new List<AbstractMenu>(GetSubMenu());
            foreach (AbstractMenu sub in menus)
                Remove(sub);
        }

        /// <summary>
        /// Remove all menu items from this menu.
        /// </summary>
        public virtual void RemoveAllMenuItem()
        {
            List<AbstractMenuItem> menuItems = new List<AbstractMenuItem>(GetMenuItems());
            foreach (AbstractMenuItem item in menuItems)
                Remove(item);
        }

        /// <summary>
        /// Remove all menu items from this menu.
        /// </summary>
        public virtual void RemoveAll()
        {
            List<AbstractMenuItem> Items = new List<AbstractMenuItem>(GetItems());
            foreach (AbstractMenuItem item in Items)
                Remove(item);
        }
    }
}
