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



namespace umi3d.cdk.menu
{
    /// <summary>
    /// Abstract class for menu item.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractMenuItem : ISelectable
    {
        /// <summary>
        /// Menu name.
        /// </summary>
        public string Name = "menu";

        public Texture2D icon2D;
        public GameObject icon3D;

        /// <summary>
        /// Menu name.
        /// </summary>
        public abstract override string ToString();

        /// <summary>
        /// Event raised when menu content has been changed.
        /// </summary>
        public UnityEvent OnDestroy = new UnityEvent();


        /// <summary>
        /// Selection event subscribers.
        /// </summary>
        private readonly List<UnityAction> subscribers = new List<UnityAction>();

        /// <summary>
        /// Raise selection event.
        /// </summary>
        public virtual void Select()
        {
            var localCopySubs = new List<UnityAction>(subscribers);
            foreach (UnityAction sub in localCopySubs)
                sub.Invoke();
        }

        /// <summary>
        /// Subscribe a callback to the selection event.
        /// </summary>
        /// <param name="callback">Callback to raise on selection</param>
        /// <see cref="UnSubscribe(UnityAction)"/>
        public virtual void Subscribe(UnityAction callback)
        {
            if (!subscribers.Contains(callback))
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