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

using System;

namespace umi3d.common
{
    /// <summary>
    /// An operation to inform a browser the value of an entity's property changed.
    /// </summary>
    [Serializable]
    public class SetEntityPropertyDto : AbstractOperationDto
    {
        /// <summary>
        /// The unique identifier of the entity
        /// </summary>
        public string entityId;

        /// <summary>
        /// The name of the modified property
        /// </summary>
        public string property;

        /// <summary>
        /// The new value for the property
        /// </summary>
        public object value;

        /// <summary>
        /// Make a Copy of a SetEntityPropertyDto or a class inheriting it.
        /// </summary>
        /// <returns></returns>
        public SetEntityPropertyDto Copy()
        {
            var dto = CreateDto();
            CopyProperties(dto);
            return dto;
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected virtual SetEntityPropertyDto CreateDto()
        {
            return new SetEntityPropertyDto();
        }

        /// <summary>
        /// Copy the properties in a given SetEntityPropertyDto  or a class inheriting it.
        /// </summary>
        /// <param name="dto">The SetEntityPropertyDt to be completed</param>
        /// <returns></returns>
        protected virtual void CopyProperties(SetEntityPropertyDto dto)
        {
            dto.entityId = entityId;
            dto.property = property;
            dto.value = value;
        }
    }
}