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

namespace umi3d.edk
{
    /// <summary>
    /// Abstract interaction associated to a specific type of Data Tranfer Object.
    /// </summary>
    /// <typeparam name="DTO">type of Data Tranfer Object</typeparam>
    public abstract class AbstractInteraction<DTO> : GenericInteraction where DTO : AbstractInteractionDto
    {
       
        /// <summary>
        /// Create an empty Dto for this interaction.
        /// </summary>
        /// <returns></returns>
        public abstract DTO CreateDto();

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>a DTO representing this interaction</returns>
        public virtual DTO ToDto(UMI3DUser user)
        {
            syncProperties();
            var dto = CreateDto();
            dto.Id = Id;
            dto.Name = currentInteractionName;
            dto.ToolId = currentTool == null ? null : currentTool.Id;
            dto.ToolboxId = 
                (currentTool != null)
                    && (currentTool is CVETool)
                    && (currentTool as CVETool).currentToolbox == null 
                ? (currentTool as CVETool).currentToolbox.Id
                : null;
            dto.Icon2D = Icon2D.ToDto();
            dto.Icon3D = Icon3D.ToDto();
            return dto;
        }

        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public override AbstractInteractionDto ConvertToDto(UMI3DUser user)
        {
            return ToDto(user);
        }

    }
}
