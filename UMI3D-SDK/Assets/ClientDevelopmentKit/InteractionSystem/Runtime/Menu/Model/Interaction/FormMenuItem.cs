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
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.menu.interaction
{
    public class FormMenuItem : InteractionMenuItem
    {
        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private List<UnityAction<FormDto>> subscribers = new List<UnityAction<FormDto>>();

        public FormDto dto;

        /// <summary>
        /// Get the current input value.
        /// </summary>
        /// <returns></returns>
        public FormDto GetValue() { return dto; }

        /// <summary>
        /// Notify a value change.
        /// </summary>
        /// <param name="newValue">New value</param>
        /// <param name="hoveredObjectId">Id of the </param>
        public void NotifyValueChange(FormDto newValue)
        {
            dto = newValue;
            foreach (var callback in subscribers)
                callback(dto);
        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(UnityAction{FormDto})"/>
        public void Subscribe(UnityAction<FormDto> callback)
        {
            if (callback != null && !subscribers.Contains(callback))
                subscribers.Add(callback);
        }

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction{T})"/>
        public void UnSubscribe(UnityAction<FormDto> callback)
        {
            if (callback != null)
                subscribers.Remove(callback);
        }
    }
}