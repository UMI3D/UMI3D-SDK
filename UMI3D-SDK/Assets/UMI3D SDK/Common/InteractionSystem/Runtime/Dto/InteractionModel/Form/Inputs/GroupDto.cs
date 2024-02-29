using System.Collections.Generic;

namespace umi3d.common.interaction.form
{
    public class GroupDto : BaseInputDto
    {
        public List<DivDto> Children { get; set; }

        public bool CanRemember { get; set; }
        public bool SelectFirstInput { get; set; }
    }
}