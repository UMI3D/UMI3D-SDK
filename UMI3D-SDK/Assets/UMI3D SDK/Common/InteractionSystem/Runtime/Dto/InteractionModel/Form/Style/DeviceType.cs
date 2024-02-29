using System;

namespace umi3d.common.interaction.form
{
    [Flags]
	public enum DeviceType
	{
		Screen = 1,
		Vr = 2,

		All = Screen | Vr,
	}
}