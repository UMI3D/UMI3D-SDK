using System.Collections.Generic;

namespace umi3d.common.interaction.form
{
    public abstract class DivDto : ItemDto
    {
        public string Type { get; set; }
        public string Tooltip { get; set; }
        public List<StyleDto> Styles { get; set; }
    }
}