namespace umi3d.common.interaction.form
{
    public class Tooltip
    {
        public string Text { get; set; }
    }

    public class HelpTooltip : Tooltip
    {
        public int Order { get; set; }
    }
}