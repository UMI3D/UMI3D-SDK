using System.Collections.Generic;

namespace umi3d.common.interaction.form
{
    public abstract class DivDto : ItemDto
    {
        public string type { get; set; }
        public string tooltip { get; set; }
        public List<StyleDto> styles { get; set; }
    }
}