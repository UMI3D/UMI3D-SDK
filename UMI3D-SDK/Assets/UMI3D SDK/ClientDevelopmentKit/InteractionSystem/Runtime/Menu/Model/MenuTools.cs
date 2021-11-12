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

namespace umi3d.cdk.menu
{
    public class MenuTools : MonoBehaviour
    {
        public MenuAsset menu;

        /// <summary>
        /// Add a BooleanInputMenuItem to the Menu.
        /// </summary>
        [ContextMenu("Add Bool input")]
        public void AddInputItem()
        {
            var menuItem = new BooleanInputMenuItem()
            {
                Name = "My Bool Input"
            };

            menu.menu.Add(menuItem);
        }

        /// <summary>
        /// Convert to a Menu. 
        /// </summary>
        [ContextMenu("Convert to 3D Menu")]
        public void ToMenu()
        {
            menu.menu = new umi3d.cdk.menu.Menu();
            menu.menu.RemoveAllMenuItem();
            menu.menu.RemoveAllSubMenu();
            ToMenuAux(menu.menu, this.transform);
            Debug.Log("Convertion done for " + menu.menu.Name);
            Debug.Log(new List<AbstractMenu>(menu.menu.GetSubMenu()).Count);

        }


        private void ToMenuAux(umi3d.cdk.menu.Menu menu_, Transform root)
        {
            foreach (Transform child in root.transform.GetComponentInChildren<Transform>())
            {
                if (child.childCount == 0)
                {
                    Debug.Log("add item");
                    var menuItem = new umi3d.cdk.menu.MenuItem();
                    menuItem.Name = child.gameObject.name;
                    menu_.Add(menuItem);
                }
                else
                {
                    var submenu = new umi3d.cdk.menu.Menu();
                    submenu.Name = child.gameObject.name;
                    menu_.Add(submenu);
                    ToMenuAux(submenu, child);
                }
            }
        }

    }
}