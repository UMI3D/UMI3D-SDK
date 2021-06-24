using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.edk.volume.volumedrawing
{
    public class Edge : AbstractMovableObject
    {

        public static Material material;
        public static Dictionary<Point, Dictionary<Point, Edge>> instances = new Dictionary<Point, Dictionary<Point, Edge>>();

        /// <summary>
        /// Number of faces using this edges.
        /// </summary>
        public int faceCount = 0;
        public Point from;
        public Point to;

        private GameObject displayer;
        private MeshFilter filter;
        private new CapsuleCollider collider;

        /// <summary>
        /// Create a line linking from and to, or return the existing line if any.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Edge CreateOrGetLine(Point from, Point to)
        {
            Edge l = GetLine(from, to);
            if (l != null)
                return l;

            else
            {
                GameObject newLineGO = new GameObject("line from " + from + " to " + to);
                newLineGO.transform.parent = from.transform;
                Edge edge = newLineGO.AddComponent<Edge>();
                edge.from = from;
                edge.to = to;

                from.SubscribeToMove(_ => edge.UpdateDisplay());
                from.SubscribeToMoveEnd(() => edge.UpdateDisplay());
                to.SubscribeToMove(_ => edge.UpdateDisplay());
                to.SubscribeToMoveEnd(() => edge.UpdateDisplay());

                RegisterLine(edge);
                edge.UpdateDisplay();
                return edge;
            }
        }

        /// <summary>
        /// Delete a line (real delete only if the line is not used by any other face).
        /// </summary>
        /// <param name="line"></param>
        public static void DeleteLine(Edge edge)
        {
            edge.faceCount--;

            if (edge.faceCount <= 0)
            {
                if (instances.TryGetValue(edge.from, out Dictionary<Point, Edge> dic))
                {
                    if (dic.ContainsKey(edge.to))
                    {
                        dic.Remove(edge.to);
                        Destroy(edge.gameObject);
                    }
                }
                else if (instances.TryGetValue(edge.to, out Dictionary<Point, Edge> dic2))
                {
                    if (dic2.ContainsKey(edge.from))
                    {
                        dic2.Remove(edge.from);
                        Destroy(edge.gameObject);
                    }
                }
                else
                {
                    throw new System.Exception("Something went wrong while deleting the line from instances.");
                }

            }
        }

        private static void RegisterLine(Edge line)
        {
            if (instances.TryGetValue(line.from, out Dictionary<Point, Edge> fromDic))
            {
                if (fromDic.TryGetValue(line.to, out Edge l))
                {
                    return;
                }
            }
            else if (instances.TryGetValue(line.to, out Dictionary<Point, Edge> toDic))
            {
                if (toDic.TryGetValue(line.from, out Edge l))
                {
                    return;
                }
            }

            if (!instances.ContainsKey(line.from))
            {
                instances.Add(line.from, new Dictionary<Point, Edge>());
            }

            instances[line.from].Add(line.to, line);

        }

        public static Edge GetLine(Point from, Point to)
        {
            if (instances.TryGetValue(from, out Dictionary<Point, Edge> fromDic))
            {
                if (fromDic.TryGetValue(to, out Edge line))
                {
                    return line;
                }
            }
            else if (instances.TryGetValue(to, out Dictionary<Point, Edge> toDic))
            {
                if (toDic.TryGetValue(from, out Edge line))
                {
                    return line;
                }
            }
            return null;
        }

        protected virtual void Awake()
        {
            displayer = new GameObject();
            displayer.transform.parent = this.transform;
            displayer.transform.position = Vector3.zero;

            filter = displayer.AddComponent<MeshFilter>();
            filter.gameObject.SetActive(false);//TODO HERE remove
            displayer.AddComponent<MeshRenderer>().material = material;

            GameObject colliderGO = new GameObject();
            colliderGO.transform.parent = this.transform;
            collider = colliderGO.AddComponent<CapsuleCollider>();


            UpdateDisplay();
        }

        protected virtual void UpdateDisplay()
        {
            if ((to == null) || (from == null))
                return;

            this.transform.position = .5f * (from.transform.position + to.transform.position);

            Mesh mesh = filter.mesh = new Mesh();

            Vector3 edgeDirection = (to.transform.position - from.transform.position).normalized;
            Vector3 localUp = Vector3.Dot(Vector3.up, edgeDirection) > .9f ? -Vector3.forward : Vector3.ProjectOnPlane(Vector3.up, edgeDirection);
            Vector3 localRight = Vector3.Cross(edgeDirection, localUp).normalized;
            float size = 0.01f;

            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(this.transform.InverseTransformPoint(from.transform.position + localUp * .6f * size));
            vertices.Add(this.transform.InverseTransformPoint(from.transform.position - localUp * .3f * size + localRight * .5f * size));
            vertices.Add(this.transform.InverseTransformPoint(from.transform.position - localUp * .3f * size - localRight * .5f * size));
            vertices.Add(this.transform.InverseTransformPoint(to.transform.position + localUp * .6f * size));
            vertices.Add(this.transform.InverseTransformPoint(to.transform.position - localUp * .3f * size + localRight * .5f * size));
            vertices.Add(this.transform.InverseTransformPoint(to.transform.position - localUp * .3f * size - localRight * .5f * size));

            List<int> triangle = new List<int>();

            triangle.Add(0);
            triangle.Add(2);
            triangle.Add(1);

            triangle.Add(3 + 0);
            triangle.Add(3 + 1);
            triangle.Add(3 + 2);

            triangle.Add(0);
            triangle.Add(0 + 3);
            triangle.Add(2);

            triangle.Add(0 + 3);
            triangle.Add(2 + 3);
            triangle.Add(2);

            triangle.Add(0);
            triangle.Add(1);
            triangle.Add(0 + 3);

            triangle.Add(0 + 3);
            triangle.Add(1);
            triangle.Add(1 + 3);

            triangle.Add(1);
            triangle.Add(2);
            triangle.Add(2 + 3);

            triangle.Add(1 + 3);
            triangle.Add(1);
            triangle.Add(2 + 3);


            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangle.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            GeometryTools.UnwrapUV(mesh);
            filter.mesh = mesh;

            collider.transform.position = (from.transform.position + to.transform.position) / 2f;
            collider.transform.LookAt(to.transform.position);
            collider.center = Vector3.zero;
            collider.direction = 2;
            collider.radius = 0.03f;
            collider.height = Vector3.Distance(from.transform.position, to.transform.position);

        }


        public override void MoveBegin()
        {
            from.MoveBegin();
            to.MoveBegin();
            base.MoveBegin();
        }

        public override void MoveEnd()
        {
            from.MoveEnd();
            to.MoveEnd();
            base.MoveEnd();
        }

        public override void Move(Vector3 translation)
        {
            from.Move(translation);
            to.Move(translation);
            base.Move(translation);
        }


        public string shaderColorPropertyName = "_Color";
        private float originalSaturation;
        private float originalValue;
        private float originalAlpha;
        private Color originalColor;

        public override void DisableHighlight()
        {
            filter.GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, originalColor);
        }

        public override void EnableHighlight()
        {
            originalColor = material.GetColor(shaderColorPropertyName);
            originalAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out float H, out originalSaturation, out originalValue);

            Color newColor = Color.HSVToRGB(H, Mathf.Lerp(originalSaturation, 1f, .5f), Mathf.Lerp(originalValue, 0f, .5f));
            newColor.a = originalAlpha;

            filter.GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, newColor);
        }

        public void Display()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

    }
}