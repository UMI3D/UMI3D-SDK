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
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    public class CVEInteractable : AbstractCVETool
    {
        public override AbstractToolDto ConvertToDto(UMI3DUser user)
        {
            InteractableDto dto = new InteractableDto()
            {
                id = toolId,
                description = description,
                name = Display.name,
                icon2D = Display.icon2D.ToDto(),
                icon3D = Display.icon3D.ToDto()
            };

            IEnumerable<AbstractInteractionDto> tools = Interactions.Where(i => i != null && i.AvailableFor(user)).Select(i => i.ConvertToDto(user));
            dto.interactions.AddRange(tools);
            return dto;
        }

        public override bool UpdateAvailabilityForUser(UMI3DUser user)
        {
            bool wasAvailable = (availableLastFrame.ContainsKey(user)) ? availableLastFrame[user] : false;
            bool available = AvailableFor(user);

            return wasAvailable && available;
        }
    }
}