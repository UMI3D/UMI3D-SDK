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
using TMPro;
using UnityEngine.UI;
using umi3d.cdk.menu.view;

namespace BrowserDesktop.Menu
{
    public class DropDownInputDisplayer2D : AbstractDropDownInputDisplayer,IMenuItemAnchor
    {

        public TextMeshProUGUI label;

        /// <summary>
        /// Dropdown.
        /// </summary>
        public TMP_Dropdown dropdown;

        Vector3 anchor;
        Vector3 direction;

        Vector2 size = Vector2.zero;

        public void Set(Vector3 anchor, Vector3 direction)
        {
            this.anchor = anchor;
            this.direction = direction;
            size = Vector2.one;
            RectTransform rect = GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            LayoutRebuilder.MarkLayoutForRebuild(rect);
            rect.localPosition = ComputePosition(rect);
        }

        private void Update()
        {
            RectTransform rect = GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            if (size != this.size)
            {
                this.size = size;
                rect.localPosition = ComputePosition(rect);
            }
        }

        Vector3 ComputePosition(RectTransform rect)
        {
            Vector3 projection = new Vector3(Vector2.Dot(direction, Vector2.right), Vector2.Dot(direction, Vector2.up));
            if (projection.x < 0.5f && projection.x > -0.5f) projection.x = 0;
            else projection.x = Mathf.Sign(projection.x) * size.x / 2;
            if (projection.y < 0.5f && projection.y > -0.5f) projection.y = 0;
            else projection.y = Mathf.Sign(projection.y) * size.y / 2;
            return anchor + projection;
        }
        public override void Clear()
        {
            dropdown.onValueChanged.RemoveAllListeners();
        }

        public override void Display(bool forceUpdate = false)
        {
            dropdown.gameObject.SetActive(true);
            dropdown.ClearOptions();
            dropdown.AddOptions(menuItem.options);
            dropdown.value = menuItem.options.IndexOf(GetValue());
            dropdown.onValueChanged.AddListener((i) => NotifyValueChange(menuItem.options[i]));
            label.text = menuItem.ToString();
        }

        public override void Hide()
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.gameObject.SetActive(false);
        }
    }
}
