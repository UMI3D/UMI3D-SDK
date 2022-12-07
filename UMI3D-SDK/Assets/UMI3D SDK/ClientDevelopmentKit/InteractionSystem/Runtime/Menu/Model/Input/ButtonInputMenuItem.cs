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
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.menu
{
    /// <summary>
    /// <see cref="MenuItem"/> for Button input.
    /// </summary>
    /// <see cref="BooleanInputMenuItem"/>
    public class ButtonInputMenuItem : AbstractInputMenuItem<bool>
    {
        /// <summary>
        /// If true, the button will stay pressed on selection.
        /// </summary>
        public bool toggle = false;

        /// <summary>
        /// Image to display (if any).
        /// </summary>
        public Sprite sprite;

        /// <summary>
        /// Is the button pressed ?
        /// </summary>
        private bool pressedState = false;

        /// <summary>
        /// Subscribers on value change
        /// </summary>
        private readonly List<UnityAction<bool>> subscribers = new List<UnityAction<bool>>();



        /// <summary>
        /// Subscribe a callback for button press.
        /// </summary>
        /// <param name="callback">Callback to invoke on button press</param>
        public override void Subscribe(UnityAction<bool> callback)
        {
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }

        /// <summary>
        /// Unsubscribe a callback from the value change.
        /// </summary>
        /// <param name="callback"></param>
        public override void UnSubscribe(UnityAction<bool> callback)
        {
            subscribers.Remove(callback);
        }

        /// <summary>
        /// Get the name of the MenuItem
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Return the State of the Button Input Menu Item.
        /// </summary>
        /// <returns>true if the button is pressed, false if not</returns>
        public override bool GetValue()
        {
            return pressedState;
        }

        /// <summary>
        /// Notify a value change to the subscribers.
        /// </summary>
        /// <param name="newValue"></param>
        public override void NotifyValueChange(bool newValue)
        {
            if (toggle)
            {
                pressedState = !pressedState;
            }

            foreach (UnityAction<bool> sub in subscribers)
            {
                sub.Invoke(pressedState);
            }
        }
    }
}