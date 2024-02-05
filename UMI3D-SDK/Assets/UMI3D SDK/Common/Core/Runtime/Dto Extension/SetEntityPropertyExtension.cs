/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.common
{
    public static class SetEntityPropertyExtension 
    {
        /// <summary>
        /// Make a Copy of a SetEntityPropertyDto or a class inheriting it.
        /// </summary>
        /// <returns></returns>
        static public SetEntityPropertyDto Copy(this SetEntityPropertyDto origin)
        {
            SetEntityPropertyDto dto = CreateDto();
            origin.CopyProperties(dto);
            return dto;
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        static private SetEntityPropertyDto CreateDto()
        {
            return new SetEntityPropertyDto();
        }

        /// <summary>
        /// Copy the properties in a given SetEntityPropertyDto  or a class inheriting it.
        /// </summary>
        /// <param name="dto">The SetEntityPropertyDt to be completed</param>
        /// <returns></returns>
        static private void CopyProperties(this SetEntityPropertyDto origin, SetEntityPropertyDto dto)
        {
            dto.entityId = origin.entityId;
            dto.property = origin.property;
            dto.value = origin.value;
        }
    }
}