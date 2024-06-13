using System.Collections.Generic;

namespace umi3d.common.interaction.form
{
    public class GroupDto : BaseInputDto
    {
        public List<DivDto> children { get; set; }

        public bool canRemember { get; set; }
        public bool selectFirstInput { get; set; }
    }
}