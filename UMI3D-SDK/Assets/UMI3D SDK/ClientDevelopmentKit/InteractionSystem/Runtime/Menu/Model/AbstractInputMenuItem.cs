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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    /// <summary>
    /// Base class for input menu items.
    /// </summary>
    public abstract class AbstractInputMenuItem<T> : AbstractInputMenuItem, IObservable<T>
    {
        /// <summary>
        /// Parameter DTO the menu is for.
        /// </summary>
        public AbstractParameterDto<T> dto;

        /// <summary>
        /// Get the current input value.
        /// </summary>
        /// <returns></returns>
        public abstract T GetValue();

        /// <summary>
        /// Notify a value change.
        /// </summary>
        /// <param name="newValue">New value</param>
        /// <param name="hoveredObjectId">Id of the </param>
        public abstract void NotifyValueChange(T newValue);

        public System.Func<T, ParameterSettingRequestDto> GetParameterFunc;

        /// <inheritdoc/>
        public override ParameterSettingRequestDto GetParameter()
        {
            return GetParameterFunc?.Invoke(GetValue());
        }

        /// <summary>
        /// Subscribe a callback to the value change.
        /// </summary>
        /// <param name="callback">Callback to raise on a value change (argument is the new value)</param>
        /// <see cref="UnSubscribe(UnityAction{T})"/>
        public abstract void Subscribe(UnityAction<T> callback);

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction{T})"/>
        public abstract void UnSubscribe(UnityAction<T> callback);
    }

    /// <summary>
    /// Base class for input menu items.
    /// </summary>
    public abstract class AbstractInputMenuItem : MenuItem
    {
        /// <summary>
        /// Get the associated <see cref="ParameterSettingRequestDto"/>
        /// </summary>
        /// <returns></returns>
        public abstract ParameterSettingRequestDto GetParameter();
    }
}