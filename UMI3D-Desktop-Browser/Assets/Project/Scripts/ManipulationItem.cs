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