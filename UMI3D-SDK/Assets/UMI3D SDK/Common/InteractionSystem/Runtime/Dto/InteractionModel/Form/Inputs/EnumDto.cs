using System.Collections.Generic;
using System.Linq;
using umi3d.common.interaction.form;

public class EnumDto<T, G> : BaseInputDto
    where T : EnumValue<G> 
    where G : DivDto
{
    public List<T> Values { get; set; }
    public bool CanSelectMultiple { get; set; }

    public override object GetValue() => Values.Where(e => e.IsSelected);
}

public class EnumValue<G> where G : DivDto
{
    public G Item { get; set; }
    public bool IsSelected { get; set; }
}