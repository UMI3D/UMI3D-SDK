using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.edk.volume.volumedrawing
{
    public class VolumeSlice : MonoBehaviour, IVolumeDescriptor
    {

        /// <summary>
        /// in local referential.
        /// </summary>
        private List<Point> points = new List<Point>();

        /// <summary>
        /// in local referential.
        /// </summary>
        private List<Edge> edges = new List<Edge>();

        /// <summary>
        /// in local referential.
        /// </summary>
        private List<Face> faces = new List<Face>();


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

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entity = this,
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
            };
            return operation;
        }


        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            List<int> edges_ = new List<int>();
            foreach(Edge e in edges)
            {
                edges_.Add(points.IndexOf(e.from));
                edges_.Add(points.IndexOf(e.to));
            }

            VolumeSliceDto dto = new VolumeSliceDto()
            {
                id = Id(),
                points = points.ConvertAll(p => p.Id()),
                faces = faces.ConvertAll(p => p.Id()),
                edges = edges_
            };

            return dto;
        }

        private string id = "";
        public string Id()
        {
            if (id.Equals(""))
                id = Random.Range(0, 10000000000000000).ToString();
            return id;
        }


        #region filter
        HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion
    }
}