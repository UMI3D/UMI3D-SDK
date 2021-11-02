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
using System.Linq;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.volume
{
    /// <summary>
    /// Face of a volume slice.
    /// </summary>
    public class Face : AbstractMovableObject, IVolumeDescriptor
    {

        public Material material
        {
            get { return material_; }
            set
            {
                material_ = value;
                child.GetComponent<Renderer>().material = value;
            }
        }

        [SerializeField]
        private Material material_;

        private List<Point> points;

        private MeshFilter meshfilter;
        private new MeshCollider collider;



        private GameObject rotationWrapper;

        GameObject child;
        public virtual void Setup()
        {
            child = new GameObject("child");
            child.transform.parent = this.transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;


            meshfilter = child.AddComponent<MeshFilter>();
            MeshRenderer rnd = child.AddComponent<MeshRenderer>();
            rnd.material = material;
            collider = child.AddComponent<MeshCollider>();

        }

        public List<Edge> GetEdges()
        {
            List<Edge> edges = new List<Edge>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Edge edge = Edge.GetLine(points[i], points[i + 1]);
                if (edge == null)
                    throw new System.Exception("Something went wrong, edge should not be null");
                edges.Add(edge);
            }
            Edge lastEdge = Edge.GetLine(points[points.Count - 1], points[0]);
            if (lastEdge == null)
                throw new System.Exception("Something went wrong, edge should not be null");
            edges.Add(lastEdge);

            return edges;
        }


        private class PointUpdate
        {
            public Point point;
            public UnityAction<Vector3> pointPosition;
        }
        private List<PointUpdate> onPointsUpates = new List<PointUpdate>();
        public void SetPoints(List<Point> points)
        {

            this.points = new List<Point>(points);
            List<Vector3> pointsPos = points.ConvertAll<Vector3>(dp => dp.transform.position);

            Mesh surface = new Mesh();
            surface.SetVertices(pointsPos);

            List<int> triangles = new List<int>(GeometryTools.Triangulate(pointsPos));
            surface.SetTriangles(triangles, 0);

            meshfilter.mesh = surface;
            meshfilter.mesh.RecalculateBounds();
            meshfilter.mesh.RecalculateNormals();


            for (int i = 0; i < points.Count - 1; i++)
            {
                Edge.CreateOrGetLine(points[i], points[i + 1]);
            }
            Edge.CreateOrGetLine(points[points.Count - 1], points[0]);


            GeometryTools.UnwrapUV(meshfilter.mesh);

            PlaceOriginOnSurfaceBarycenter();
            WrapMeshFilter();
            UpdateCollider();

            #region point updates

            foreach (PointUpdate callback in onPointsUpates)
            {
                callback.point.UnsubscribeToMove(callback.pointPosition);
            }

            for (int i = 0; i < points.Count; i++)
            {
                int i_copy = i;
                PointUpdate pu = new PointUpdate()
                {
                    point = points[i],
                    pointPosition = pointPos =>
                    {
                        Vector3[] verts = meshfilter.mesh.vertices;
                        verts[i_copy] = this.transform.InverseTransformPoint(pointPos);
                        meshfilter.mesh.vertices = verts;
                        meshfilter.mesh.RecalculateBounds();
                        UpdateCollider();
                    }
                };
                onPointsUpates.Add(pu);
                points[i].SubscribeToMove(pu.pointPosition);
            }

            #endregion
        }
        private void UpdateCollider()
        {
            Vector3 surfaceNormal = GeometryTools.GetSurfaceNormal(new List<Vector3>(meshfilter.mesh.vertices));

            Mesh inflatedMesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            foreach (Vector3 v in meshfilter.mesh.vertices)
            {
                vertices.Add(v - surfaceNormal * 0.1f);
            }
            foreach (Vector3 v in meshfilter.mesh.vertices)
            {
                vertices.Add(v + surfaceNormal * 0.1f);
            }

            List<int> triangles = new List<int>(meshfilter.mesh.triangles);
            List<int> inflatedUpperFace = triangles.ConvertAll<int>(t => t + meshfilter.mesh.vertexCount);
            triangles.AddRange(inflatedUpperFace);

            for (int i = 0; i < meshfilter.mesh.vertexCount - 1; i++)
            {
                triangles.Add(i);
                triangles.Add(meshfilter.mesh.vertexCount + i + 1);
                triangles.Add(i + 1);

                triangles.Add(i);
                triangles.Add(meshfilter.mesh.vertexCount + i);
                triangles.Add(meshfilter.mesh.vertexCount + i + 1);
            }

            triangles.Add(meshfilter.mesh.vertexCount - 1);
            triangles.Add(meshfilter.mesh.vertexCount);
            triangles.Add(0);

            triangles.Add(meshfilter.mesh.vertexCount - 1);
            triangles.Add(meshfilter.mesh.vertexCount + meshfilter.mesh.vertexCount - 1);
            triangles.Add(meshfilter.mesh.vertexCount);



            inflatedMesh.vertices = vertices.ToArray();
            inflatedMesh.triangles = triangles.ToArray();
            inflatedMesh.RecalculateNormals();

            collider.sharedMesh = inflatedMesh;
        }

        public List<Point> GetPoints()
        {
            return points;
        }

        public override void MoveBegin()
        {
            foreach (Point p in points)
            {
                p.MoveBegin();
            }

            base.MoveBegin();
        }

        public override void MoveEnd()
        {
            foreach (Point p in points)
            {
                p.MoveEnd();
            }

            base.MoveEnd();
        }

        public override void Move(Vector3 translation)
        {
            this.transform.Translate(translation, Space.World);
            foreach (Point p in points)
            {
                p.Move(translation);
            }

            base.Move(translation);
        }

        public struct ExtrusionInfo
        {
            public Face extrusionFace;
            public VolumeSlice createdSlice;
        }

        /// <summary>
        /// Extrude a face and return the duplicated face.
        /// </summary>
        /// <param name="pointPrefab"></param>
        /// <returns></returns>
        public ExtrusionInfo Extrude(Point pointPrefab)
        {
            List<Face> newFaces = new List<Face>();
            List<Edge> newEdges = new List<Edge>();
            List<Point> newPoints = new List<Point>();

            #region duplicate the base surface upward ----------------------------------------------------------
            int basePointCount = points.Count;
            Vector3 surfaceNormal = GeometryTools.GetSurfaceNormal(points.ConvertAll(p => p.transform.position));
            List<Point> extrusionPoints = new List<Point>();
            for (int i = 0; i < basePointCount; i++)
            {
                Point p = Instantiate(pointPrefab, this.transform);
                p.transform.position = points[i].transform.position + surfaceNormal;

                extrusionPoints.Add(p);
                newPoints.Add(p);
            }
            extrusionPoints.Reverse();

            GameObject newFaceGO = new GameObject();
            newFaceGO.transform.parent = this.transform.parent;
            newFaceGO.name = "extrusion";
            Face newFace = newFaceGO.AddComponent<Face>();
            newFace.Setup();
            newFace.material = this.material;
            newFace.SetPoints(extrusionPoints);
            newFaces.Add(newFace);
            newEdges.AddRange(newFace.GetEdges());
            #endregion



            #region link the two surfaces -----------------------------------------------------------------------

            for (int i = 0; i < basePointCount - 1; i++)
            {
                List<Point> sidePoints = new List<Point>();
                sidePoints.Add(extrusionPoints[basePointCount - 1 - i]);
                sidePoints.Add(extrusionPoints[basePointCount - 1 - i - 1]);
                sidePoints.Add(points[i + 1]);
                sidePoints.Add(points[i]);


                GameObject sideFaceGO = new GameObject();
                sideFaceGO.transform.parent = this.transform.parent;
                sideFaceGO.name = "sideFace";
                Face sideFace = sideFaceGO.AddComponent<Face>();
                sideFace.Setup();
                sideFace.material = this.material;
                sideFace.SetPoints(sidePoints);

                newFaces.Add(sideFace);
                newEdges.AddRange(sideFace.GetEdges());
            }


            List<Point> lastSidePoints = new List<Point>();
            lastSidePoints.Add(extrusionPoints[0]);
            lastSidePoints.Add(extrusionPoints[basePointCount - 1]);
            lastSidePoints.Add(points[0]);
            lastSidePoints.Add(points[basePointCount - 1]);


            GameObject lastSideFaceGO = new GameObject();
            lastSideFaceGO.transform.parent = this.transform.parent;
            lastSideFaceGO.name = "sideFace";
            Face lastSideFace = lastSideFaceGO.AddComponent<Face>();
            lastSideFace.Setup();
            lastSideFace.material = this.material;
            lastSideFace.SetPoints(lastSidePoints);

            newFaces.Add(lastSideFace);
            newEdges.AddRange(lastSideFace.GetEdges());

            #endregion

            newFace.MoveBegin();
            newFace.Move(-surfaceNormal * .99f);
            newFace.MoveEnd();




            #region volume component -------------------------------------------------------------------
            GameObject volumeGO = new GameObject("Volume");
            VolumeSlice v = volumeGO.AddComponent<VolumeSlice>();

            List<Face> faces_ = new List<Face>();
            faces_.Add(this);
            faces_.AddRange(newFaces);

            List<Edge> edges_ = new List<Edge>();
            edges_.AddRange(this.GetEdges());
            edges_.AddRange(newEdges);

            List<Point> points_ = new List<Point>();
            points_.AddRange(this.points);
            points_.AddRange(newPoints);

            v.Setup(points_, edges_, faces_);
            #endregion


            return new ExtrusionInfo()
            {
                extrusionFace = newFace,
                createdSlice = v
            };
        }


        public void PlaceOriginOnSurfaceBarycenter()
        {
            Vector3 baricenter = Vector3.zero;

            foreach (Vector3 p in meshfilter.mesh.vertices)
            {
                baricenter += p / meshfilter.mesh.vertexCount;
            }

            Vector3 translation = baricenter - meshfilter.transform.position;

            List<Vector3> points = new List<Vector3>(meshfilter.mesh.vertices);
            points = points.ConvertAll(p => p - translation);
            meshfilter.mesh.vertices = points.ToArray();

            meshfilter.transform.position += translation;

            this.transform.position = meshfilter.transform.position;
            meshfilter.transform.localPosition = Vector3.zero;
        }

        public void WrapMeshFilter()
        {
            if (rotationWrapper != null)
                throw new System.Exception("Mesh already wrapped");

            Vector3 surfaceNormal = GeometryTools.GetSurfaceNormal(new List<Vector3>(meshfilter.mesh.vertices));
            rotationWrapper = new GameObject("Rotation wrapper");
            rotationWrapper.transform.parent = this.transform;
            rotationWrapper.transform.localPosition = Vector3.zero;
            rotationWrapper.transform.localScale = Vector3.one;

            rotationWrapper.transform.LookAt(meshfilter.transform.position + surfaceNormal);

            meshfilter.transform.parent = rotationWrapper.transform;
            meshfilter.transform.localRotation = Quaternion.Inverse(rotationWrapper.transform.localRotation);
            meshfilter.transform.localScale = Vector3.one;

        }



        public string shaderColorPropertyName = "_Albedo";
        private float originalSaturation;
        private float originalValue;
        private float originalAlpha;
        private Color originalColor;

        public override void DisableHighlight()
        {
            meshfilter.GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, originalColor);
        }

        public override void EnableHighlight()
        {
            originalColor = material.GetColor(shaderColorPropertyName);
            originalAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out float H, out originalSaturation, out originalValue);

            Color newColor = Color.HSVToRGB(H, Mathf.Lerp(originalSaturation, 1f, .5f), Mathf.Lerp(originalValue, 0f, .5f));
            newColor.a = originalAlpha;

            meshfilter.GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, newColor);
        }

        public void Display()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public GeometryTools.Face3 ToFace3()
        {
            GeometryTools.Face3 face3 = new GeometryTools.Face3()
            {
                points = this.points.ConvertAll(p => p.transform.position),
                edges = new List<GeometryTools.Line3>()
            };

            for (int i = 0; i < points.Count - 1; i++)
            {
                face3.edges.Add(new GeometryTools.Line3()
                {
                    from = points[i].transform.position,
                    to = points[i + 1].transform.position
                });
            }
            face3.edges.Add(new GeometryTools.Line3()
            {
                from = points[points.Count - 1].transform.position,
                to = points[0].transform.position
            });
            return face3;
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
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
            FaceDto dto = new FaceDto()
            {
                id = Id(),
                pointsIds = points.ConvertAll(p => p.Id())
            };

            return dto;
        }

        private ulong? id = null;
        public ulong Id()
        {
            if (id == null)
                id = UMI3DEnvironment.Register(this);
            return id.Value;
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

        public Bytable ToBytes(UMI3DUser user)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}