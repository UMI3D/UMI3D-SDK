using System.Collections;
using System.Collections.Generic;

namespace umi3d.common.volume
{
	public class VolumeDto : VolumePartDto
	{
		/// <summary>
		/// List of ths volume's slices.
		/// </summary>
		public List<VolumeSliceDto> slices;
	}
}