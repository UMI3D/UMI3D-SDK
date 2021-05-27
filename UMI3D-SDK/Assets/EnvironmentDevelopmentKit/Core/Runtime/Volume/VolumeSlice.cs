using System.Collections;
using System.Collections.Generic;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.edk.volume.volumedrawing
{
    public class VolumeSlice : MonoBehaviour
    {

        /// <summary>
        /// in local referential.
        /// </summary>
        private List<Point> points;

        /// <summary>
        /// in local referential.
        /// </summary>
        private List<Edge> edges;

        /// <summary>
        /// in local referential.
        /// </summary>
        private List<Face> faces;


        [SerializeField] private bool displayEdges;
        [SerializeField] private bool displayFaces;
        [SerializeField] private bool displayPoints;

        private void OnDrawGizmos()
        {
            if (displayEdges)
            {
                Gizmos.color = Color.red;
                foreach (Edge line in edges)
                {
                    Gizmos.DrawLine(line.from.transform.position, line.to.transform.position);
                }
            }

            if (displayPoints)
            {
                Gizmos.color = Color.green;
                foreach (Point p in points)
                {
                    Gizmos.DrawSphere(p.transform.position, 0.1f);
                }
            }

            if (displayFaces)
            {
                Gizmos.color = Color.blue;
                if (faces == null)
                    return;
                foreach (Face f in faces)
                {
                    List<Point> points = f.GetPoints();
                    Gizmos.DrawLine(points[0].transform.position, points[1].transform.position);
                    Gizmos.DrawLine(points[1].transform.position, points[2].transform.position);
                    Gizmos.DrawLine(points[2].transform.position, points[0].transform.position);
                }
            }
        }


        public List<Point> GetPoints() => points;
        public List<Edge> GetEdges() => edges;
        public List<Face> GetFaces() => faces;


        public void Setup(List<Point> points, List<Edge> edges, List<Face> faces)
        {
            this.points = points;
            this.edges = edges;
            this.faces = faces;

            this.transform.position = GeometryTools.Barycenter(points.ConvertAll(p => p.transform.position));
        }

        /// <summary>
        /// Check if a point is inside the slice.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
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

            Ray randomRay = new Ray(point, new Vector3(Random.value, Random.value, Random.value).normalized);
            Bounds bounds = GeometryTools.ComputeBoundingBox(points.ConvertAll(p => p.transform.position));
            List<GeometryTools.Face3> planarFaces = new List<GeometryTools.Face3>();
            foreach (var f in faces)
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

        public void MoveBegin()
        {
            throw new System.NotImplementedException(); //todo
        }

        public void MoveEnd()
        {
            throw new System.NotImplementedException(); //todo
        }

        public void Move(Vector3 translation)
        {
            throw new System.NotImplementedException(); //todo
        }

        public void Display()
        {
            foreach (var p in points)
                p.Display();
            foreach (var l in edges)
                l.Display();
            foreach (var f in faces)
                f.Display();
        }

        public void Hide()
        {
            foreach (var p in points)
                p.Hide();
            foreach (var l in edges)
                l.Hide();
            foreach (var f in faces)
                f.Hide();
        }
    }
}