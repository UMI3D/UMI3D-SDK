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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    public class UMI3DManipulation : AbstractInteraction
    {
        [System.Serializable]
        public class ManipulationListener : UnityEvent<ManipulationEventContent> { }

        public class ManipulationEventContent : InteractionEventContent
        {
            public Vector3 translation;
            public Quaternion rotation;

            public ManipulationEventContent(UMI3DUser user, ManipulationRequestDto dto) : base(user, dto)
            {
                translation = dto.translation;
                rotation = dto.rotation;
            }

            public ManipulationEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType, Vector3 translation, Quaternion rotation) : base(user, toolId, id, hoveredObjectId, boneType)
            {
                this.translation = translation;
                this.rotation = rotation;
            }
        }

        /// <summary>
        /// Space referential.
        /// </summary>
        public UMI3DNode frameOfReference;

        /// <summary>
        /// List of DoF seperation options.
        /// </summary>
        public List<DofGroupOption> dofSeparationOptions;

        /// <summary>
        /// Event called on manipulation by user.
        /// </summary>
        public ManipulationListener onManipulated = new ManipulationListener();

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new ManipulationDto();
        }

        /// <summary>
        /// Write the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var mDto = dto as ManipulationDto;
            if (frameOfReference != null)
                mDto.frameOfReference = frameOfReference.Id();

            foreach (DofGroupOption entity in dofSeparationOptions)
                mDto.dofSeparationOptions.Add(entity.ToDto(user));
        }

        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.Manipulation;
        }

        public override Bytable ToByte(UMI3DUser user)
        {
            return base.ToByte(user)
                + UMI3DNetworkingHelper.Write(frameOfReference.Id())
                + UMI3DNetworkingHelper.WriteIBytableCollection(dofSeparationOptions);
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ManipulationRequestDto manip:
                    onManipulated.Invoke(new ManipulationEventContent(user, manip));
                    break;
            }
        }

        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.ManipulationRequest:
                    Vector3 translation = UMI3DNetworkingHelper.Read<Vector3>(container);
                    Quaternion rotation = UMI3DNetworkingHelper.Read<Quaternion>(container);
                    onManipulated.Invoke(new ManipulationEventContent(user, toolId, interactionId, hoverredId, boneType, translation, rotation));
                    break;
                default:
                    throw new System.Exception($"User interaction not supported ({operationId}) ");
            }
        }

        /// <summary>
        /// Degree of freedom group.
        /// </summary>
        [System.Serializable]
        public class DofGroup : IBytable
        {
            public string name;
            public DofGroupEnum dofs;

            public bool IsCountable()
            {
                return true;
            }

            public Bytable ToBytableArray(params object[] parameters)
            {
                return UMI3DNetworkingHelper.Write(name)
                    + UMI3DNetworkingHelper.Write((int)dofs);
            }

            /// <summary>
            /// Convert to dto for a given user.
            /// </summary>
            /// <param name="user">User to convert for</param>
            /// <returns></returns>
            public DofGroupDto ToDto(UMI3DUser user)
            {
                var dto = new DofGroupDto
                {
                    name = name,
                    dofs = dofs
                };
                return dto;
            }
        }

        /// <summary>
        /// List of DofGroup.
        /// </summary>
        [System.Serializable]
        public class DofGroupOption : IBytable
        {
            public string name;
            public List<DofGroup> separations = new List<DofGroup>();

            public bool IsCountable()
            {
                return true;
            }

            public Bytable ToBytableArray(params object[] parameters)
            {
                return UMI3DNetworkingHelper.Write(name)
                    + UMI3DNetworkingHelper.WriteIBytableCollection((IEnumerable<IBytable>)separations);
            }

            /// <summary>
            /// Convert to dto for a given user.
            /// </summary>
            /// <param name="user">User to convert for</param>
            /// <returns></returns>
            public DofGroupOptionDto ToDto(UMI3DUser user)
            {
                var dto = new DofGroupOptionDto
                {
                    name = name
                };
                foreach (DofGroup entity in separations)
                    dto.separations.Add(entity.ToDto(user));
                return dto;
            }
        }
    }
}