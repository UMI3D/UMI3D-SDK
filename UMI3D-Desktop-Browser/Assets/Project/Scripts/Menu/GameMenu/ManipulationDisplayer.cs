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
using TMPro;

namespace BrowserDesktop.Menu
{
    public class ManipulationDisplayer : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI label = null;
        [SerializeField]
        Image icon = null;
        [SerializeField]
        Image ActiveBackground = null;
        [SerializeField]
        Image NotActiveBackground = null;

        public void Set(string label, Sprite icon)
        {
            this.label.text = label;
            if (icon != null)
            {
                this.icon.enabled = true;
                this.icon.sprite = icon;
            }
            else
                this.icon.enabled = false;
        }

        public void State(bool active)
        {
            ActiveBackground.enabled = active;
            NotActiveBackground.enabled = !active;
        }

    }
}