using System.Collections;
using System.Collections.Generic;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.cdk.volumes
{
    public class VolumeSlice 
    {
        public VolumeSliceDto originalDto;
        public string id { get; private set; }
        private List<Point> points;
        private List<int> edges;
        private List<Face> faces;

        public VolumeSlice() { }
        public VolumeSlice(VolumeSliceDto dto)
        {
            this.originalDto = dto;
            this.id = dto.id;
            this.points = dto.points.ConvertAll(pid => VolumeSliceGroupManager.Instance.GetPoint(pid));
            this.edges = dto.edges;
            this.faces = dto.faces.ConvertAll(fid => VolumeSliceGroupManager.Instance.GetFace(fid));
        }

        public bool isInside(Vector3 point)
        {
            /*
             * Algorithm :
             *  Cast a random ray from the point and count the number of intersections with the mesh
             *  
             *  Create a random ray from the point
             *  Compute a new list of faces from the faces property to ensure planar faces
             *  Compute the bounding box of the volume slice
             *  for each face f in planar faces
             *      compute the plane of f
             *      compute the intersection of the ray and the plane
             *      if intersec if in bounding box
             *          check if the intersec is inside the face
             *      
             */
            int intersectionCount = 0;

            Ray randomRay = new Ray(point, Vector3.up);
            Bounds bounds = GeometryTools.ComputeBoundingBox(points.ConvertAll(p => p.position));
            List<GeometryTools.Face3> planarFaces = new List<GeometryTools.Face3>();
            foreach (Face f in faces)
            {
                planarFaces.AddRange(GeometryTools.GetPlanarSubFaces(f.ToFace3()));
            }

            foreach (GeometryTools.Face3 face in planarFaces)
            {
                Plane plane = GeometryTools.GetPlane(face.points);
                if (plane.Raycast(randomRay, out float enter))
                {
                    Vector3 intersection = plane.ClosestPointOnPlane(randomRay.origin + enter * randomRay.direction);
                    if (bounds.Contains(intersection))
                    {
                        if (face.isInside(intersection))
                        {
                            intersectionCount++;
                        }
                    }
                }
            }
            return (intersectionCount % 2) == 1;
        }

        public void SetPoints(List<PointDto> newPoints)
        {
            points = newPoints.ConvertAll(dto => VolumeSliceGroupManager.Instance.GetPoint(dto.id));
        }
        public void SetEdges(List<int> newEdges)
        {
            edges = newEdges;
        }
        public void SetFaces(List<FaceDto> newFaces)
        {
            faces = newFaces.ConvertAll(dto => VolumeSliceGroupManager.Instance.GetFace(dto.id));
        }
        public List<Point> GetPoints()
        {
            return points;
        }
        public List<int> GetEdges()
        {
            return edges;
        }
        public List<Face> GetFaces()
        {
            return faces;
        }
    }
}