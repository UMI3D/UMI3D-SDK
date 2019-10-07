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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk
{
    /// <summary>
    /// Object selection.
    /// </summary>
    public class Pick : AbstractInteraction<PickDto>
    {
        [System.Serializable]
        public class PickListener : UnityEvent<UMI3DUser> { }
        
        /// <summary>
        /// Object to pick.
        /// </summary>
        public GenericObject3D target;

        /// <summary>
        /// Called on user pick.
        /// </summary>
        [SerializeField]
        public PickListener onPicked = new PickListener();

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override PickDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);

            dto.Target = target.Id;

            return dto;
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            onPicked.Invoke(user);
        }

        /// <summary>
        /// Create an empty Dto for this interaction.
        /// </summary>
        /// <returns></returns>
        public override PickDto CreateDto()
        {
            return new PickDto();
        }


        /// <summary>
        /// Return true if this interaction is available for the given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check availability for</param>
        /// <returns></returns>
        public override bool AvailableAsChildFor(UMI3DUser user)
        {
            if (target == null)
                return false;
            else
                return base.AvailableAsChildFor(user) && target.VisibleFor(user);
        }

        /// <summary>
        /// Return true if this interaction is available for the given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check availability for</param>
        /// <returns></returns>
        public override bool AvailableFor(UMI3DUser user)
        {
            if (target == null)
                return false;
            else
                return base.AvailableFor(user) && target.VisibleFor(user);
        }
    }
}