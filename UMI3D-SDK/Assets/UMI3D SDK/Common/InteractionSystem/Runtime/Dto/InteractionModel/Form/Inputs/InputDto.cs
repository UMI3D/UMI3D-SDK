namespace umi3d.common.interaction.form
{
    public class InputDto<T> : BaseInputDto
    {
        public T Value { get; set; }

        public override object GetValue() => Value;
    }
}