using System.Collections;
using System.Collections.Generic;

namespace umi3d.common.volume
{
	public class VolumeSliceDto : VolumePartDto
	{
		public string id;
		public List<string> points;
		public List<int> edges;
		public List<string> faces;
	}
}