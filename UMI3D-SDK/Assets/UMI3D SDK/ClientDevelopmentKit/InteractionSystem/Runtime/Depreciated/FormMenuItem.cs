#if !UMI3D_NEW_LABEL
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

using System;
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.menu.interaction
{
    [Obsolete("This class or method will be removed when the new label feature will be activated.")]
    /// <summary>
    /// <see cref="AbstractMenuItem"/> for <see cref="FormDto"/>
    /// </summary>
    public class FormMenuItem : InteractionMenuItem
    {
        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<Action<List<ParameterSettingRequestDto>>> subscribers = new List<Action<List<ParameterSettingRequestDto>>>();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Answers to the form as a list of <see cref="ParameterSettingRequestDto"/>.
        /// </summary>
        public List<ParameterSettingRequestDto> answers;

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Form the menu item is associated with.
        /// </summary>
        public FormDto dto;

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Get the current input value.
        /// </summary>
        /// <returns></returns>
        public FormDto GetValue() { return dto; }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Notify a value change.
        /// </summary>
        /// <param name="newValue">New value</param>
        /// <param name="hoveredObjectId">Id of the </param>
        public void NotifyValueChange(List<ParameterSettingRequestDto> newValue)
        {
            answers = newValue;
            foreach (Action<List<ParameterSettingRequestDto>> callback in subscribers)
                callback(answers);
        }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(Action{FormDto})"/>
        public void Subscribe(Action<List<ParameterSettingRequestDto>> callback)
        {
            if (callback != null && !subscribers.Contains(callback))
                subscribers.Add(callback);
        }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(Action{T})"/>
        public void UnSubscribe(Action<List<ParameterSettingRequestDto>> callback)
        {
            if (callback != null)
                subscribers.Remove(callback);
        }
    }
}
#endif