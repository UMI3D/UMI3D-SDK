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
using UnityEngine;

namespace umi3d.common.volume
{
    /// <summary>
    /// Contains several useful functions for geometry computing.
    /// </summary>
    public abstract class GeometryTools
    {
        /// <summary>
        /// For logging purpose.
        /// </summary>
        private static string ListToString<T>(List<T> list)
        {
            if (list.Count == 0)
                return "[]";

            string display = "[";
            list.ForEach(e => display += e.ToString() + ", ");
            display = display.Remove(display.Length - 3);
            display += "]";
            return display;
        }

        /// <summary>
        /// Compute the barycenter of a point set.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector3 Barycenter(List<Vector3> points)
        {
            Vector3 center = Vector3.zero;
            foreach (Vector3 p in points)
                center += p * 1f / points.Count;

            return center;
        }

        /// <summary>
        /// Calculate the distance of a given point to a line.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineDirection"></param>
        /// <param name="linePoint"></param>
        /// <returns></returns>
        public static float DistanceToLine(Vector3 point, Vector3 lineDirection, Vector3 linePoint)
        {
            var pointOnLine = Vector3.Project(point - linePoint, lineDirection);
            return (point - linePoint - pointOnLine).magnitude;
        }


        /// <summary>
        /// Check if a polygon intersect itself.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static bool IsSurfaceSelfIntersecting(List<Vector3> points)
        {
            //step 1 : construct plane
            Plane plane = GetPlane(points);

            //u,v are a base for the plane in world referential.
            Vector3 u = Vector3.ProjectOnPlane(Vector3.forward, plane.normal).normalized; //forward is a random vector
            Vector3 v = Vector3.Cross(plane.normal, u).normalized;

            //step 2 : convert all points in the plane referential
            var pointsInPlaneReferential = new List<Vector3>();
            foreach (Vector3 p in points)
            {
                Vector3 pProjected = plane.ClosestPointOnPlane(p) - (plane.normal * plane.distance);
                pointsInPlaneReferential.Add(
                    new Vector2(
                        Vector3.Dot(pProjected, u),
                        Vector3.Dot(pProjected, v)));
            }

            //step 3 : list edges in plane referential
            var edges = new List<Line3>();
            for (int i = 0; i < pointsInPlaneReferential.Count - 1; i++)
            {
                edges.Add(new Line3()
                {
                    from = pointsInPlaneReferential[i],
                    to = pointsInPlaneReferential[i + 1]
                });
            }
            edges.Add(new Line3()
            {
                from = pointsInPlaneReferential[pointsInPlaneReferential.Count - 1],
                to = pointsInPlaneReferential[0]
            });

            //step 4 : check for edges intersection
            foreach (Line3 edgeA in edges)
            {
                foreach (Line3 edgeB in edges)
                {
                    //if the edge are not connected
                    if ((Vector3.Distance(edgeA.from, edgeB.to) > 0.0001f) && (Vector3.Distance(edgeA.to, edgeB.from) > 0.0001f))
                    {
                        if (edgeA.Intersect(edgeB))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if all points of a list can be contained in a single plane.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="threeshold"></param>
        /// <returns></returns>
        public static bool IsSurfacePlanar(List<Vector3> points, float threeshold = 0.01f)
        {
            if (points.Count < 3)
                throw new System.Exception("Not enouth points");

            if (points.Count == 3)
                return true;

            //create a candidate plane.
            Plane plane = GetPlane(points);


            //check if the plane is correct.
            foreach (Vector3 p in points)
            {
                if (Mathf.Abs(plane.GetDistanceToPoint(p)) >= threeshold)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Return the best plane that contains every points (if planar).
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Plane GetPlane(List<Vector3> points)
        {
            Vector3 planeCenter = Vector3.zero;

            foreach (Vector3 p in points)
            {
                planeCenter += p * 1f / points.Count;
            }

            Vector3 planeNormal = Vector3.zero;
            for (int i = 1; i < points.Count - 1; i++)
            {
                var normal = Vector3.Cross(
                    points[i] - planeCenter,
                    points[i + 1] - planeCenter);

                planeNormal += normal * 1f / points.Count;
            }


            return new Plane(planeNormal, planeCenter);
        }

        /// <summary>
        /// Find an ear in the polygon and return the first index of the 3 consecutives points composing the eat.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static int FindEar(List<Vector3> points)
        {
            for (int i = 0; i < points.Count - 2; i++)
            {
                if (IsInsidePolygon((points[i] + points[i + 1] + points[i + 2]) / 3f, points))
                {
                    bool isThereAnyPointInsideThisTriangle = false;
                    for (int j = 0; j < points.Count; j++)
                    {
                        if ((j < i) || (j > i + 2))
                        {
                            if (IsInTriangle(points[j], points[i], points[i + 1], points[i + 2]))
                            {
                                isThereAnyPointInsideThisTriangle = true;
                            }
                        }
                    }
                    if (!isThereAnyPointInsideThisTriangle)
                        return i;
                }
            }
            throw new System.Exception("FindEar : Something went wrong !");
        }


        public static Bounds ComputeBoundingBox(List<Vector3> points)
        {
            Vector3 center = Vector3.zero;
            Vector3 min = float.MaxValue * Vector3.one;
            Vector3 max = float.MinValue * Vector3.one;

            foreach (Vector3 p in points)
            {
                center += p * 1f / points.Count;

                if (p.x < min.x)
                    min.x = p.x;
                if (p.y < min.y)
                    min.y = p.y;
                if (p.z < min.z)
                    min.z = p.z;

                if (p.x > max.x)
                    max.x = p.x;
                if (p.y > max.y)
                    max.y = p.y;
                if (p.z > max.z)
                    max.z = p.z;
            }

            return new Bounds(center, max - min);

        }

        /// <summary>
        /// Merge meshes into one.
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        public static Mesh Merge(IEnumerable<Mesh> meshes)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            int trisOffset = 0;

            foreach (Mesh mesh in meshes)
            {
                verts.AddRange(mesh.vertices);
                tris.AddRange(mesh.triangles.ToList().ConvertAll(i => i + trisOffset));
                trisOffset += mesh.vertexCount;
            }

            var merged = new Mesh
            {
                vertices = verts.ToArray(),
                triangles = tris.ToArray()
            };
            merged.RecalculateNormals();
            merged.RecalculateBounds();
            merged.RecalculateTangents();
            merged.Optimize();
            return merged;
        }


        public class Face3
        {
            public List<Vector3> points;
            public List<Line3> edges;

            public Face3() { }

            /// <summary>
            /// Return true if the point is inside the face.
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public bool isInside(Vector3 point)
            {
                /*
                 * Random raycast and count intersections.
                 */

                ///Dummy size upper limit.
                float raySize = 0;
                foreach (Vector3 p in points)
                {
                    raySize += Mathf.Abs((p - point).magnitude);
                }

                var ray = new Line3()
                {
                    from = point,
                    to = ((((points[0] + points[1]) / 2f) - point).normalized * raySize) + point
                };

                int interCount = 0;

                foreach (Line3 line in edges)
                {
                    if (line.Intersect(ray))
                        interCount++;
                }

                return (interCount % 2) == 1;
            }

            public List<Face3> Triangulate()
            {
                int[] trianglesIndexes = GeometryTools.Triangulate(points);
                var triangles = new List<Face3>();

                for (int i = 0; i <= trianglesIndexes.Length - 3; i += 3)
                {
                    var points_ = new List<Vector3>();
                    var edges_ = new List<Line3>();

                    points_.Add(points[trianglesIndexes[i + 0]]);
                    points_.Add(points[trianglesIndexes[i + 1]]);
                    points_.Add(points[trianglesIndexes[i + 2]]);

                    edges_.Add(new Line3()
                    {
                        from = points[trianglesIndexes[i + 0]],
                        to = points[trianglesIndexes[i + 1]]
                    });
                    edges_.Add(new Line3()
                    {
                        from = points[trianglesIndexes[i + 1]],
                        to = points[trianglesIndexes[i + 2]]
                    });
                    edges_.Add(new Line3()
                    {
                        from = points[trianglesIndexes[i + 2]],
                        to = points[trianglesIndexes[i + 0]]
                    });

                    triangles.Add(new Face3()
                    {
                        points = points_,
                        edges = edges_
                    });
                }

                return triangles;
            }

            public Plane GetPlane()
            {
                return GeometryTools.GetPlane(points);
            }
        }

        /// <summary>
        /// If the face is not planar, return a list a faces such as every face is planar and the union of faces equals this face.
        /// Returned faces may have loose parts.
        /// </summary>
        /// <returns></returns>
        public static List<Face3> GetPlanarSubFaces(Face3 face)
        {
            /*
             * 1- Separate the face into triangles
             * 2- Group the triangle by normal
             * 3- Construct faces of same normal
             */

            List<Face3> triangles = face.Triangulate();

            var facesByNormals = new Dictionary<Vector3, List<Face3>>();
            foreach (Face3 triangle in triangles)
            {
                Vector3 normal = GetSurfaceNormal(triangle.points);

                if (facesByNormals.TryGetValue(normal, out List<Face3> trianglesWithSameNormal))
                    facesByNormals.Remove(normal);
                else
                    trianglesWithSameNormal = new List<Face3>();
                trianglesWithSameNormal.Add(triangle);
                facesByNormals.Add(normal, trianglesWithSameNormal);
            }

            var export = new List<Face3>();
            foreach (KeyValuePair<Vector3, List<Face3>> planeTris in facesByNormals)
            {
                var plane = new Face3
                {
                    points = new List<Vector3>(),
                    edges = new List<Line3>()
                };

                foreach (Face3 tri in planeTris.Value)
                {
                    foreach (Vector3 point in tri.points)
                    {
                        if (!plane.points.Contains(point))
                        {
                            plane.points.Add(point);
                        }
                    }

                    //check here for inside edges removal
                    foreach (Line3 edge in tri.edges)
                    {
                        if (plane.edges.Contains(edge))
                            plane.edges.Remove(edge);
                        else
                            plane.edges.Add(edge);
                    }
                }

                export.Add(plane);
            }
            return export;
        }

        public class Line3
        {
            public Vector3 from;
            public Vector3 to;

            public bool Intersect(Line3 other)
            {
                //plane detection                                
                var plane = new Plane();
                plane.Set3Points(this.from, this.to, other.from);

                if (Vector3.Distance(other.to, plane.ClosestPointOnPlane(other.to)) > 0.001f)
                    return false; //Not coplanars


                //Convert lines 3D coordinates into 2D plane coordinates
                Vector3 u = Vector3.zero;
                Vector3 v = Vector3.zero;
                while ((u.magnitude == 0) || (v.magnitude == 0))
                {
                    var randomPointA = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                    var randomPointB = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                    u = plane.ClosestPointOnPlane(randomPointA) - (plane.distance * plane.normal);
                    v = plane.ClosestPointOnPlane(randomPointB) - (plane.distance * plane.normal);
                }

                var from_ = new Vector2(Vector3.Dot(u, this.from), Vector3.Dot(v, this.from));
                var to_ = new Vector2(Vector3.Dot(u, this.to), Vector3.Dot(v, this.to));
                var otherFrom = new Vector2(Vector3.Dot(u, other.from), Vector3.Dot(v, other.from));
                var otherTo = new Vector2(Vector3.Dot(u, other.to), Vector3.Dot(v, other.to));


                ///notations and algo from : https://stackoverflow.com/questions/3838329/how-can-i-check-if-two-segments-intersect
                float X1 = from_.x;
                float Y1 = from_.y;
                float X2 = to_.x;
                float Y2 = to_.y;
                float X3 = otherFrom.x;
                float Y3 = otherFrom.y;
                float X4 = otherTo.x;
                float Y4 = otherTo.y;

                if (Mathf.Max(X1, X2) < Mathf.Min(X3, X4))
                    return false;

                if (X1 == X2) // from is vertical (NOT TESTED YET ! --> not working yet ...)
                {
                    if (X3 == X4) //lines are parallels
                    {
                        if (X1 != X3)
                        {
                            return false;
                        }
                        else
                        {
                            ///see : https://stackoverflow.com/questions/3269434/whats-the-most-efficient-way-to-test-two-integer-ranges-for-overlap
                            float x1 = Mathf.Min(Y1, Y2);
                            float x2 = Mathf.Max(Y1, Y2);
                            float y1 = Mathf.Min(Y3, Y4);
                            float y2 = Mathf.Max(Y3, Y4);
                            return (x1 <= y2) && (y1 <= x2);
                        }
                    }
                    else
                    {
                        //1- calculate the equation f(x) of the line "other".
                        //2- evalute f(X1) and check if it is on the line "from".
                        float f(float x)
                        {
                            return ((Y4 - Y3) / (X4 - X3) * (x - X3)) + Y3;
                        }

                        float fx1 = f(X1);
                        return (Mathf.Min(Y1, Y2) <= fx1) && (fx1 <= Mathf.Max(Y1, Y2));
                    }
                }
                else if (X3 == X4) //same as above but for other.
                {
                    X1 = otherFrom.x;
                    Y1 = otherFrom.y;
                    X2 = otherTo.x;
                    Y2 = otherTo.y;
                    X3 = from_.x;
                    Y3 = from_.y;
                    X4 = to_.x;
                    Y4 = to_.y;

                    if (X3 == X4) //lines are parallels
                    {
                        if (X1 != X3)
                            return false;
                        else
                        {
                            ///see : https://stackoverflow.com/questions/3269434/whats-the-most-efficient-way-to-test-two-integer-ranges-for-overlap
                            float x1 = Mathf.Min(Y1, Y2);
                            float x2 = Mathf.Max(Y1, Y2);
                            float y1 = Mathf.Min(Y3, Y4);
                            float y2 = Mathf.Max(Y3, Y4);
                            return (x1 <= y2) && (y1 <= x2);
                        }
                    }
                    else
                    {
                        //1- calculate the equation f(x) of the line "other".
                        //2- evalute f(X1) and check if it is on the line "from".
                        float f(float x)
                        {
                            return ((Y4 - Y3) / (X4 - X3) * (x - X3)) + Y3;
                        }

                        float fx1 = f(X1);
                        return (Mathf.Min(Y1, Y2) <= fx1) && (fx1 <= Mathf.Max(Y1, Y2));
                    }
                }
                else
                {
                    float A1 = (Y1 - Y2) / (X1 - X2);
                    float A2 = (Y3 - Y4) / (X3 - X4);
                    float b1 = Y1 - (A1 * X1);
                    float b2 = Y3 - (A2 * X3);

                    if (A1 == A2) //parallel
                        return false;

                    float Xa = (b2 - b1) / (A1 - A2);
                    return (Xa >= Mathf.Max(Mathf.Min(X1, X2), Mathf.Min(X3, X4)))
                        && (Xa <= Mathf.Min(Mathf.Max(X1, X2), Mathf.Max(X3, X4)));
                }
            }
        }


        /// <summary>
        /// Return true if a point is inside a plygon described by a list of points.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool IsInsidePolygon(Vector3 target, List<Vector3> polygon)
        {
            var raycast = new Line3()
            {
                from = target,
                to = target + ((polygon[1] - polygon[0]) * 2 * ComputeBoundingBox(polygon).size.magnitude)
            };

            var edges = new List<Line3>();
            for (int i = 0; i < polygon.Count - 1; i++)
            {

                edges.Add(new Line3()
                {
                    from = polygon[i],
                    to = polygon[i + 1]
                });
            }
            edges.Add(new Line3() { from = polygon[polygon.Count - 1], to = polygon[0] });

            int intersectionCount = 0;
            foreach (Line3 edge in edges)
            {
                if (edge.Intersect(raycast))
                {
                    intersectionCount++;
                }
            }

            return (intersectionCount % 2) != 0;
        }

        /// <summary>
        /// Is target inside of the triangle abc ?
        /// </summary>
        /// <param name="target"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsInTriangle(Vector3 target, Vector3 a, Vector3 b, Vector3 c)
        {

            //plane detection                                
            var plane = new Plane();
            plane.Set3Points(a, b, c);

            //Convert points 3D coordinates into 2D plane coordinates
            Vector3 randomU = Vector3.zero;
            Vector3 randomV = Vector3.zero;
            while ((randomU.magnitude == 0) || (randomV.magnitude == 0))
            {
                var randomPointA = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                var randomPointB = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                randomU = plane.ClosestPointOnPlane(randomPointA) - (plane.distance * plane.normal);
                randomV = plane.ClosestPointOnPlane(randomPointB) - (plane.distance * plane.normal);
            }

            var target2D = new Vector2(Vector3.Dot(target, randomU), Vector3.Dot(target, randomV));
            var a2D = new Vector2(Vector3.Dot(a, randomU), Vector3.Dot(a, randomV));
            var b2D = new Vector2(Vector3.Dot(b, randomU), Vector3.Dot(b, randomV));
            var c2D = new Vector2(Vector3.Dot(c, randomU), Vector3.Dot(c, randomV));


            //algorithm from https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle

            float sign(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return ((p1.x - p3.x) * (p2.y - p3.y)) - ((p2.x - p3.x) * (p1.y - p3.y));
            }

            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(target2D, a2D, b2D);
            d2 = sign(target2D, b2D, c2D);
            d3 = sign(target2D, c2D, a2D);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }


        /// <summary>
        /// Triangulate a polygone (all points must be differents!).
        /// Return the index of the triangles' points as in Unity's Mesh triangle property.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static int[] Triangulate(List<Vector3> points)
        {
            if (points.Count < 3)
                throw new System.Exception("Not enough points (min is 3)");

            //ear clipping method
            var triangles = new List<int>();
            var buffer = new List<Vector3>(points);

            while (buffer.Count > 3)
            {
                int earIndex = FindEar(buffer);
                triangles.Add(points.IndexOf(buffer[earIndex]));
                triangles.Add(points.IndexOf(buffer[earIndex + 1]));
                triangles.Add(points.IndexOf(buffer[earIndex + 2]));

                if (earIndex < buffer.Count - 1)
                    buffer.RemoveAt(earIndex + 1);
                else
                    buffer.RemoveAt(0);
            }

            triangles.Add(points.IndexOf(buffer[0]));
            triangles.Add(points.IndexOf(buffer[1]));
            triangles.Add(points.IndexOf(buffer[2]));

            return triangles.ToArray();
        }


        public static void UnwrapUV(Mesh mesh)
        {
            Vector3 normal = GeometryTools.GetSurfaceNormal(new List<Vector3>(mesh.vertices));
            var uvplane = new Plane(normal, 0);

            Vector3 randomU = Vector3.zero;
            Vector3 randomV = Vector3.zero;
            while ((randomU.magnitude == 0) || (randomV.magnitude == 0))
            {
                var randomPointA = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                var randomPointB = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                randomU = uvplane.ClosestPointOnPlane(randomPointA) - (uvplane.distance * uvplane.normal);
                randomV = uvplane.ClosestPointOnPlane(randomPointB) - (uvplane.distance * uvplane.normal);
            }

            var rawUV = new List<Vector2>();
            foreach (Vector3 vert in mesh.vertices)
            {
                rawUV.Add(new Vector2(
                    Vector3.Dot(vert, randomU),
                    Vector3.Dot(vert, randomV)));
            }

            Vector2[] uv = Normalize(rawUV).ToArray();

            mesh.SetUVs(0, uv);
        }

        /// <summary>
        /// Return a copy of the given list scaled to fit within a square of size 1.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<Vector2> Normalize(List<Vector2> points)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (Vector2 p in points)
            {
                if (p.x < minX)
                    minX = p.x;
                if (p.y < minY)
                    minY = p.y;
                if (p.x > maxX)
                    maxX = p.x;
                if (p.y > maxY)
                    maxY = p.y;
            }

            var scaledPoints = new List<Vector2>();

            foreach (Vector2 p in points)
            {
                scaledPoints.Add(new Vector2(
                    Mathf.InverseLerp(minX, maxX, p.x),
                    Mathf.InverseLerp(minY, maxY, p.y)));
            }

            return scaledPoints;
        }

        public static Vector3 GetSurfaceNormal(List<Vector3> points)
        {
            Vector3 surfaceCenter = Vector3.zero;

            foreach (Vector3 p in points)
            {
                surfaceCenter += p * 1f / points.Count;
            }

            Vector3 surfaceNormal = Vector3.zero;
            for (int i = 1; i < points.Count - 1; i++)
            {
                var normal = Vector3.Cross(
                    points[1] - points[0],
                    points[i + 1] - points[i]);

                surfaceNormal += normal * 1f / points.Count;
            }

            return surfaceNormal.normalized;
        }

        /// <summary>
        /// Merge vertices when their distance to each others is bellow a given threeshold.
        /// </summary>
        public static Mesh MergeVerticesByDistance(Mesh mesh, float threeshold = 0.01f)
        {
            int vertCount = mesh.vertexCount;
            for (int i = 0; i < vertCount; i++)
            {
                for (int j = 0; j < vertCount; j++)
                {
                    if (i != j)
                    {
                        if (Vector3.Distance(mesh.vertices[i], mesh.vertices[j]) < threeshold)
                        {
                            mesh = MergeVertices(mesh, i, j);
                            vertCount--;
                            j--;
                        }
                    }
                }
            }
            return mesh;
        }

        /// <summary>
        /// Find the two points from mesh that are the closest to each other, and return their indexes.
        /// </summary>
        public static Vector2Int GetClosestPoints(Mesh mesh)
        {
            if (mesh.vertexCount < 2)
                throw new System.Exception("Mesh must have at least 2 vertices.");

            float minDistance = Vector3.Distance(mesh.vertices[0], mesh.vertices[1]);
            var closests = new Vector2Int(0, 1);

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                for (int j = i + 1; j < mesh.vertexCount; j++)
                {
                    float dist = Vector3.Distance(mesh.vertices[i], mesh.vertices[j]);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closests = new Vector2Int(i, j);
                    }
                }
            }

            return closests;
        }

        /// <summary>
        /// Merge two vertices into the first vertex location.
        /// </summary>
        /// <returns></returns>
        public static Mesh MergeVertices(Mesh mesh, int vertice1, int vertice2)
        {
            /* Algorithm :
             *  - Replace each triangle with vertice2 index with the vertice1 index.
             *  - Remove the vertice2 index from the vertices array.
             *  - Shift the indexes in the triangles to match with the new vertices array.
             */

            var vertices = mesh.vertices.ToList();
            var triangles = mesh.triangles.ToList();

            triangles = triangles.ConvertAll(index => (index == vertice2) ? vertice1 : index);
            vertices.RemoveAt(vertice2);
            triangles = triangles.ConvertAll(index => (index >= vertice2) ? index - 1 : index);

            var result = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            return result;
        }

        /// <summary>
        /// Force all triangles to face upward.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Mesh ForceNormalUp(Mesh mesh)
        {
            var triangles = mesh.triangles.ToList();

            for (int i = 0; i + 2 < triangles.Count; i += 3)
            {
                Vector3 barycenter = (mesh.vertices[triangles[i]] + mesh.vertices[triangles[i + 1]] + mesh.vertices[triangles[i + 2]]) / 3f;

                bool normalUp = Vector3.Dot(Vector3.Cross(mesh.vertices[triangles[i]] - barycenter, mesh.vertices[triangles[i + 1]] - barycenter), Vector3.up) >= 0;
                if (!normalUp)
                {
                    int buffer = triangles[i];
                    triangles[i] = triangles[i + 1];
                    triangles[i + 1] = buffer;
                }
            }

            var result = new Mesh
            {
                vertices = mesh.vertices,
                triangles = triangles.ToArray()
            };
            return result;
        }

        /// <summary>
        /// Return true if the list contains the sequence in the same order.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static bool ListContainsSequence<T>(List<T> list, List<T> sequence)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Equals(sequence[0]))
                {
                    bool allEqual = true;
                    for (int j = 1; j < sequence.Count; j++)
                    {
                        if (i + j >= list.Count)
                            return false;

                        if (!list[i + j].Equals(sequence[j]))
                        {
                            allEqual = false;
                            break;
                        }
                    }
                    if (allEqual)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Compute the area of a triangle.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static float GetArea(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            float base_ = Vector3.Distance(point1, point2);
            float height = DistanceToLine(point3, point2 - point1, point1);
            return base_ * height / 2f;
        }

        public static bool IsTriangleFlat(List<Vector3> triangle)
        {
            return GetArea(triangle[0], triangle[1], triangle[2]) == 0;
        }

        public static Mesh RemoveFlatTriangles(Mesh mesh)
        {
            var tris = mesh.triangles.ToList();
            int offset = 0;
            for (int i = 0; i < mesh.triangles.Length - 2; i += 3)
            {
                if (IsTriangleFlat(new List<Vector3> { mesh.vertices[tris[i - offset]], mesh.vertices[tris[i + 1 - offset]], mesh.vertices[tris[i + 2 - offset]] }))
                {
                    tris.RemoveAt(i - offset);
                    tris.RemoveAt(i - offset);
                    tris.RemoveAt(i - offset);
                    offset += 3;
                }
            }

            var result = new Mesh
            {
                vertices = mesh.vertices,
                triangles = tris.ToArray()
            };
            result.Optimize();
            return result;
        }

        /// <summary>
        /// Get the faces of the base of a volume. 
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="angleLimit">Angle of a volume face's normal under which the face is considered horizontal.</param>
        /// <param name="stepLimit">(TO IMPLEMENTED YET) Include small steps in he base, in a similar way than navmesh auto generation.</param>
        /// <param name="pointsMergeDistance">Define the distance within which multiple points close to each others are merged into one. If negative, no merge will be performed.</param>
        /// <returns></returns>
        public static Mesh GetBase(Mesh volume, float angleLimit, float stepLimit, float pointsMergeDistance = -1)
        {
            /* Algorithm :
             *  - Merge points if needed
             *  - Get the lowest point
             *  - for each triangle using this point,
             *      - if the triangle is horizontal enough (see angleLimit), add it to the returned mesh
             *  - repeat for added points until no left triangle eligible. 
             */


            var verticeToTriangles = new Dictionary<int, List<int>>();
            /// <summary>
            /// Get all triangles from volume involving a given point (returing triangles start index in volume.triangles).
            /// </summary>
            List<int> GetTriangles(int point)
            {
                if (verticeToTriangles.TryGetValue(point, out List<int> triangles_))
                    return triangles_;

                var triangles = new List<int>();

                for (int i = 0; i < volume.triangles.Length - 3; i += 3)
                {
                    if ((volume.triangles[i] == point) || (volume.triangles[i + 1] == point) || (volume.triangles[i + 2] == point))
                    {
                        triangles.Add(i);
                    }
                }

                verticeToTriangles.Add(point, triangles);
                return triangles;
            }

            if (pointsMergeDistance > 0)
            {
                volume = MergeVerticesByDistance(volume, pointsMergeDistance);
            }

            volume.Optimize(); //somehow managed to avoid wierd bugs ...
            volume = RemoveFlatTriangles(volume);

            var baseSurface = new Mesh();
            var baseSurfaceTrianglesIndexesInGlobalVolumeData = new List<int>();

            int lowestPointIndex = -1;
            float lowestPosition = float.MaxValue;
            for (int i = 0; i < volume.vertices.Length; i++)
            {
                if (volume.vertices[i].y < lowestPosition)
                {
                    lowestPosition = volume.vertices[i].y;
                    lowestPointIndex = i;
                }
            }

            var investigatedPoints = new List<int>();
            var pointsToInvestigate = new List<int>
            {
                lowestPointIndex
            };
            int watchdogMax = volume.vertexCount;
            int watchdog = 0;
            while ((pointsToInvestigate.Count > 0) && (watchdog <= watchdogMax))
            {
                watchdog++;
                var pointsToAddToPointsToInvestigate = new List<int>();
                foreach (int p in pointsToInvestigate)
                {
                    investigatedPoints.Add(p);

                    List<int> trianglesIndexes = GetTriangles(p);

                    foreach (int triangleIndex in trianglesIndexes)
                    {
                        var triangle = new List<Vector3>
                        {
                            volume.vertices[volume.triangles[triangleIndex]],
                            volume.vertices[volume.triangles[triangleIndex + 1]],
                            volume.vertices[volume.triangles[triangleIndex + 2]]
                        };
                        Vector3 faceNormal = GetSurfaceNormal(triangle);
                        float angle = Vector3.Angle(faceNormal, Vector3.up);

                        angle = Mathf.Min(angle, 180 - angle);
                        if ((angle < angleLimit) && !ListContainsSequence(baseSurfaceTrianglesIndexesInGlobalVolumeData, new List<int>() { volume.triangles[triangleIndex], volume.triangles[triangleIndex + 1], volume.triangles[triangleIndex + 2] }))
                        {
                            baseSurfaceTrianglesIndexesInGlobalVolumeData.Add(volume.triangles[triangleIndex]);
                            baseSurfaceTrianglesIndexesInGlobalVolumeData.Add(volume.triangles[triangleIndex + 1]);
                            baseSurfaceTrianglesIndexesInGlobalVolumeData.Add(volume.triangles[triangleIndex + 2]);

                            int trianglePointA = volume.triangles[triangleIndex];
                            int trianglePointB = volume.triangles[triangleIndex + 1];
                            int trianglePointC = volume.triangles[triangleIndex + 2];

                            if (!investigatedPoints.Contains(trianglePointA))
                                if (!pointsToAddToPointsToInvestigate.Contains(trianglePointA))
                                    pointsToAddToPointsToInvestigate.Add(trianglePointA);

                            if (!investigatedPoints.Contains(trianglePointB))
                                if (!pointsToAddToPointsToInvestigate.Contains(trianglePointB))
                                    pointsToAddToPointsToInvestigate.Add(trianglePointB);

                            if (!investigatedPoints.Contains(trianglePointC))
                                if (!pointsToAddToPointsToInvestigate.Contains(trianglePointC))
                                    pointsToAddToPointsToInvestigate.Add(trianglePointC);
                        }
                    }
                }

                pointsToInvestigate.Clear();
                pointsToInvestigate.AddRange(pointsToAddToPointsToInvestigate);
            }



            List<Vector3> baseSurfaceVertices = baseSurfaceTrianglesIndexesInGlobalVolumeData.ConvertAll<Vector3>(ti => volume.vertices[ti]);
            List<int> baseSurfaceTriangles = baseSurfaceTrianglesIndexesInGlobalVolumeData.ConvertAll<int>(ti => baseSurfaceVertices.IndexOf(volume.vertices[ti]));
            baseSurface.vertices = baseSurfaceVertices.ToArray();
            baseSurface.triangles = baseSurfaceTriangles.ToArray();

            if (pointsMergeDistance > 0)
                baseSurface = MergeVerticesByDistance(baseSurface, pointsMergeDistance);

            baseSurface.RecalculateNormals();
            baseSurface.RecalculateTangents();
            baseSurface.RecalculateBounds();
            return baseSurface;
        }

        public static Mesh GetCylinder(Vector3 position, Quaternion rotation, Vector3 scale, float radius, float height, int subdiv = 16)
        {
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var faces = new List<int>();

            for (int i = 0; i < subdiv; i++)
            {
                vertices.Add(position + Vector3.Scale(scale, rotation * Quaternion.Euler(i * 360f / subdiv * Vector3.up) * Vector3.right * radius));
            }
            for (int i = 0; i < subdiv; i++)
            {
                vertices.Add(position + Vector3.Scale(scale, rotation * ((Quaternion.Euler(i * 360f / subdiv * Vector3.up) * Vector3.right * radius) + (height * Vector3.up))));
            }

            for (int i = 0; i < subdiv - 1; i++)
            {
                faces.Add(i);
                faces.Add(i + 1);
                faces.Add(i + subdiv);

                faces.Add(i + 1);
                faces.Add(i + subdiv + 1);
                faces.Add(i + subdiv);

                faces.Add(subdiv - 1);
                faces.Add(i + 1);
                faces.Add(i);

                faces.Add((2 * subdiv) - 1);
                faces.Add(subdiv + i);
                faces.Add(subdiv + i + 1);
            }

            faces.Add(subdiv - 1);
            faces.Add(0);
            faces.Add(subdiv + subdiv - 1);

            faces.Add(0);
            faces.Add(subdiv);
            faces.Add(subdiv - 1 + subdiv);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = faces.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.OptimizeReorderVertexBuffer();
            return mesh;
        }

        public static Mesh GetBox(Vector3 position, Quaternion rotation, Vector3 scale, Bounds bounds)
        {
            var matrix = Matrix4x4.TRS(position, rotation, scale);
            return GetBox(matrix, bounds);
        }

        public static Mesh GetBox(Matrix4x4 transform, Bounds bounds)
        {
            var points = new List<Vector3>
            {
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(0, 0, 0), bounds.size)),
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(0, 0, 1), bounds.size)),
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(0, 1, 0), bounds.size)),
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(0, 1, 1), bounds.size)),
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(1, 0, 0), bounds.size)),
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(1, 0, 1), bounds.size)),
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(1, 1, 0), bounds.size)),
                transform.MultiplyPoint(bounds.min + Vector3.Scale(new Vector3(1, 1, 1), bounds.size))
            };

            var tris = new List<int>()
            {
                1,5,7,
                1,7,3,
                0,4,1,
                1,4,5,
                2,3,7,
                2,7,6,
                0,1,3,
                0,3,2,
                4,7,5,
                4,6,7,
                0,6,4,
                0,2,6
            };


            var mesh = new Mesh
            {
                vertices = points.ToArray(),
                triangles = tris.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.Optimize();
            return mesh;
        }

        /// <summary>
        /// Check if a point is inside a mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsInside(Mesh mesh, Vector3 point)
        {
            int interCount = 0;
            var ray = new Ray(point, point + (mesh.bounds.size * 1.1f));

            for (int i = 0; i < mesh.triangles.Length - 2; i += 3)
            {
                var triangle = new List<Vector3>()
                {
                    mesh.vertices[mesh.triangles[i]],
                    mesh.vertices[mesh.triangles[i+1]],
                    mesh.vertices[mesh.triangles[i+2]]
                };
                if (GetPlane(triangle).Raycast(ray, out float enter))
                {
                    if (IsInTriangle(point + (ray.direction * enter), triangle[0], triangle[1], triangle[2]))
                    {
                        interCount++;
                    }
                }
            }

            return (interCount % 2) == 1;
        }
    }
}