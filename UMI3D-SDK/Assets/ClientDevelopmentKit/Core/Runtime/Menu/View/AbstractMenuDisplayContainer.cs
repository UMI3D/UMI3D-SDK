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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.menu.view
{
    /// <summary>
    /// Abstract class for display containers (such as carousels, raidals, ...).
    /// </summary>
    [System.Serializable]
    public abstract class AbstractMenuDisplayContainer : AbstractDisplayer, IEnumerable<AbstractDisplayer>
    {

        /// <summary>
        /// Is the container being displayed ?
        /// </summary>
        [SerializeField]
        public bool isExpanded { get; protected set; }
        public bool isDisplayed { get; protected set; }

        #region Navigation

        /// <summary>
        /// Event raised when the back button is pressed (one frame only).
        /// </summary>
        public UnityEvent backButtonPressed = new UnityEvent();

        [ContextMenu("Back")]
        public void Back()
        {
            CurrentMenuDisplayContainer()?.backButtonPressed.Invoke();
        }

        /// <summary>
        /// Generation to replace on Expand. Negative value means no replacement; 0 replace siblings;
        /// </summary>
        public int generationOffsetOnExpand = -1;

        /// <summary>
        /// Does the container allow to navigate on several branch at once
        /// </summary>
        public bool parallelNavigation = false;

        /// <summary>
        /// Parent container (if any).
        /// </summary>
        public AbstractMenuDisplayContainer parent;

        /// <summary>
        /// Display child
        /// </summary>
        /// <param name="forceUpdate"></param>
        public abstract void Expand(bool forceUpdate = false);

        /// <summary>
        /// Hide without destroying
        /// </summary>
        /// <param name="forceUpdate"></param>
        public abstract void Collapse(bool forceUpdate = false);

        /// <summary>
        /// Display child of Container
        /// </summary>
        /// <param name="container"> container to display</param>
        /// <param name="forceUpdate"></param>
        public abstract void ExpandAs(AbstractMenuDisplayContainer container, bool forceUpdate = false);

        /// <summary>
        /// The AbstractMenuDisplayContainer that have is content displayed in this container. 
        /// </summary>
        public abstract AbstractMenuDisplayContainer CurrentMenuDisplayContainer();

        #endregion

        #region Data management

        /// <summary>
        /// Get the number of item in this Container
        /// </summary>
        /// <returns></returns>
        public abstract int Count();

        /// <summary>
        /// Insert an element in the display container.
        /// </summary>
        /// <param name="element">Element to insert</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public abstract void Insert(AbstractDisplayer element, bool updateDisplay = true);

        /// <summary>
        /// Insert an element in the display container at a given index.
        /// </summary>
        /// <param name="element">Element to insert</param>
        /// <param name="index">Index to insert at</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public abstract void Insert(AbstractDisplayer element, int index, bool updateDisplay = true);

        /// <summary>
        /// Insert a collection of elements in the display container.
        /// </summary>
        /// <param name="elements">Elements to insert</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public abstract void InsertRange(IEnumerable<AbstractDisplayer> elements, bool updateDisplay = true);


        /// <summary>
        /// Remove an object from the display container.
        /// </summary>
        /// <param name="element">Element to remove</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public abstract bool Remove(AbstractDisplayer element, bool updateDisplay = true);

        /// <summary>
        /// Remove the object at the specified index 
        /// </summary>
        /// <param name="index">Index of the element to remove</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public abstract bool RemoveAt(int index, bool updateDisplay = true);

        /// <summary>
        /// Remove all elements from the display container.
        /// </summary>
        public abstract int RemoveAll();

        /// <summary>
        /// Get the index of a contained element.
        /// </summary>
        /// <param name="element">Element to get index of</param>
        /// <returns></returns>
        public abstract int GetIndexOf(AbstractDisplayer element);

        /// <summary>
        /// Does this DisplayContainer Contains AbstractDisplayer
        /// </summary>
        /// <returns></returns>
        public abstract bool Contains(AbstractDisplayer element);

        /// <summary>
        /// Get Displayer in this container
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<AbstractDisplayer> GetDisplayers();

        #endregion
        #region Enumerable

        public IEnumerator<AbstractDisplayer> GetEnumerator()
        {
            return GetDisplayers().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public abstract AbstractDisplayer this[int i]
        {
            get;
            set;
        }

        #endregion
    }
}