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
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace umi3d.cdk.interaction
{
    [Serializable]
    public class UIManipulationControlEntity 
        : AbstractControlEntity, HasManipulationControlData, IUIControlEntity
    {
        public ManipulationControlData manipulationData = new();
        public UIInputType input = new();

        public UIManipulationControlEntity()
        {
            controlData.canDissociateHandler = value =>
            {
                if (value is not InputActionPhase phase)
                {
                    return true;
                }

                return phase == InputActionPhase.Canceled;
            };
            controlData.enableHandler += (this as IUIControlEntity).Enable;
            controlData.disableHandler += (this as IUIControlEntity).Disable;
        }

        public ManipulationControlData ManipulationControlData
        {
            get
            {
                return manipulationData;
            }
            set
            {
                manipulationData = value;
            }
        }

        public UIInputType Input
        {
            get
            {
                return input;
            }
        }

        public void UIActionPerformed(System.Object value)
        {
            if (value is not InputActionPhase _value)
            {
                throw new Exception($"[UMI3D] Control: ui input value is not InputActionPhase");
            }
            controlData.ActionPerformed(_value);
        }
    }
}