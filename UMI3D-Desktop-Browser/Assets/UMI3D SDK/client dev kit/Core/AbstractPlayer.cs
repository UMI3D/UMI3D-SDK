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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for UMI3D Player.
    /// </summary>
    public abstract class AbstractPlayer : MonoBehaviour
    {
    
        /// <summary>
        /// Reset player.
        /// </summary>
        public void ResetModule()
        {
            ResetInteractions();
        }

        /// <summary>
        /// Interactions ids.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> InteractionIds();

        /// <summary>
        /// Reset interactions.
        /// </summary>
        public abstract void ResetInteractions();



        /// <summary>
        /// Add interactions to the interactions list.
        /// </summary>
        /// <param name="data">Interactions to add</param>
        public void AddInteractions(AddInteractionsDto data)
        {
            if (data == null)
                return;
            foreach (var i in data.Entities)
                AddInteraction(i);
        }

        /// <summary>
        /// Add an interaction to the interactions list.
        /// </summary>
        /// <param name="interaction"></param>
        public void AddInteraction(AbstractInteractionDto interaction)
        {
            if (interaction == null)
                return;

            if (interaction is ActionDto)
                AddAction(interaction as ActionDto);
            else if (interaction is DirectInputDto)
                AddDirectInput(interaction as DirectInputDto);
            else if (interaction is HoverDto)
                AddHover(interaction as HoverDto);
            else if (interaction is InteractionListDto)
                AddInteractionList(interaction as InteractionListDto);
            else if (interaction is PickDto)
                AddPick(interaction as PickDto);
            else if (interaction is ManipulationDto)
                AddManipulation(interaction as ManipulationDto);
        }


        protected abstract void AddAction(ActionDto dto);
        protected abstract void AddDirectInput(DirectInputDto dto);
        protected abstract void AddHover(HoverDto dto);
        protected abstract void AddInteractionList(InteractionListDto dto);
        protected abstract void AddPick(PickDto dto);
        protected abstract void AddManipulation(ManipulationDto dto);


        /// <summary>
        /// Remove interactions from the interactions list.
        /// </summary>
        /// <param name="data">Interactions to remove</param>
        public void RemoveInteractions(RemoveInteractionsDto data)
        {
            if ( data == null)
                return;
            RemoveInteractions(data.Ids.ToArray());
        }

        /// <summary>
        /// Remove interactions from the interactions list.
        /// </summary>
        /// <param name="ids">Interactions' ids to remove.</param>
        public void RemoveInteractions(string[] ids)
        {
            if (ids == null)
                return;
            else
                foreach (string id in ids)
                    RemoveInteraction(id);
        }

        /// <summary>
        /// Remove an interaction from the interactions list.
        /// </summary>
        /// <param name="id">Interaction id to remove</param>
        public abstract void RemoveInteraction(string id);

        /// <summary>
        /// Update an interaction in the list.
        /// </summary>
        /// <param name="data">Interaction to update</param>
        public void UpdateInteraction(UpdateInteractionDto data)
        {
            if (data == null)
                return;
            UpdateInteraction(data.Entity);
        }

        /// <summary>
        /// Update an interaction in the list.
        /// </summary>
        /// <param name="interaction">Interaction to update</param>
        public void UpdateInteraction(AbstractInteractionDto interaction)
        {
            if (interaction == null)
                return;
            if (interaction is ActionDto)
                UpdateAction(interaction as ActionDto);
            else if (interaction is DirectInputDto)
                UpdateDirectInput(interaction as DirectInputDto);
            else if (interaction is HoverDto)
                UpdateHover(interaction as HoverDto);
            else if (interaction is InteractionListDto)
                UpdateInteractionList(interaction as InteractionListDto);
            else if (interaction is PickDto)
                UpdatePick(interaction as PickDto);
            else if (interaction is ManipulationDto)
                UpdateManipulation(interaction as ManipulationDto);
        }

        protected abstract void UpdateAction(ActionDto dto);
        protected abstract void UpdateDirectInput(DirectInputDto dto);
        protected abstract void UpdateHover(HoverDto dto);
        protected abstract void UpdateInteractionList(InteractionListDto dto);
        protected abstract void UpdatePick(PickDto dto);
        protected abstract void UpdateManipulation(ManipulationDto dto);

        protected virtual void OnUpdate()
        {
        }


        protected void Update()
        {
            OnUpdate();
        }

    }
}