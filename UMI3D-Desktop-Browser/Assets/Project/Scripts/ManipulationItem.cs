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
using UnityEngine.UI;


namespace DesktopUI
{

    public class ManipulationItem : PictureItem
    {
        public Image background;
        public ManipulationDto dto;
        public DofGroupEnum dofGroup;
        public InteractionMapper mapper;
        

        private void Update()
        {
            if (mapper == null || dto == null)
                return;
            var selected = (mapper.currentDof == dto.Id  &&  mapper.currentDofGroup == dofGroup);
            if (background != null)
                background.color = selected ? Theme.PrincipalColor : Theme.SecondaryColor;
            if (label != null)
                label.color = selected ? Theme.PrincipalTextColor : Theme.SecondaryTextColor;
        }
    }

}