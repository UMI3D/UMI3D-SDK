/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using umi3d.cdk.menu.core;
using umi3d.cdk.menu.view;

namespace BrowserDesktop.Menu
{
    [RequireComponent(typeof(Button))]
    public class MenuItemButtonDisplayer : AbstractDisplayer
    {
        /// <summary>
        /// Button attached to this object.
        /// </summary>
        private Button button;

        public override void Clear()
        {
        }

        public override void Display(bool forceUpdate = false)
        {
            this.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public override void SetMenuItem(AbstractMenuItem item)
        {
            button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = item.Name;
        }

        private void Awake()
        {
            button = this.GetComponent<Button>();
        }

        public override int IsSuitableFor(umi3d.cdk.menu.core.AbstractMenuItem menu)
        {
            return (menu is MenuItem) ? 1 : 0;
        }
    }
}