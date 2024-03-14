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
using UnityEngine.InputSystem;

namespace umi3d.cdk.interaction
{
    [Serializable]
    public class PhysicalButtonControlEntity : AbstractControlEntity, HasButtonControlData
    {
        public ButtonControlData buttonData = new();
        public InputActionProperty input = new();

        public ButtonControlData ButtonControlData
        {
            get
            {
                return buttonData;
            }
            set
            {
                buttonData = value;
            }
        }

        public PhysicalButtonControlEntity()
        {
            controlData.canDissociateHandler = value =>
            {
                if (value is not InputActionPhase phase)
                {
                    return true;
                }

                return phase == InputActionPhase.Canceled;
            };
            controlData.enableHandler += Enable;
            controlData.disableHandler += Disable;
        }

        public void Enable()
        {
            try
            {
                input.action.performed += ActionPerformed;
            }
            catch (NullReferenceException)
            {
                UnityEngine.Debug.LogError($"[UMI3D] Control: new input type action is null");
            }
        }

        public void Disable()
        {
            try
            {
                input.action.performed -= ActionPerformed;
            }
            catch (NullReferenceException)
            {
                UnityEngine.Debug.LogError($"[UMI3D] Control: new input type action is null");
            }
        }

        void ActionPerformed(InputAction.CallbackContext ctxt)
        {
            controlData.ActionPerformed(ctxt.phase);
        }
    }
}