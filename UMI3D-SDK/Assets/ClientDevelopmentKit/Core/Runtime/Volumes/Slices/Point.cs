using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common.volume;

namespace umi3d.cdk.volumes
{
	public class Point
	{
        public string id { get; private set; }
        public Vector3 position { get; private set; }

    	public void Setup(PointDto dto)
        {
            id = dto.id;
            position = dto.position;
        }

        public void SetPosition(Vector3 newPosition)
        {
            position = newPosition;
        }
	}
}