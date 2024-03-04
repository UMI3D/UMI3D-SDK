namespace umi3d.common.interaction.form
{
    public class BaseInputDto : DivDto
    {
        public string Label { get; set; }

        public ulong PreviousId { get; set; }
        public ulong NextId { get; set; }

        public bool IsInteractable { get; set; }
        public bool SubmitOnValidate { get; set; }

        public virtual object GetPlaceHolder() { return null; }

        public virtual object GetValue() { return null; }
    }
}