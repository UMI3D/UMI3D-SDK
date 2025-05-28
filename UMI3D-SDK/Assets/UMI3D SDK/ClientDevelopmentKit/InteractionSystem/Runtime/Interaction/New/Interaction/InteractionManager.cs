/*
Copyright 2019 - 2025 Inetum

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
using System.Collections.ObjectModel;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public class InteractionManager 
    {
        #region Initialize

        static Lazy<InteractionManager> _default = new(() => new());
        public static InteractionManager @default => _default.Value;

        InteractionManager()
        {

        }

        #endregion

        List<Interaction> _interactions = new();
        public ReadOnlyCollection<Interaction> interactions => _interactions.AsReadOnly();

        /// <summary>
        /// Searches for an existing interaction based on the specified environment ID and DTO (Data Transfer Object). 
        /// If an interaction matching the criteria exists, it is returned. Otherwise, a new interaction is created, 
        /// added to the internal list, registered in the environment loader, and returned.<br/>
        /// <br/>
        /// <example>
        /// Given an environment ID and a DTO, when calling this method, it will either return an existing interaction 
        /// or create a new one and register it in the environment loader.<br/>
        /// <br/>
        /// <code>
        /// bool isNew = interactionManager.InstantiateOrGet(out Interaction interaction, 12345UL, dto);
        /// if (isNew)
        /// {
        ///     Console.WriteLine("A new interaction was created and registered.");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("An existing interaction was returned.");
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="interaction">
        /// The output parameter that will hold the existing or newly created interaction.
        /// </param>
        /// <param name="environmentId">
        /// The unique identifier of the environment associated with the interaction.
        /// </param>
        /// <param name="dto">
        /// The data transfer object (DTO) containing the interaction's details.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether a new interaction was created (`true`) or an existing interaction was found (`false`).
        /// </returns>
        public bool InstantiateOrGet(out Interaction interaction, ulong environmentId, AbstractInteractionDto dto)
        {
            interaction = _interactions.Find(interaction => interaction.environmentId == environmentId && interaction.dto.id == dto.id);
            if (interaction != null)
            {
                UnityEngine.Debug.Log($"[InteractionManager] Notice: interaction for id: '{environmentId}' and dto's id: '{dto.id}' already exist.");
                return false;
            }

            UnityEngine.Debug.Log($"[InteractionManager] Notice: interaction for id: '{environmentId}' and dto's id: '{dto.id}' created.");
            interaction = new(environmentId, dto);
            _interactions.Add(interaction);
            UMI3DEnvironmentLoader.Instance.RegisterEntity(environmentId, dto.id, dto, interaction).NotifyLoaded();
            return true;
        }

        public bool TryToRemoveInteraction(Interaction interaction)
        {
            if (!_interactions.Contains(interaction))
            {
                return false;
            }

            _ = UMI3DEnvironmentLoader.Instance.DeleteEntityInstance(interaction.environmentId, interaction.dto.id);
            _interactions.Remove(interaction);
            return true;
        }
        public bool TryToRemoveInteraction(ulong environmentId, ulong interactionId)
        {
            Interaction interaction = _interactions.Find(interaction => interaction.environmentId == environmentId && interaction.dto.id == interactionId);
            if (interaction == null)
            {
                return false;
            }

            _ = UMI3DEnvironmentLoader.Instance.DeleteEntityInstance(environmentId, interactionId);
            _interactions.Remove(interaction);
            return true;
        }
    }
}