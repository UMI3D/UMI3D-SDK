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


namespace umi3d.edk
{
    /// <summary>
    /// Back end action.
    /// </summary>
    public class CVEAction : AbstractInteraction<ActionDto>
    {
        /// <summary>
        /// Event raised on user interaction.
        /// </summary>
        public UMI3DUserInteractionEvent onTriggered = new UMI3DUserInteractionEvent();

        /// <summary>
        /// 
        /// </summary>
        public List<CVEInput> inputs = new List<CVEInput>();

        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override ActionDto CreateDto()
        {
            return new ActionDto();
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            onTriggered.Invoke(user, evt);
        }

        /// <summary>
        /// Convert to ActionDto for a given user.
        /// </summary>
        public override ActionDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            foreach (var i in inputs)
                dto.Inputs.Add(i.ToDto(user));
            return dto;
        }

    }
}
