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
using UnityEngine;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Cylinder shaped primitive.
    /// </summary>
    public class Cylinder : AbstractPrimitive
    {
        /// <summary>
        /// Radius of the cylinder.
        /// </summary>
        public float radius { get; private set; }
        /// <summary>
        /// Heigth of the cylinder.
        /// </summary>
        public float height { get; private set; }
        /// <summary>
        /// Position of the cylinder.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// Rotation of the cylinder.
        /// </summary>
        public Quaternion rotation;
        /// <summary>
        /// Scale of the cylinder.
        /// </summary>
        public Vector3 scale;

        /// <summary>
        /// World to local matrix
        /// </summary>
        public Matrix4x4 localToWorld => Matrix4x4.TRS(position, rotation, scale);

        /// <inheritdoc/>
        public override void Delete() { }

        /// <inheritdoc/>
        public override void GetBase(System.Action<Mesh> onsuccess, float angleLimit)
        {
            int subdiv = 128; //meh...

            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var faces = new List<int>();

            vertices.Add(this.position);
            for (int i = 0; i < subdiv; i++)
            {
                vertices.Add(position + Vector3.Scale(scale, rotation * Quaternion.Euler(i * 360f / subdiv * Vector3.up) * Vector3.right * radius));
            }
            for (int i = 1; i < subdiv; i++)
            {
                faces.Add(0);
                faces.Add(i);
                faces.Add(i + 1);
            }
            faces.Add(0);
            faces.Add(subdiv);
            faces.Add(1);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = faces.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.OptimizeReorderVertexBuffer();
            onsuccess.Invoke(mesh);
        }

        /// <inheritdoc/>
        public override Mesh GetMesh()
        {
            return GeometryTools.GetCylinder(position, rotation, scale, radius, height, 16 * ((int)radius + 1));
        }

        /// <inheritdoc/>
        public override bool IsInside(Vector3 point, Space relativeTo)
        {
            /*
             * algorithm :
             * 1- convert point into cylinder local coordinate
             * 2- check if distance from center is < radius
             * 3- check height
             */

            Vector3 localCoordinate =
                relativeTo == Space.Self ?
                    point :
                    Vector3.Scale(
                        new Vector3(
                            1f / scale.x,
                            1f / scale.y,
                            1f / scale.z),
                        Quaternion.Inverse(rootNode?.transform.rotation ?? Quaternion.identity) * (point - (rootNode?.transform.position ?? Vector3.zero)));


            if (Vector3.ProjectOnPlane(localCoordinate, Vector3.up).magnitude > radius)
                return false;
            else if ((localCoordinate.y < 0) || (localCoordinate.y > height))
                return false;
            else
                return true;
        }

        public void SetRadius(float newRadius)
        {
            radius = newRadius;
            onUpdate.Invoke();
        }

        public void SetHeight(float newHeight)
        {
            height = newHeight;
            onUpdate.Invoke();
        }
    }
}