namespace umi3d.common.interaction.form
{
    public class ButtonDto : BaseInputDto
    {
        public ButtonType ButtonType { get; set; }
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