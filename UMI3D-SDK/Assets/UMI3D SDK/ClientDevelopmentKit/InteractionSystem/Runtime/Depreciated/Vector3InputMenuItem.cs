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
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    [Obsolete("This class or method will be removed when the new label feature will be activated.")]
    /// <summary>
    /// <see cref="AbstractMenuItem"/> for text input.
    /// </summary>
    public class Vector3InputMenuItem : AbstractInputMenuItem<Vector3>
    {
        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Input value.
        /// </summary>
        private Vector3 value = Vector3.zero;

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<Action<Vector3>> subscribers = new List<Action<Vector3>>();

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <inheritdoc/>
        public override Vector3 GetValue()
        {
            return value;
        }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Notify a change of the input value.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(Vector3 newValue)
        {
            value = newValue;
            foreach (Action<Vector3> sub in subscribers)
            {
                sub.Invoke(newValue);
            }
        }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Subscribe a callback for input value change.
        /// </summary>
        /// <param name="callback">Callback to invoke on input value change</param>
        public override bool Subscribe(Action<Vector3> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
                return true;
            }
            return false;
        }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback"></param>
        public override bool UnSubscribe(Action<Vector3> callback)
        {
           return subscribers.Remove(callback);
        }
    }
}
#endif