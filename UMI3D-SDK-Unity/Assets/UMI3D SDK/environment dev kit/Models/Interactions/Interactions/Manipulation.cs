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
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk
{
    /// <summary>
    /// 3D Object Manipulation
    /// </summary>
    public class Manipulation : AbstractInteraction<ManipulationDto>
    {
        [System.Serializable]
        public class ManipulationListener : UnityEvent<UMI3DUser,Vector3,Quaternion> { }
        
        /// <summary>
        /// Space referential.
        /// </summary>
        public AbstractObject3D frameOfReference;

        /// <summary>
        /// List of DoF seperation options.
        /// </summary>
        public List<DofGroupOption> dofSeparationOptions;

        /// <summary>
        /// Event called on manipulation by user.
        /// </summary>
        public ManipulationListener onManipulated = new ManipulationListener();

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override ManipulationDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);

            if (frameOfReference != null)
                dto.frameOfReference = frameOfReference.Id;

            foreach (var entity in this.dofSeparationOptions)
                dto.dofSeparationOptions.Add(entity.ToDto(user));
            return dto;
        }

        /// <summary>
        /// Initialise the component.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();
        }


        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt == null)
                return;

            var res = DtoUtility.Deserialize(evt);
            var manip = res as ManipulationParametersDto;
            var translation = manip.Translation;
            var rotation = manip.Rotation;
            onManipulated.Invoke(user, translation, rotation);
        }
        
        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override ManipulationDto CreateDto()
        {
            return new ManipulationDto();
        }

        /// <summary>
        /// Degree of freedom group.
        /// </summary>
        [System.Serializable]
        public class DofGroup 
        {
            public string name;
            public DofGroupEnum dofs;

            /// <summary>
            /// Convert to dto for a given user.
            /// </summary>
            /// <param name="user">User to convert for</param>
            /// <returns></returns>
            public DofGroupDto ToDto(UMI3DUser user)
            {
                var dto = new DofGroupDto();
                dto.name = this.name;
                dto.dofs = this.dofs;
            return dto;
            }
        }

        /// <summary>
        /// List of DofGroup.
        /// </summary>
        [System.Serializable]
        public class DofGroupOption
        {
            public string name;
            public List<DofGroup> separations = new List<DofGroup>();

            /// <summary>
            /// Convert to dto for a given user.
            /// </summary>
            /// <param name="user">User to convert for</param>
            /// <returns></returns>
            public DofGroupOptionDto ToDto(UMI3DUser user)
            {
                var dto = new DofGroupOptionDto();
                dto.name = this.name;
                foreach (DofGroup entity in this.separations)
                    dto.separations.Add(entity.ToDto(user));
            return dto;
            }
        }


    }
}