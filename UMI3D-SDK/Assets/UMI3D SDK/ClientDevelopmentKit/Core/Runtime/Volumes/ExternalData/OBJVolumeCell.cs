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
    /// Volume cell described by a mesh.
    /// </summary>
    public class OBJVolumeCell : AbstractVolumeCell
    {
        public List<Mesh> meshes;
        public List<GameObject> parts;

        public ulong id;

        /// <inheritdoc/>
        public override void GetBase(System.Action<Mesh> onsuccess, float angleLimit)
        {
            List<Mesh> bases = meshes.ConvertAll(m => GeometryTools.GetBase(m, angleLimit, 0.01f));
            onsuccess.Invoke(GeometryTools.ForceNormalUp(GeometryTools.Merge(bases)));
        }

        /// <inheritdoc/>
        public override Mesh GetMesh()
        {
            return GeometryTools.Merge(meshes);
        }

        /// <inheritdoc/>
        public override ulong Id() => id;

        /// <inheritdoc/>
        public override bool IsInside(Vector3 point, Space relativeTo) => meshes.Exists(mesh => GeometryTools.IsInside(mesh, point));
    }
}