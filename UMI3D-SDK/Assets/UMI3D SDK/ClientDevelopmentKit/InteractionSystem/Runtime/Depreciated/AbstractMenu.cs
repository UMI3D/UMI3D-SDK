#if !UMI3D_NEW_LABEL
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
    [Obsolete("This class or method will be removed when the new label feature will be activated.")]
    public class AbstractMenuItemAdded : UnityEvent<AbstractMenuItem> { }
    [Obsolete("This class or method will be removed when the new label feature will be activated.")]
    public class AbstractMenuItemRemoved : UnityEvent<AbstractMenuItem> { }


    [Obsolete("This class or method will be removed when the new label feature will be activated.")]
    /// <summary>
    /// Abstract class for menu.
    /// </summary>
    public abstract class AbstractMenu : AbstractMenuItem
    {
        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Is navigation allowed through this menu.
        /// </summary>
        public bool navigable = true;

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Event raised when menu content has been changed.
        /// </summary>
        public UnityEvent onContentChange = new UnityEvent();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Event raised when a sub menu has been added.
        /// </summary>
        public AbstractMenuItemAdded onAbstractMenuItemAdded = new AbstractMenuItemAdded();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Event raised when a sub-menu has been removed.
        /// </summary>
        public AbstractMenuItemRemoved OnAbstractMenuItemRemoved = new AbstractMenuItemRemoved();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Submenus contained in this menu.
        /// </summary>
        public abstract IEnumerable<AbstractMenu> GetSubMenu();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Menu items contained in this menu.
        /// </summary>
        public abstract IEnumerable<AbstractMenuItem> GetMenuItems();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Menu items and SubMenus contained in this menu.
        /// </summary>
        public abstract IEnumerable<AbstractMenuItem> GetItems();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Add a menu item to this menu.
        /// </summary>
        /// <param name="menuItem">Menu item to add</param>
        public abstract bool Add(AbstractMenuItem abstractMenuItem);

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Add a menu item to this menu without notify events.
        /// </summary>
        /// <param name="abstractMenuItem"></param>
        /// <returns></returns>
        public abstract bool AddWithoutNotify(AbstractMenuItem abstractMenuItem);

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Remove a menu or a menuItem from this menu.
        /// </summary>
        /// <param name="menuItem">item to remove</param>
        public abstract bool Remove(AbstractMenuItem menuItem);

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Remove a menu or menuItem from this menu without notify events.
        /// </summary>
        /// <param name="abstractMenu"></param>
        /// <returns></returns>
        public abstract bool RemoveWithoutNotify(AbstractMenuItem abstractMenu);

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Return true if the item is in collection.
        /// </summary>
        /// <param name="menuItem">item to check</param>
        public abstract bool Contains(AbstractMenuItem abstractMenuItem);

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Return the number of Items and sub-menus.
        /// </summary>
        public abstract int Count { get; }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Remove all sub menus from this menu.
        /// </summary>
        public virtual void RemoveAllSubMenu()
        {
            var menus = new List<AbstractMenu>(GetSubMenu());
            foreach (AbstractMenu sub in menus)
                Remove(sub);
        }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Remove all menu items from this menu.
        /// </summary>
        public virtual void RemoveAllMenuItem()
        {
            var menuItems = new List<AbstractMenuItem>(GetMenuItems());
            foreach (AbstractMenuItem item in menuItems)
                Remove(item);
        }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Remove all menu items from this menu.
        /// </summary>
        public virtual void RemoveAll()
        {
            var Items = new List<AbstractMenuItem>(GetItems());
            foreach (AbstractMenuItem item in Items)
                Remove(item);
        }
    }
}
#endif