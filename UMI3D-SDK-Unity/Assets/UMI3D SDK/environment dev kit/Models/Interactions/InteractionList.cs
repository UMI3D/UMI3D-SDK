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
using UnityEngine;


namespace umi3d.edk
{   
    /// <summary>
    /// List of interaction.
    /// </summary>
    public class InteractionList : MonoBehaviour //AbstractInteraction<InteractionListDto>
    {
        /// <summary>
        /// Interaction list.
        /// </summary>
        public List<GenericInteraction> list = new List<GenericInteraction>();

        /*
        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override InteractionListDto CreateDto()
        {
            return new InteractionListDto();
        }

        
        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override InteractionListDto ToDto(UMI3DUser user)
        {
            InteractionListDto dto = base.ToDto(user);
            foreach (var i in list)
                if(i.AvailableAsChildFor(user))
                    dto.Interactions.Add(i.ConvertToDto(user));
            return dto;
        }*/

    }
}
