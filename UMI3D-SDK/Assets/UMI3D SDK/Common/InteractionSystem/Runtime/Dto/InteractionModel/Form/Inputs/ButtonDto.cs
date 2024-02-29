namespace umi3d.common.interaction.form
{
    public class ButtonDto : BaseInputDto
    {
        public ButtonType Type { get; set; }
    }
    public enum ButtonType
    {
        None,
        Submit,
        Reset,
        Cancel,
        Back
    }
}