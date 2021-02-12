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
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.menu.view
{

    /// <summary>
    /// Abstract base class for menu display.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractDisplayer : MonoBehaviour, ISelectable
    {
        public abstract void Display(bool forceUpdate = false);
        public abstract void Hide();
        public virtual void Clear()
        {
            onClear.Invoke();
            onClear.RemoveAllListeners();
        }

        public UnityEvent onClear;

        /// <summary>
        /// Menu displayed.
        /// </summary>
        public AbstractMenuItem menu { get; protected set; }

        /// <summary>
        /// Set Menu displayed.
        /// </summary>
        public virtual void SetMenuItem(AbstractMenuItem menu)
        {
            this.menu = menu;
            Subscribe(menu.Select);
        }

        /// <summary>
        /// State If the current displayer is compatible with a menuItem 
        /// </summary>
        /// <param name="menu">Menu To test</param>
        /// <returns>1 to infinity if compatible and 0 if not</returns>
        public abstract int IsSuitableFor(AbstractMenuItem menu);


        /// <summary>
        /// Selection event subscribers.
        /// </summary>
        private List<UnityAction> subscribers = new List<UnityAction>();

        /// <summary>
        /// Raise selection event.
        /// </summary>
        [ContextMenu("Select")]
        public virtual void Select()
        {
            foreach (UnityAction sub in subscribers)
            {
                sub.Invoke();
            }
        }

        /// <summary>
        /// Subscribe a callback to the selection event.
        /// </summary>
        /// <param name="callback">Callback to raise on selection</param>
        /// <see cref="UnSubscribe(UnityAction)"/>
        public virtual void Subscribe(UnityAction callback)
        {
            subscribers.Add(callback);
        }

        /// <summary>
        /// Unsubscribe a callback from the selection event.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction)"/>
        public virtual void UnSubscribe(UnityAction callback)
        {
            subscribers.Remove(callback);
        }

    }
}