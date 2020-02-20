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
    public abstract class AbstractParameterInput<InputMenuItem, ParameterType, ValueType> : AbstractUMI3DInput
        where InputMenuItem : AbstractInputMenuItem<ValueType>, new()
        where ParameterType : AbstractParameterDto<ValueType>

    {
        /// <summary>
        /// Associated menu item.
        /// </summary>
        public InputMenuItem menuItem;


        /// <summary>
        /// Interaction currently associated to this input.
        /// </summary>
        protected AbstractInteractionDto currentInteraction;

        /// <summary>
        /// Associated callback
        /// </summary>
        /// <see cref="Associate(AbstractInteractionDto)"/>
        protected UnityAction<ValueType> callback;



        public override void Associate(AbstractInteractionDto interaction)
        {
            if (currentInteraction != null)
            {
                throw new System.Exception("This input is already associated to another interaction (" + currentInteraction + ")");
            }

            if (interaction is ParameterType)
            {
                menuItem = new InputMenuItem()
                {
                    dto = interaction as ParameterType,
                    Name = interaction.Name
                };
                if (CircleMenu.Exist)
                    CircleMenu.Instance.MenuDisplayManager.menu.Add(menuItem);

                menuItem.NotifyValueChange((interaction as ParameterType).Value);
                callback = x => UMI3DHttpClient.Interact(currentInteraction.Id, x);
                menuItem.Subscribe(callback);
                currentInteraction = interaction;
            }
            else
            {
                throw new System.Exception("Incompatible interaction");
            }
        }

        public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs)
        {
            throw new System.Exception("Incompatible interaction");
        }

        public override AbstractInteractionDto CurrentInteraction()
        {
            return currentInteraction;
        }

        public override void Dissociate()
        {
            currentInteraction = null;
            menuItem.UnSubscribe(callback);
            if (CircleMenu.Exist)
                CircleMenu.Instance?.MenuDisplayManager?.menu?.Remove(menuItem);
        }

        public override bool IsCompatibleWith(AbstractInteractionDto interaction)
        {
            return interaction is ParameterType;
        }

        public override bool IsAvailable()
        {
            return currentInteraction == null;
        }

        private void OnDestroy()
        {
            Dissociate();
        }
    }
}