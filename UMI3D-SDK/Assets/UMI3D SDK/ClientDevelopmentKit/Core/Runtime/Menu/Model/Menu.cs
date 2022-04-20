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
    public class Menu : AbstractMenu
    {
        public List<MenuItem> MenuItems = new List<MenuItem>();
        public List<Menu> SubMenu = new List<Menu>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="abstractMenuItem">Menu or menuItem to add</param>
        public override bool Add(AbstractMenuItem abstractMenuItem)
        {
            if (AddWithoutNotify(abstractMenuItem))
            {
                if (abstractMenuItem is Menu menu)
                    menu.onContentChange.AddListener(onContentChange.Invoke);
                onAbstractMenuItemAdded.Invoke(abstractMenuItem);
                onContentChange.Invoke();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="abstractMenuItem"></param>
        public override bool AddWithoutNotify(AbstractMenuItem abstractMenuItem)
        {
            if (Contains(abstractMenuItem))
                return false;

            if (abstractMenuItem is MenuItem menuItem)
            {
                MenuItems.Add(menuItem);
                return true;
            }
            else if (abstractMenuItem is Menu menu)
            {
                SubMenu.Add(menu);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="abstractMenuItem"></param>
        /// <returns></returns>
        public override bool Remove(AbstractMenuItem abstractMenuItem)
        {
            if (RemoveWithoutNotify(abstractMenuItem))
            {
                OnAbstractMenuItemRemoved?.Invoke(abstractMenuItem);
                onContentChange.Invoke();
                if (abstractMenuItem is Menu menu)
                    menu.onContentChange.RemoveAllListeners();
                return true;
            }
            else
                return false;
        }

        public void Destroy()
        {
            onAbstractMenuItemAdded.RemoveAllListeners();
            OnAbstractMenuItemRemoved.RemoveAllListeners();
            OnDestroy.Invoke();
            OnDestroy.RemoveAllListeners();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="abstractMenuItem"></param>
        /// <returns></returns>
        public override bool RemoveWithoutNotify(AbstractMenuItem abstractMenuItem)
        {
            if (!Contains(abstractMenuItem))
                return false;

            if (abstractMenuItem is MenuItem menuItem)
                return MenuItems.Remove(menuItem);
            else if (abstractMenuItem is Menu menu)
                return SubMenu.Remove(menu);
            else
                return false;
        }

        ///<inheritdoc/>
        public override bool Contains(AbstractMenuItem abstractMenuItem)
        {
            if (abstractMenuItem is MenuItem menuItem)
                return MenuItems.Contains(menuItem);
            else if (abstractMenuItem is Menu menu)
                return SubMenu.Contains(menu);
            else
                return false;
        }

        ///<inheritdoc/>
        public override int Count => MenuItems.Count + SubMenu.Count;

        ///<inheritdoc/>
        public override IEnumerable<AbstractMenuItem> GetItems()
        {
            var items = new List<AbstractMenuItem>();
            foreach (MenuItem item in GetMenuItems())
                items.Add(item);
            foreach (Menu item in GetSubMenu())
                items.Add(item);
            return items;
        }

        /// <summary>
        /// Menu items contained in this menu.
        /// </summary>
        public override IEnumerable<AbstractMenuItem> GetMenuItems()
            => MenuItems;

        /// <summary>
        /// Submenus contained in this menu.
        /// </summary>
        public override IEnumerable<AbstractMenu> GetSubMenu()
            => SubMenu;

        

        ///<inheritdoc/>
        public override string ToString()
            => Name;
    }
}