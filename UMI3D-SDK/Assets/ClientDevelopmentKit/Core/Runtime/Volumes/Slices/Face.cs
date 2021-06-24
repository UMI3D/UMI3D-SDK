using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common.volume;

namespace umi3d.cdk.volumes
{
	public class Face 
	{
        public Point[] points { get => points_.ToArray(); }
        public string id { get; private set; }
        private List<Point> points_;

        public void Setup(FaceDto dto)
        {
            points_ = dto.pointsIds.ConvertAll(id => VolumeSliceGroupManager.Instance.GetPoint(id));
            id = dto.id;
        }

        public void SetPoints(List<string> newPoints)
        {
            if (newPoints == null)
                throw new System.Exception("Internal error : points cannot be null");

            points_ = newPoints.ConvertAll(id => UMI3DEnvironmentLoader.GetEntity(id).Object as Point);
        }


        public GeometryTools.Face3 ToFace3()
        {
            GeometryTools.Face3 face3 = new GeometryTools.Face3()
            {
                points = this.points_.ConvertAll(p => p.position),
                edges = new List<GeometryTools.Line3>()
            };

            for(int i=0; i<points_.Count - 1; i++)
            {
                face3.edges.Add(new GeometryTools.Line3()
                {
                    from = points_[i].position,
                    to = points_[i + 1].position
                });
            }
            face3.edges.Add(new GeometryTools.Line3()
            {
                from = points_[points_.Count - 1].position,
                to = points_[0].position
            });
            return face3;
        }
	}
}