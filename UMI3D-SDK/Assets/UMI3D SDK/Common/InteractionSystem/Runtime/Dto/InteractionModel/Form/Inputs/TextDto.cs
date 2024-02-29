namespace umi3d.common.interaction.form
{
    public class TextDto : InputDto<string>
    {
        public TextType Type { get; set; }
        public string PlaceHolder { get; set; }
    }
    public enum TextType
    {
        Text,
        Mail,
        Password,
        Phone,
        URL,
        Number
    }
}