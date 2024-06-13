using System.Collections.Generic;
using System.Linq;
using umi3d.common.interaction.form;

public class EnumDto<T, G> : BaseInputDto
    where T : EnumValue<G> 
    where G : DivDto
{
    public List<T> values { get; set; }
    public bool canSelectMultiple { get; set; }

    public override object GetValue() => values.Where(e => e.isSelected);
}

public class EnumValue<G> where G : DivDto
{
    public G item { get; set; }
    public bool isSelected { get; set; }
}