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

namespace umi3d.cdk.menu
{
    //[System.Serializable]
    public class Menu : AbstractMenu
    {
        //[SerializeField]
        public List<MenuItem> MenuItems = new List<MenuItem>();

        //[SerializeField]
        public List<Menu> SubMenu = new List<Menu>();

        /// <summary>
        /// Add a menu item to this menu.
        /// </summary>
        /// <param name="menuItem">Menu item to add</param>
        public override void Add(AbstractMenuItem menuItem)
        {
            if (menuItem is MenuItem)
            {
                MenuItems.Add(menuItem as MenuItem);
                onContentChange.Invoke();
                onAbstractMenuItemAdded.Invoke(menuItem);
            }
            else if (menuItem is Menu)
            {
                SubMenu.Add(menuItem as Menu);
                (menuItem as Menu).onContentChange.AddListener(() => onContentChange.Invoke());
                onContentChange.Invoke();
                onAbstractMenuItemAdded.Invoke(menuItem);
            }
        }

        ///<inheritdoc/>
        public override bool Contains(AbstractMenuItem menuItem)
        {
            if (menuItem is MenuItem)
            {
                return MenuItems.Contains(menuItem as MenuItem);
            }
            else if (menuItem is Menu)
            {
                return SubMenu.Contains(menuItem as Menu);
            }
            return false;
        }

        ///<inheritdoc/>
        public override int Count => MenuItems.Count + SubMenu.Count;

        ///<inheritdoc/>
        public override IEnumerable<AbstractMenuItem> GetItems()
        {
            var items = new List<AbstractMenuItem>();
            foreach (MenuItem item in GetMenuItems())
            {
                items.Add(item);
            }
            foreach (Menu item in GetSubMenu())
            {
                items.Add(item);
            }
            return items;
        }

        /// <summary>
        /// Menu items contained in this menu.
        /// </summary>
        public override IEnumerable<AbstractMenuItem> GetMenuItems()
        {
            return MenuItems;
        }

        /// <summary>
        /// Submenus contained in this menu.
        /// </summary>
        public override IEnumerable<AbstractMenu> GetSubMenu()
        {
            return SubMenu;
        }

        /// <summary>
        /// Remove a menu item from this menu.
        /// </summary>
        /// <param name="menuItem">Menu item to remove</param>
        public override bool Remove(AbstractMenuItem menuItem)
        {
            bool result = false;
            if (Contains(menuItem))
            {
                if (menuItem is MenuItem)
                {
                    menuItem.OnDestroy.Invoke();
                    menuItem.OnDestroy.RemoveAllListeners();
                    result = MenuItems.Remove(menuItem as MenuItem);
                    onContentChange.Invoke();
                }
                else if (menuItem is Menu)
                {
                    menuItem.OnDestroy.Invoke();
                    menuItem.OnDestroy.RemoveAllListeners();
                    result = SubMenu.Remove(menuItem as Menu);
                    onContentChange.Invoke();
                }
            }
            return result;
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}