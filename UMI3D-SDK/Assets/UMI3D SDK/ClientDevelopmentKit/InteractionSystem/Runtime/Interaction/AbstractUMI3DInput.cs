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
using umi3d.cdk.menu;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Abstract class for UMI3D Inputs.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractUMI3DInput : MonoBehaviour
    {
        /// <summary>
        /// Menu that will contain this input.
        /// </summary>
        [Tooltip("Menu that will contain this input")]
        public AbstractMenu Menu;

        /// <summary>
        /// Controller the input is related to.
        /// </summary>
        [SerializeField]
        public AbstractController controller { get; protected set; }

        /// <summary>
        /// Available numbers of degrees of freedom.
        /// </summary>
        [Tooltip("Available numbers of degrees of freedom")]
        public readonly int DegreesOfFreedom = 0;

        /// <summary>
        /// Event raised when the input is pressed down (first frame only).
        /// </summary>
        [Tooltip("Event raised when the input is pressed down (first frame only)")]
        public UnityEvent onInputDown = new UnityEvent();

        /// <summary>
        /// Event raised when the input is released (first frame only).
        /// </summary>
        [Tooltip("Event raised when the input is released (first frame only)")]
        public UnityEvent onInputUp = new UnityEvent();


        /// <summary>
        /// Initialise component.
        /// </summary>
        /// <param name="controller"><see cref="AbstractController"/> containing this input</param>
        public virtual void Init(AbstractController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Check if an interaction is compatible with the input.
        /// </summary>
        /// <param name="interaction">Interaction to check</param>
        /// <returns>True is it the interaction is compatible.</returns>
        public abstract bool IsCompatibleWith(AbstractInteractionDto interaction);

        /// <summary>
        /// Is the input currently available for association ?
        /// </summary>
        /// <returns></returns>
        /// <see cref="Associate(AbstractInteractionDto)"/>
        /// <see cref="Associate(ManipulationDto, DofGroupEnum)"/>
        public abstract bool IsAvailable();

        /// <summary>
        /// Get The current Interaction associated with this input.
        /// </summary>
        /// <returns></returns>
        public abstract AbstractInteractionDto CurrentInteraction();

        /// <summary>
        /// Associate an AbstractInteractionDto to this Input.
        /// </summary>
        /// <param name="interaction">The interaction that will be associated</param>
        public abstract void Associate(AbstractInteractionDto interaction, ulong toolId, ulong hoveredObjectId);

        /// <summary>
        /// Associate a ManipulationDto to this input.
        /// </summary>
        /// <param name="manipulation"></param>
        /// <param name="dofs"></param>
        public abstract void Associate(ManipulationDto manipulation, DofGroupEnum dofs, ulong toolId, ulong hoveredObjectId);

        /// <summary>
        /// Associate an AbstractInteractionDto to this Input.
        /// </summary>
        /// <param name="interaction">The interaction that will be associated</param>
        public abstract void UpdateHoveredObjectId(ulong hoveredObjectId);

        /// <summary>
        /// Dissociate the current associated interaction.
        /// </summary>
        public abstract void Dissociate();
    }
}