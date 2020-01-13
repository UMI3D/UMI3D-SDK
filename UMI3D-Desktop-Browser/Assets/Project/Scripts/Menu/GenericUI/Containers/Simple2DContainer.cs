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
using umi3d.cdk.menu.core;
using umi3d.cdk.menu.view;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BrowserDesktop.Menu
{

    public class Simple2DContainer : AbstractMenuDisplayContainer
    {
        public GameObject viewport;
        public TextMeshProUGUI label;
        public Button backButton;
        public Button selectButton;

        private List<AbstractDisplayer> containedDisplayers = new List<AbstractDisplayer>();

        private AbstractMenuDisplayContainer virtualContainer;

        protected AbstractMenuDisplayContainer VirtualContainer
        {
            get => virtualContainer; set {
                if(virtualContainer != null)
                    backButton?.onClick.RemoveListener(virtualContainer.backButtonPressed.Invoke);
                virtualContainer = value;
                bool display = virtualContainer?.parent != null;
                if(backButton != null && backButton.gameObject.activeSelf != display)backButton.gameObject.SetActive(display);
                if (display)
                    backButton?.onClick.AddListener(virtualContainer.backButtonPressed.Invoke);
            }
        }

        public override AbstractDisplayer this[int i] { get => containedDisplayers[i]; set { RemoveAt(i); Insert(value,i); } }

        void HideBackButton()
        {
            backButton?.gameObject.SetActive(false);
            if(virtualContainer != null)
                backButton?.onClick.RemoveListener(virtualContainer.backButtonPressed.Invoke);
        }

        public override void Clear()
        {
            foreach (var displayer in containedDisplayers)
                displayer.Clear();
            HideBackButton();
            selectButton?.onClick.RemoveAllListeners();
            RemoveAll();
        }

        public override bool Contains(AbstractDisplayer element)
        {
            return containedDisplayers.Contains(element);
        }

        public override void Display(bool forceUpdate = false)
        {
            if (isDisplayed && !forceUpdate)
            {
                return;
            }
            selectButton?.onClick.AddListener(Select);
            this.gameObject.SetActive(true);
            if (VirtualContainer != null) VirtualContainer = this;
            if (label)
                label.text = menu.Name;
            foreach (AbstractDisplayer disp in containedDisplayers)
            {
                disp.Display();
            }
            isDisplayed = true;
        }

        public override int GetIndexOf(AbstractDisplayer element)
        {
            return element.transform.GetSiblingIndex();
        }

        protected override IEnumerable<AbstractDisplayer> GetDisplayers()
        {
            return containedDisplayers;
        }

        //public override IEnumerable<AbstractMenuDisplayContainer> GetSubMenuDisplayContainers()
        //{
        //    List<AbstractMenuDisplayContainer> container = new List<AbstractMenuDisplayContainer>();
        //    foreach(var displayer in containedDisplayers)
        //    {
        //        if (displayer is AbstractMenuDisplayContainer)
        //            container.Add(displayer as AbstractMenuDisplayContainer);
        //    }
        //    return container;
        //}

        public override void Hide()
        {
            foreach (AbstractDisplayer disp in containedDisplayers)
            {
                disp.Hide();
            }
            HideBackButton();
            selectButton?.onClick.RemoveListener(Select);
            this.gameObject.SetActive(false);
            isDisplayed = false;
        }

        /// <summary>
        /// Insert an element in the display container.
        /// </summary>
        /// <param name="element">Element to insert</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
        {
            element.transform.SetParent(viewport.transform, false);

            containedDisplayers.Add(element);
            if (updateDisplay)
                Display();
        }

        /// <summary>
        /// Insert a collection of elements in the display container.
        /// </summary>
        /// <param name="elements">Elements to insert</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public override void InsertRange(IEnumerable<AbstractDisplayer> elements, bool updateDisplay = true)
        {
            foreach(AbstractDisplayer e in elements)
            {
                Insert(e, false);
            }
            if (updateDisplay)
                Display();
        }

        public override void Insert(AbstractDisplayer element, int index, bool updateDisplay = true)
        {
            element.transform.SetParent(viewport.transform, false);
            element.transform.SetSiblingIndex(index);

            containedDisplayers.Add(element);
            if (updateDisplay)
                Display();
        }

        /// <summary>
        /// Remove an object from the display container.
        /// </summary>
        /// <param name="element">Element to remove</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public override bool Remove(AbstractDisplayer element, bool updateDisplay = true)
        {
            if (element == null) return false;
            bool ok = containedDisplayers.Remove(element);
            if (updateDisplay)
                Display();
            return ok;
        }

        /// <summary>
        /// Remove all elements from the display container.
        /// </summary>
        public override int RemoveAll()
        {
            List<AbstractDisplayer> elements = new List<AbstractDisplayer>(containedDisplayers);
            int count = 0;
            foreach (AbstractDisplayer element in elements)
                if (Remove(element, false)) count++;
            return count;
        }

        public override bool RemoveAt(int index, bool updateDisplay = true)
        {
           return Remove(containedDisplayers?[index], updateDisplay);
        }

        public override void Expand(bool forceUpdate = false)
        {
            if (!isDisplayed)
            {
                Display(forceUpdate);
            }
            if (isExpanded && !forceUpdate)
            {
                return;
            }
            
            if (VirtualContainer != null && VirtualContainer != this)
            {
                label.text = menu.Name;
                foreach (AbstractDisplayer displayer in VirtualContainer)
                {
                    displayer.Hide();
                    displayer.transform.SetParent((VirtualContainer as Simple2DContainer)?.viewport?.transform);
                }
            }

            viewport.SetActive(true);
            VirtualContainer = this;
            selectButton?.onClick.RemoveListener(Select);
            foreach (AbstractDisplayer displayer in this)
            {
                displayer.Display();
            }
            isExpanded = true;
        }

        public override void Collapse(bool forceUpdate = false)
        {
            if (!isExpanded && !forceUpdate)
            {
                return;
            }
            if (VirtualContainer != null && VirtualContainer != this)
            {
                label.text = menu.Name;
                foreach (AbstractDisplayer displayer in VirtualContainer)
                {
                    displayer.Hide();
                    displayer.transform.SetParent((VirtualContainer as Simple2DContainer)?.viewport?.transform);
                }
                VirtualContainer = this;
            }
            HideBackButton();
            viewport.SetActive(false);
            backButton?.gameObject.SetActive(false);
            selectButton?.onClick.AddListener(Select);
            foreach (AbstractDisplayer displayer in this)
            {
                displayer.Hide();
            }
            isExpanded = false;
        }

        public override void ExpandAs(AbstractMenuDisplayContainer Container, bool forceUpdate = false)
        {
            if (isExpanded && !forceUpdate)
            {
                return;
            }
            if (VirtualContainer != null && VirtualContainer != Container)
            {
                backButton?.gameObject.SetActive(false);
                selectButton?.onClick.AddListener(Select);
                foreach (AbstractDisplayer displayer in this)
                {
                    displayer.Hide();
                }
            }

            VirtualContainer = Container;
            label.text = Container.menu.Name;
            viewport.SetActive(true);
            selectButton?.onClick.RemoveListener(Select);
            foreach (AbstractDisplayer displayer in VirtualContainer)
            {
                displayer.transform.SetParent( viewport.transform);
                displayer.Display();
            }
            isExpanded = true;
        }

        public override AbstractMenuDisplayContainer CurrentMenuDisplayContainer()
        {
            return VirtualContainer;
        }


        public override int IsSuitableFor(umi3d.cdk.menu.core.AbstractMenuItem menu)
        {
            return (menu is umi3d.cdk.menu.core.Menu) ? 1 : 0;
        }

        public override int Count()
        {
            return containedDisplayers.Count;
        }
    }
}