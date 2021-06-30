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
using UnityEngine;

namespace umi3d.common.volume
{

    public abstract class GeometryTools
    {
        /// <summary>
        /// Compute the barycenter of a point set.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector3 Barycenter(List<Vector3> points)
        {
            Vector3 center = Vector3.zero;
            foreach (var p in points)
                center += p * 1f / ((float)points.Count);

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
            Vector3 pointOnLine = Vector3.Project(point - linePoint, lineDirection);
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
            List<Vector3> pointsInPlaneReferential = new List<Vector3>();
            foreach (Vector3 p in points)
            {
                Vector3 pProjected = plane.ClosestPointOnPlane(p) - plane.normal * plane.distance;
                pointsInPlaneReferential.Add(
                    new Vector2(
                        Vector3.Dot(pProjected, u),
                        Vector3.Dot(pProjected, v)));
            }

            //step 3 : list edges in plane referential
            List<Line3> edges = new List<Line3>();
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
                Vector3 normal = Vector3.Cross(
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

                Line3 ray = new Line3()
                {
                    from = point,
                    to = ((points[0] + points[1]) / 2f - point).normalized * raySize + point
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
                List<Face3> triangles = new List<Face3>();

                for (int i = 0; i <= trianglesIndexes.Length - 3; i += 3)
                {
                    List<Vector3> points_ = new List<Vector3>();
                    List<Line3> edges_ = new List<Line3>();

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

            Dictionary<Vector3, List<Face3>> facesByNormals = new Dictionary<Vector3, List<Face3>>();
            foreach (Face3 triangle in triangles)
            {
                Vector3 normal = GetSurfaceNormal(triangle.points);

                List<Face3> trianglesWithSameNormal;
                if (facesByNormals.TryGetValue(normal, out trianglesWithSameNormal))
                    facesByNormals.Remove(normal);
                else
                    trianglesWithSameNormal = new List<Face3>();
                trianglesWithSameNormal.Add(triangle);
                facesByNormals.Add(normal, trianglesWithSameNormal);
            }

            List<Face3> export = new List<Face3>();
            foreach (var planeTris in facesByNormals)
            {
                Face3 plane = new Face3();
                plane.points = new List<Vector3>();
                plane.edges = new List<Line3>();

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
                Plane plane = new Plane();
                plane.Set3Points(this.from, this.to, other.from);

                if (Vector3.Distance(other.to, plane.ClosestPointOnPlane(other.to)) > 0.001f)
                    return false; //Not coplanars


                //Convert lines 3D coordinates into 2D plane coordinates
                Vector3 u = Vector3.zero;
                Vector3 v = Vector3.zero;
                while ((u.magnitude == 0) || (v.magnitude == 0))
                {
                    Vector3 randomPointA = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                    Vector3 randomPointB = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                    u = plane.ClosestPointOnPlane(randomPointA) - plane.distance * plane.normal;
                    v = plane.ClosestPointOnPlane(randomPointB) - plane.distance * plane.normal;
                }

                Vector2 from_ = new Vector2(Vector3.Dot(u, this.from), Vector3.Dot(v, this.from));
                Vector2 to_ = new Vector2(Vector3.Dot(u, this.to), Vector3.Dot(v, this.to));
                Vector2 otherFrom = new Vector2(Vector3.Dot(u, other.from), Vector3.Dot(v, other.from));
                Vector2 otherTo = new Vector2(Vector3.Dot(u, other.to), Vector3.Dot(v, other.to));


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
                            return (Y4 - Y3) / (X4 - X3) * (x - X3) + Y3;
                        }

                        float fx1 = f(X1);
                        return ((Mathf.Min(Y1, Y2) <= fx1) && (fx1 <= Mathf.Max(Y1, Y2)));
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
                            return (Y4 - Y3) / (X4 - X3) * (x - X3) + Y3;
                        }

                        float fx1 = f(X1);
                        return ((Mathf.Min(Y1, Y2) <= fx1) && (fx1 <= Mathf.Max(Y1, Y2)));
                    }
                }
                else
                {
                    float A1 = (Y1 - Y2) / (X1 - X2);
                    float A2 = (Y3 - Y4) / (X3 - X4);
                    float b1 = Y1 - A1 * X1;
                    float b2 = Y3 - A2 * X3;

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
            Line3 raycast = new Line3()
            {
                from = target,
                to = target + (polygon[1] - polygon[0]) * 2 * ComputeBoundingBox(polygon).size.magnitude
            };

            List<Line3> edges = new List<Line3>();
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
            Plane plane = new Plane();
            plane.Set3Points(a, b, c);

            //Convert points 3D coordinates into 2D plane coordinates
            Vector3 randomU = Vector3.zero;
            Vector3 randomV = Vector3.zero;
            while ((randomU.magnitude == 0) || (randomV.magnitude == 0))
            {
                Vector3 randomPointA = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                Vector3 randomPointB = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                randomU = plane.ClosestPointOnPlane(randomPointA) - plane.distance * plane.normal;
                randomV = plane.ClosestPointOnPlane(randomPointB) - plane.distance * plane.normal;
            }

            Vector2 target2D = new Vector2(Vector3.Dot(target, randomU), Vector3.Dot(target, randomV));
            Vector2 a2D = new Vector2(Vector3.Dot(a, randomU), Vector3.Dot(a, randomV));
            Vector2 b2D = new Vector2(Vector3.Dot(b, randomU), Vector3.Dot(b, randomV));
            Vector2 c2D = new Vector2(Vector3.Dot(c, randomU), Vector3.Dot(c, randomV));


            //algorithm from https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle

            float sign(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
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
            List<int> triangles = new List<int>();
            List<Vector3> buffer = new List<Vector3>(points);

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
            Plane uvplane = new Plane(normal, 0);

            Vector3 randomU = Vector3.zero;
            Vector3 randomV = Vector3.zero;
            while ((randomU.magnitude == 0) || (randomV.magnitude == 0))
            {
                Vector3 randomPointA = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                Vector3 randomPointB = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                randomU = uvplane.ClosestPointOnPlane(randomPointA) - uvplane.distance * uvplane.normal;
                randomV = uvplane.ClosestPointOnPlane(randomPointB) - uvplane.distance * uvplane.normal;
            }

            List<Vector2> rawUV = new List<Vector2>();
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

            List<Vector2> scaledPoints = new List<Vector2>();

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
                Vector3 normal = Vector3.Cross(
                    points[1] - points[0],
                    points[i + 1] - points[i]);

                surfaceNormal += normal * 1f / points.Count;
            }

            return surfaceNormal.normalized;
        }

        /// <summary>
        /// Get the faces of the base of a volume. 
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="angleLimit">Angle of a volume face's normal under which the face is considered horizontal.</param>
        /// <returns></returns>
        public static Mesh GetBase(Mesh volume, float angleLimit = 10)
        {
            /* Algorithm :
             *  - Get the lowest point
             *  - for each triangle using this point,
             *      - if the triangle is horizontal enough (see angleLimit), add it to the returned mesh
             *  - repeat for added points until no left triangle eligible. 
             */

            /// <summary>
            /// Get all triangles from volume involving a given point (returing triangles start index in volume.triangles).
            /// </summary>
            List<int> GetTriangles(int point)
            {
                List<int> triangles = new List<int>();

                for (int i = 0; i < volume.triangles.Length - 3; i += 3)
                {
                    if ((volume.triangles[i] == point) || (volume.triangles[i + 1] == point) || (volume.triangles[i + 2] == point))
                    {
                        triangles.Add(i);
                    }
                }

                return triangles;
            }

            Mesh baseSurface = new Mesh();
            List<int> baseSurfaceTrianglesIndexesInGlobalVolumeData = new List<int>();

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

            List<int> investigatedPoints = new List<int>();
            List<int> pointsToInvestigate = new List<int>();
            pointsToInvestigate.Add(lowestPointIndex);
            int watchdogMax = volume.vertexCount;
            int watchdog = 0;
            while ((pointsToInvestigate.Count > 0) && (watchdog <= watchdogMax))
            {
                watchdog++;
                List<int> pointsToAddToPointsToInvestigate = new List<int>();
                foreach (int p in pointsToInvestigate)
                {
                    investigatedPoints.Add(p);

                    List<int> trianglesIndexes = GetTriangles(p);
                    foreach (int triangleIndex in trianglesIndexes)
                    {
                        List<Vector3> triangle = new List<Vector3>();
                        triangle.Add(volume.vertices[volume.triangles[triangleIndex]]);
                        triangle.Add(volume.vertices[volume.triangles[triangleIndex + 1]]);
                        triangle.Add(volume.vertices[volume.triangles[triangleIndex + 2]]);

                        float angle = Vector3.Angle(GetSurfaceNormal(triangle), Vector3.up);
                        angle = Mathf.Min(angle, 180 - angle);
                        if (angle < angleLimit)
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
            baseSurface.RecalculateNormals();
            baseSurface.RecalculateTangents();
            baseSurface.RecalculateBounds();
            return baseSurface;
        }

        public static Mesh GetCylinder(Vector3 position, Quaternion rotation, Vector3 scale, float radius, float height, int subdiv = 16)
        {
            Mesh mesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<int> faces = new List<int>();

            for (int i = 0; i < subdiv; i++)
            {
                vertices.Add(position + rotation * Vector3.Scale(scale, Quaternion.Euler(i * 360f / subdiv * Vector3.up) * Vector3.right * radius));
            }
            for (int i = 0; i < subdiv; i++)
            {
                vertices.Add(position + rotation * Vector3.Scale(scale, (Quaternion.Euler(i * 360f / subdiv * Vector3.up) * Vector3.right * radius + height * Vector3.up)));
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

                faces.Add(2 * subdiv - 1);
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


    }
}