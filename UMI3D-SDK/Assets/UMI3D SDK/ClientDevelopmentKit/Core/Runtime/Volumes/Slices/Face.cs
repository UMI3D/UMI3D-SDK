/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;
using umi3d.common.volume;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Volume slice face.
    /// </summary>
	public class Face 
	{
        public Point[] points { get => points_.ToArray(); }
        public ulong id { get; private set; }
        private List<Point> points_;

        public void Setup(FaceDto dto)
        {
            points_ = dto.pointsIds.ConvertAll(id => VolumeSliceGroupManager.Instance.GetPoint(id));
            id = dto.id;
        }

        public void SetPoints(List<ulong> newPoints)
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