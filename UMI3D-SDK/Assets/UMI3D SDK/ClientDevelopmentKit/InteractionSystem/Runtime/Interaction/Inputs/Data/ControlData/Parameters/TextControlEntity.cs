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
    public class TextControlEntity : AbstractControlEntity, IUIControlEntity
    {
        public UIInputType input = new();

        public TextControlEntity()
        {
            controlData.canDissociateHandler = value =>
            {
                return true;
            };
            controlData.enableHandler += (this as IUIControlEntity).Enable;
            controlData.disableHandler += (this as IUIControlEntity).Disable;
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
            if (value is not string _value)
            {
                throw new Exception($"[UMI3D] Control: ui input value is not string");
            }
            controlData.ActionPerformed(_value);
        }
    }
}