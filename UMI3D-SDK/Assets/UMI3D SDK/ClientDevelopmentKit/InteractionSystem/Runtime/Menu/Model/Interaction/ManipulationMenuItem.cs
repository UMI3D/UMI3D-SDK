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
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.menu.interaction
{
    public class ManipulationMenuItem : InteractionMenuItem, ITogglable
    {
        /// <summary>
        /// Dto for this manipulation.
        /// </summary>
        public new ManipulationDto interaction;

        /// <summary>
        /// Dof Seperation.
        /// </summary>
        public DofGroupDto dof;

        /// <summary>
        /// Toggle event subscribers.
        /// </summary>
        private readonly List<UnityAction<bool>> subscribers = new List<UnityAction<bool>>();

        /// <summary>
        /// Raise deselection event.
        /// </summary>
        public void Deselect()
        {
            foreach (UnityAction<bool> sub in subscribers)
            {
                sub.Invoke(false);
            }
        }

        /// <summary>
        /// Raise selection event.
        /// </summary>
        public override void Select()
        {
            base.Select();
            foreach (UnityAction<bool> sub in subscribers)
            {
                sub.Invoke(true);
            }
        }

        /// <summary>
        /// Subscribe a callback from the selection or deselection event.
        /// </summary>
        /// <param name="callback">Callback to subscribe</param>
        /// <see cref="UnSubscribe(UnityAction{bool})"/>
        public void Subscribe(UnityAction<bool> callback)
        {
            subscribers.Add(callback);
        }

        /// <summary>
        /// Unsubscribe a callback from the selection or deselection event.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction{bool})"/>
        public void UnSubscribe(UnityAction<bool> callback)
        {
            subscribers.Remove(callback);
        }
    }
}