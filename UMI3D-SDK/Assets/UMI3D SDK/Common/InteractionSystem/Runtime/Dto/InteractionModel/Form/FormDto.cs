using System.Collections.Generic;

namespace umi3d.common.interaction.form
{
    public class FormDto : ItemDto
    {
        public string name { get; set; }
        public string description { get; set; }

        public List<PageDto> pages { get; set; }
    }

}