using System.Collections;
using System.Collections.Generic;

namespace umi3d.common.volume
{
	public class FaceDto : AbstractEntityDto, IEntity
	{
		public List<string> pointsIds;
	}
}