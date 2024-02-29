using UnityEngine.UIElements;

namespace umi3d.common.interaction.form
{
    public class PositionStyleDto : VariantStyleDto
    {
        public Position Position { get; set; }

        public StyleLength Top { get; set; }
        public StyleLength Bottom { get; set; }
        public StyleLength Right { get; set; }
        public StyleLength Left { get; set; }
    }
}