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

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Game object containing UMI3D <see cref="umi3d.cdk.interaction.Interactable"/>.
    /// </summary>
    public class InteractableContainer : MonoBehaviour
    {
        /// <summary>
        /// List of all <see cref="umi3d.cdk.interaction.Interactable"/> containers.
        /// </summary>
        public static List<InteractableContainer> containers = new List<InteractableContainer>();

        /// <summary>
        /// Interatable associated with the object.
        /// </summary>
        [Tooltip("Interatable associated with the object")]
        public Interactable Interactable;

        private void Awake()
        {
            if (!containers.Contains(this))
                containers.Add(this);
        }

        private void OnDestroy()
        {
            containers.Remove(this);
        }
    }
}