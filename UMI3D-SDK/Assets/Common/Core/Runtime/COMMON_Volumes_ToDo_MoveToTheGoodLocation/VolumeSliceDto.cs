using System.Collections;
using System.Collections.Generic;

namespace umi3d.common.volume
{
	public class VolumeSliceDto : AbstractEntityDto, IEntity
	{
		public List<string> points;
		public List<int> edges;
		public List<string> faces;
	}
}