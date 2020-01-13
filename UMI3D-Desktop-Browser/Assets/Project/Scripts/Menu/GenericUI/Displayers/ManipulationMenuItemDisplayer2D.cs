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
using UnityEngine.Events;
using UnityEngine.UI;
using umi3d.cdk.menu.view;

namespace BrowserDesktop.Menu
{
    /// <summary>
    /// 2D Displayer for action menu item.
    /// </summary>

    public class ManipulationMenuItemDisplayer2D : AbstractManipulationMenuItemDisplayer, IMenuItemAnchor
    {

        /// <summary>
        /// Selection event subscribers.
        /// </summary>
        private List<UnityAction<bool>> subscribers = new List<UnityAction<bool>>();

        public bool select = false;

        /// <summary>
        /// Button
        /// </summary>
        public Button button;

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
            rect.localPosition = anchor + new Vector3(Vector2.Dot(direction, Vector2.right) * size.x / 2, Vector2.Dot(direction, Vector2.up) * size.y / 2);
        }

        private void Update()
        {
            RectTransform rect = GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            if (size != this.size)
            {
                this.size = size;
                rect.localPosition = anchor + new Vector3(Vector2.Dot(direction, Vector2.right) * size.x / 2, Vector2.Dot(direction, Vector2.up) * size.y / 2);
            }
        }

        public override void Clear()
        {
            button.onClick.RemoveListener(NotifyPress);
        }

        public override void Display(bool forceUpdate = false)
        {
            button.gameObject.SetActive(true);
            button.onClick.AddListener(NotifyPress);
        }

        public override void Hide()
        {
            button.onClick.RemoveListener(NotifyPress);
            button.gameObject.SetActive(false);
        }

        void NotifyPress()
        {
            ChangeSelectState(!select);
        }

        /// <summary>
        /// Raise selection event.
        /// </summary>
        public override void Select()
        {
            foreach (UnityAction<bool> sub in subscribers)
                sub.Invoke(true);
        }

        public override void Deselect()
        {
            foreach (UnityAction<bool> sub in subscribers)
                sub.Invoke(false);
        }

        /// <summary>
        /// Select if the argument is true, deselect it otherwise.
        /// </summary>
        /// <param name="select"></param>
        public void ChangeSelectState(bool select)
        {
            this.select = select;
            if (select)
                Select();
            else
                Deselect();
        }

        /// <summary>
        /// Subscribe a callback to the selection event.
        /// </summary>
        /// <param name="callback">Callback to raise on selection</param>
        /// <see cref="UnSubscribe(UnityAction)"/>
        public override void Subscribe(UnityAction<bool> callback)
        {
            subscribers.Add(callback);
        }

        /// <summary>
        /// Unsubscribe a callback from the selection event.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction)"/>
        public override void UnSubscribe(UnityAction<bool> callback)
        {
            subscribers.Remove(callback);
        }

    }
}