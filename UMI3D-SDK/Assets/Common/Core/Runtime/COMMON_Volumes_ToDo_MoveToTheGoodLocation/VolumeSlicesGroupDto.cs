using System.Collections;
using System.Collections.Generic;

namespace umi3d.common.volume
{
	public class VolumeSlicesGroupDto : AbstractVolumeCellDto
	{
		/// <summary>
		/// List of ths volume's slices.
		/// </summary>
		public List<string> slicesIds;
	}
}