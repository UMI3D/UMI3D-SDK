﻿using System.Collections;
using System.Collections.Generic;

namespace umi3d.common.volume
{
	public class PointDto : AbstractEntityDto, IEntity
	{
		public SerializableVector3 position;
	}
}