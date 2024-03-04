namespace umi3d.common.interaction.form
{
    public class BaseInputDto : DivDto
    {
        public string label { get; set; }

        public ulong previousId { get; set; }
        public ulong nextId { get; set; }

        public bool isInteractable { get; set; }
        public bool submitOnValidate { get; set; }

        public virtual object GetPlaceHolder() { return null; }

        public virtual object GetValue() { return null; }
    }
}