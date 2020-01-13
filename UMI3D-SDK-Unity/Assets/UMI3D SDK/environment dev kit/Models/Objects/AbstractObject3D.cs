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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Abstract scene object associated to a specific type of Data Tranfer Object.
    /// </summary>
    /// <typeparam name="DTO">type of Data Tranfer Object</typeparam>
    public abstract class AbstractObject3D<DTO> : GenericObject3D where DTO: AbstractObject3DDto
    {

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        public abstract DTO CreateDto();


        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public virtual DTO ToDto(UMI3DUser user)
        {
            SyncProperties();
            var dto = CreateDto();
            dto.Time = Time.realtimeSinceStartup;
            dto.Id = Id;
            dto.Pid = ParentId;
            dto.Name = gameObject.name;
            dto.ImmersiveOnly = objectImmersiveOnly.GetValue(user);
            dto.Billboard = objectBillboard.GetValue(user);
            dto.Position = objectPosition.GetValue(user);
            dto.Scale = objectScale.GetValue(user);
            dto.Rotation = objectRotation.GetValue(user);
            dto.TrackerDto = ARTracker?.ToDto(user);
            dto.Interactable = (isInteractable)? GetInteractableDto(user) : null;
            return dto;
        }


        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override AbstractObject3DDto ConvertToDto(UMI3DUser user)
        {
            return ToDto(user);
        }
        
        
    }

}
