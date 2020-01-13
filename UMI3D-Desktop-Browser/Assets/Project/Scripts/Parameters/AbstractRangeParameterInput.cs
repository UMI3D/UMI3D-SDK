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
using umi3d.cdk;
using umi3d.common;
using umi3d.cdk.menu.core;
using UnityEngine.Events;
using BrowserDesktop.Menu;

namespace BrowserDesktop.Parameters
{
    public abstract class AbstractRangeParameterInput<InputMenuItem, ParameterType, ValueType> : AbstractParameterInput<InputMenuItem, ParameterType, ValueType>
    where ValueType : System.IComparable
    where InputMenuItem : AbstractRangeInputMenuItem<ValueType>, new()
    where ParameterType : AbstractRangeParameterDto<ValueType>
    {

        public override void Associate(AbstractInteractionDto interaction)
        {
            if (currentInteraction != null)
            {
                throw new System.Exception("This input is already associated to another interaction (" + currentInteraction + ")");
            }

            if (interaction is ParameterType)
            {
                ParameterType param = interaction as ParameterType;
                menuItem = new InputMenuItem()
                {
                    dto = interaction as ParameterType,
                    min = param.Min,
                    max = param.Max,
                    Name = param.Name,
                    value = param.Value,
                    increment = param.Increment
                };

                callback = x =>
                {
                    if ((x.CompareTo(param.Min) >= 0) && (x.CompareTo(param.Max) <= 0))
                    {
                        UMI3DHttpClient.Interact(interaction.Id, x);
                    }
                };

                menuItem.NotifyValueChange(param.Value);
                menuItem.Subscribe(callback);
                if (CircleMenu.Exist)
                    CircleMenu.Instance.MenuDisplayManager.menu.Add(menuItem);
                currentInteraction = interaction;
            }
            else
            {
                throw new System.Exception("Incompatible interaction");
            }
        }
    }
}