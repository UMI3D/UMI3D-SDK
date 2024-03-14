/*
Copyright 2019 - 2024 Inetum

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
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace umi3d.cdk.interaction
{
    public interface HasControlData
    {
        ControlData ControlData { get; set; }
    }

    [Serializable]
    public sealed class ControlData 
    {
        /// <summary>
        /// Whether this control is being used by the browser.
        /// </summary>
        public bool isUsed;

        /// <summary>
        /// Current controller associated with this control.
        /// </summary>
        [HideInInspector] public UMI3DController controller;
        /// <summary>
        /// Environment's id.
        /// </summary>
        [HideInInspector] public ulong environmentId;
        /// <summary>
        /// Tool's id currently associated with this control.
        /// </summary>
        [HideInInspector] public ulong toolId;
        /// <summary>
        /// Interaction currently associated with this control.
        /// </summary>
        [HideInInspector] public AbstractInteractionDto interaction;

        /// <summary>
        /// Current value.
        /// </summary>
        [HideInInspector] public System.Object value;
        [HideInInspector] public bool shouldDissociateAsSoonAsPossible;

        public Action enableHandler;
        public Action disableHandler;
        public Action dissociateHandler;
        public Action<System.Object> actionPerformedHandler;
        public Func<System.Object, bool> canPerformHandler;
        public Func<System.Object, bool> canDissociateHandler;

        public void Dissociate()
        {
            if (canDissociateHandler?.Invoke(value) ?? true)
            {
                dissociateHandler?.Invoke();
            }
            else
            {
                shouldDissociateAsSoonAsPossible = true;
            }
        }

        /// <summary>
        /// The action performed method add to the <see cref="InputAction.performed"/> event.<br/>
        /// See <see cref="actionPerformedHandler"/>.
        /// </summary>
        /// <param name="obj"></param>
        public void ActionPerformed(System.Object value)
        {
            this.value = value;
            if (canPerformHandler?.Invoke(value) ?? true)
            {
                actionPerformedHandler?.Invoke(value);
            }

            if (shouldDissociateAsSoonAsPossible && (canDissociateHandler?.Invoke(value) ?? true))
            {
                dissociateHandler?.Invoke();
                shouldDissociateAsSoonAsPossible = false;
            }
        }
    }
}