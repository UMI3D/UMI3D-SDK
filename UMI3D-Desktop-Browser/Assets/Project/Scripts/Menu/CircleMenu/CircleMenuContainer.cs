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
using umi3d.cdk.menu.view;
using umi3d.cdk.menu.core;
using UnityEngine.UI;
using UnityEngine;

namespace BrowserDesktop.Menu
{
    public class CircleMenuContainer : Simple2DContainer
    {

        public int radius;
        public int sameTimeDisplayable = 8;
        public int currentFirstDisplayed = 0;
        public GameObject View;
        public GameObject NextObject;
        public GameObject PreviousObject;
        public Button Next;
        public Button Previous;
        int count;

        private void Start()
        {
            Next.onClick.AddListener(_Next);
            Previous.onClick.AddListener(_Previous);
        }

        public override void Collapse(bool forceUpdate = false)
        {
            base.Collapse(forceUpdate);
            View.SetActive(false);
        }

        public override void Display(bool forceUpdate)
        {
            CircleMenu.Setup(this);
            transform.localPosition = Vector3.zero;
            base.Display(forceUpdate);
            currentFirstDisplayed = 0;
            OrganiseChildren();
        }

        public override void Expand(bool forceUpdate = false)
        {
            base.Expand(forceUpdate);
            currentFirstDisplayed = 0;
            OrganiseChildren();
            View.SetActive(true);
        }

        public override void ExpandAs(AbstractMenuDisplayContainer Container, bool forceUpdate = false)
        {
            base.ExpandAs(Container, forceUpdate);
            OrganiseChildren();
        }

        public void OrganiseChildren()
        {
            if (VirtualContainer == null)
                VirtualContainer = this;
            count = VirtualContainer.Count();
            if (count == 0) return;
            if (count > sameTimeDisplayable) {
                int diff = count - currentFirstDisplayed;
                if (diff < sameTimeDisplayable)
                {
                    count = diff + 1;
                }
                else
                    count = sameTimeDisplayable + 1; 
            }
            float angle = 360 / (count);
            for (int i = 0; i < VirtualContainer.Count(); i++)
            {
                if (i >= currentFirstDisplayed && i < (currentFirstDisplayed + sameTimeDisplayable))
                {
                    VirtualContainer[i].Display(true);
                    IMenuItemAnchor MenuItemAnchor = VirtualContainer[i].gameObject.GetComponent<IMenuItemAnchor>();
                    Vector3 dir = (Quaternion.AngleAxis(( -i + currentFirstDisplayed - 1 ) * -angle, Vector3.forward) * Vector3.down).normalized;
                    if (MenuItemAnchor == null)
                        transform.localPosition = dir * (radius);
                    else
                        MenuItemAnchor.Set(dir * (radius), dir);
                }
                else
                {
                    VirtualContainer[i].Hide();
                }
            }
            bool active = VirtualContainer.Count() > sameTimeDisplayable;
            if (NextObject.activeSelf != active) NextObject.SetActive(active);
            if (PreviousObject.activeSelf != active) PreviousObject.SetActive(active);
        }

        void _Next()
        {
            currentFirstDisplayed += sameTimeDisplayable;
            if (currentFirstDisplayed > VirtualContainer.Count()) currentFirstDisplayed = 0;
            OrganiseChildren();
        }

        void _Previous()
        {
            currentFirstDisplayed -= 8;
            int count = VirtualContainer.Count();
            if (currentFirstDisplayed < 0) currentFirstDisplayed = count - (count%sameTimeDisplayable);
            OrganiseChildren();
        }

        public override void SetMenuItem(AbstractMenuItem menu)
        {
            base.SetMenuItem(menu);
            OrganiseChildren();
        }
    }
}