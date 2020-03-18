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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace umi3d.edk
{
    /// <summary>
    /// Abstract UMI3D interaction.
    /// </summary>
    public class CVETool : AbstractCVETool
    {             
        
        internal CVEToolbox currentToolbox = null;
        [SerializeField] internal CVEToolbox toolbox = null;

        public void SetToolbox(CVEToolbox toolbox)
        {
            if (currentToolbox == toolbox)
                return;
            if (currentToolbox != null)
                currentToolbox.RemoveTool(this);
            if (toolbox != null)
                toolbox.AddTool(this);
        }

        /// <summary>
        /// Initialize interaction's properties.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();
            SetToolbox(toolbox);
        }


        /// <summary>
        /// Unity MonoBehaviour OnValidate method.
        /// </summary>
        protected virtual void OnValidate()
        {
            SetToolbox(toolbox);
        }



        public override bool UpdateAvailabilityForUser(UMI3DUser user)
        {
            bool wasAvailable = (availableLastFrame.ContainsKey(user)) ? availableLastFrame[user] : false;
            bool available = AvailableFor(user);

            if (wasAvailable && !available)
                user.ToolsIdsToRemove.Add(Id);

            if (!wasAvailable && available)
                user.ToolsToLoad.Add(ConvertToDto(user) as ToolDto);

            return wasAvailable && available;
        }





        /// <summary>
        /// Convert interaction to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractInteractionDto representing this interaction</returns>
        public override AbstractToolDto ConvertToDto(UMI3DUser user)
        {
            ToolDto dto = new ToolDto();
            dto.id = toolId;
            dto.name = Display.name;
            dto.icon2D = Display.icon2D.ToDto();
            dto.icon3D = Display.icon3D.ToDto();
            dto.toolboxId = currentToolbox == null ? null : currentToolbox.Id;
            IEnumerable<AbstractInteractionDto> tools = Interactions.Where(i => i != null && i.AvailableFor(user)).Select(i => i.ConvertToDto(user));
            dto.interactions.AddRange(tools);
            return dto;
        }

    }

}
