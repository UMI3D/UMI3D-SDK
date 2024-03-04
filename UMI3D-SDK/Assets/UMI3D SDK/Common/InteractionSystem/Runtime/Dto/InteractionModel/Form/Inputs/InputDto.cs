namespace umi3d.common.interaction.form
{
    public class InputDto<T> : BaseInputDto
    {
        public T Value { get; set; }

        public T PlaceHolder { get; set; }

        public override object GetPlaceHolder() => PlaceHolder;

        public override object GetValue() => Value;
    }
}