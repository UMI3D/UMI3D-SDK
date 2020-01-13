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
using umi3d.common;

namespace umi3d.cdk.menu.core
{
    /// <summary>
    /// Base class for input menu items.
    /// </summary>
    public abstract class AbstractInputMenuItem<T> : AbstractInputMenuItem, IObservable<T>
    {
        public AbstractParameterDto<T> dto;

        /// <summary>
        /// Get the current input value.
        /// </summary>
        /// <returns></returns>
        public abstract T GetValue();

        /// <summary>
        /// Notify a value change to observants.
        /// </summary>
        /// <param name="newValue"></param>
        public abstract void NotifyValueChange(T newValue);

        /// <summary>
        /// Register a callback as observant.
        /// </summary>
        /// <param name="callback"></param>
        public abstract void Subscribe(UnityAction<T> callback);

        /// <summary>
        /// Unregister an observing callback.
        /// </summary>
        /// <param name="callback"></param>
        public abstract void UnSubscribe(UnityAction<T> callback);
    }

    /// <summary>
    /// Base class for input menu items.
    /// </summary>
    public abstract class AbstractInputMenuItem : MenuItem
    {
    }
}   