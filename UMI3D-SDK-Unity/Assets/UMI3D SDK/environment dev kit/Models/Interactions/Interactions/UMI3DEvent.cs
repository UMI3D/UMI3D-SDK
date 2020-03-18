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


namespace umi3d.edk
{
    /// <summary>
    /// Back end action.
    /// </summary>
    public class UMI3DEvent : AbstractInteraction<EventDto>
    {

        public bool Hold = false;
        
        /// <summary>
        /// Called the first frame of hoverring.
        /// </summary>
        [SerializeField]
        public UMI3DUserBoneEvent onHold = new UMI3DUserBoneEvent();

        /// <summary>
        /// Called the first frame after hoverring.
        /// </summary>
        [SerializeField]
        public UMI3DUserBoneEvent onRelease = new UMI3DUserBoneEvent();

        /// <summary>
        /// Called the first frame after hoverring.
        /// </summary>
        [SerializeField]
        public UMI3DUserBoneEvent onTrigger = new UMI3DUserBoneEvent();


        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override EventDto CreateDto()
        {
            return new EventDto();
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, JSONObject arg)
        {
            if (arg == null)
                throw new System.Exception("Argument is missing");

            if (arg.type == JSONObject.Type.ARRAY)
            {
                List<JSONObject> args = arg.list;
                if (args.Count < 2)
                    throw new System.Exception("Invalid argument");

                JSONObject evt = args[0];
                string bone = args[1].str;

                if (Hold)
                {
                    if ((evt != null) && evt.IsBool)
                    {
                        if (evt.b)
                        {
                            onHold.Invoke(user, bone);
                        }
                        else
                        {
                            onRelease.Invoke(user, bone);
                        }
                    }
                }
                else
                {
                    onTrigger.Invoke(user, bone);
                }
            }
        }
        

        /// <summary>
        /// Convert to ActionDto for a given user.
        /// </summary>
        public override EventDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            dto.Hold = Hold;
            return dto;
        }

    }
}
