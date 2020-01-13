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
using BrowserDesktop.Menu;

namespace BrowserDesktop.Parameters
{
    public abstract class AbstractEnumParameterInput<InputMenuItem, ValueType> : AbstractParameterInput<InputMenuItem, EnumParameterDto<ValueType>, ValueType>
        where InputMenuItem : AbstractEnumInputMenuItem<ValueType>, new()
    {
        public override void Associate(AbstractInteractionDto interaction)
        {
            if (currentInteraction != null)
            {
                throw new System.Exception("This input is already associated to another interaction (" + currentInteraction + ")");
            }

            if (interaction is EnumParameterDto<ValueType>)
            {
                EnumParameterDto<ValueType> stringEnum = interaction as EnumParameterDto<ValueType>;

                callback = newValue =>
                {
                    UMI3DHttpClient.Interact(interaction.Id, newValue);
                };

                menuItem = new InputMenuItem()
                {
                    dto = stringEnum,
                    Name = interaction.Name,
                    options = stringEnum.PossibleValues
                };

                menuItem.NotifyValueChange(stringEnum.Value);
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