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

namespace DesktopUI
{

    public class ApplicationBarHeader : ThemeListener {

        public ApplicationBarTab tab;
        public Image background;
        public Text label;

        // Use this for initialization
        void Start()
        {
            var tabs = GetComponentInParent<ApplicationBar>();
            GetComponent<Button>().onClick.AddListener(() => tabs.ShowTab(tab));
        }

        private void Update()
        {
            if(background != null && tab != null)
                background.color = tab.gameObject.activeInHierarchy ? Theme.SecondaryColor : new Color(0,0,0,0);
            if (label != null && tab != null)
                label.color = tab.gameObject.activeInHierarchy ? Theme.SecondaryTextColor : Theme.PrincipalTextColor;
        }

        public override void ApplyTheme()
        {
            Update();
        }

    }

}