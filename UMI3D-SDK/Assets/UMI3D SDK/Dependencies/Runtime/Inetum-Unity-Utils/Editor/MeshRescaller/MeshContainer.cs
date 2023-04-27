/*
Copyright 2019 - 2023 Inetum

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

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace inetum.unityUtils.editor
{
    public class MeshContainer : ScriptableObject
    {
        /// <summary>
        /// The mesh you want to rescale
        /// </summary>
        [SerializeField] private Mesh mesh;
        /// <summary>
        /// The rescalling factor to go from the mesh to the mesh you want 
        /// </summary>
        [SerializeField] private float scaling = 1;
        /// <summary>
        /// The path where the new mesh will be created
        /// </summary>
        [SerializeField, TextArea] public string savePath = "Assets/";
        /// <summary>
        /// The new mesh which is generated 
        /// </summary>
        [HideInInspector] private Mesh newMesh;

        /// <summary>
        /// A method to calculate a new mesh by rescalling its vertecies
        /// </summary>
        /// <param name="mesh_action"> an action which is invoked when the new mesh is created </param>
        public void CalculateNewMesh(Action<Mesh, Mesh, float> mesh_action)
        {
            Vector3[] intial_vertices = mesh.vertices;
            Vector3[] new_vertices = new Vector3[intial_vertices.Length];

            newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.triangles = mesh.triangles;
            newMesh.uv = mesh.uv;
            newMesh.normals = mesh.normals;
            newMesh.colors = mesh.colors;
            newMesh.tangents = mesh.tangents;

            for (var i = 0; i < new_vertices.Length; i++)
            {
                Vector3 vertex = intial_vertices[i];
                vertex.x *= scaling;
                vertex.y *= scaling;
                vertex.z *= scaling;

                new_vertices[i] = vertex;
            }

            newMesh.vertices = new_vertices;
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            mesh_action?.Invoke(mesh, newMesh, scaling);
        }


        /// <summary>
        /// Create the new mesh which has been created by the CalculateNewMesh Focntion
        /// </summary>
        public void CreateNewMeshAsset()
        {
            newMesh.name = "_" + scaling + "_" + newMesh.name;
            string path = savePath + newMesh.name + ".asset";
            AssetDatabase.CreateAsset(newMesh, path);
        }
    }

}
#endif
