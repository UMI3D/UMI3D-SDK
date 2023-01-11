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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for UMI3D Mesh
    /// </summary>
    public class UMI3DLineRendererLoader : AbstractRenderedNodeLoader
    {

        public UMI3DLineRendererLoader() { }

        protected LineRenderer line;

        LineRenderer GetOrCreateLine(GameObject node)
        {
            if (node == null)
                return null;
            var line = node.GetComponent<LineRenderer>();
            if (line == null)
            {
                line = node.AddComponent<LineRenderer>();
                Material UMI3DMat = UMI3DEnvironmentLoader.Instance.GetBaseMaterial();

                if (UMI3DMat == null)
                {
                    UMI3DMat = new Material(Shader.Find("Sprites/Default"));
                }
                line.material = UMI3DMat;
            }
            return line;
        }

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DLineDto && base.CanReadUMI3DExtension(data);
        }

        /// <summary>
        /// Load a mesh node.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            var lineDto = data.dto as UMI3DLineDto;
            if (data.node == null)
            {
                throw (new Umi3dException("dto should be an  UMI3DAbstractNodeDto"));
            }

            await base.ReadUMI3DExtension(data);

            line = GetOrCreateLine(data.node);
            line.startColor = lineDto.startColor;
            line.endColor = lineDto.endColor;
            line.loop = lineDto.loop;
            line.useWorldSpace = lineDto.useWorldSpace;
            line.endWidth = lineDto.endWidth;
            line.startWidth = lineDto.startWidth;
            line.positionCount = lineDto.positions.Count();
            line.SetPositions(lineDto.positions.ConvertAll<Vector3>(v => v).ToArray());
            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(lineDto.id);
            if (nodeInstance != null)
                nodeInstance.renderers = new List<Renderer>() { line };
            SetMaterialOverided(lineDto, nodeInstance);
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if (data.entity?.dto is UMI3DLineDto && data.entity is UMI3DNodeInstance node)
            {
                if (await base.SetUMI3DProperty(data)) 
                    return true;
                var extension = (data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DLineDto;
                if (extension == null) return false;

                line = GetOrCreateLine(node.gameObject);
                if (line == null) return false;

                switch (data.property.property)
                {
                    case UMI3DPropertyKeys.LineEndColor:
                        extension.endColor = (Color)data.property.value;
                        line.endColor = extension.endColor;
                        break;
                    case UMI3DPropertyKeys.LineEndWidth:
                        extension.endWidth = (float)data.property.value;
                        line.endWidth = extension.endWidth;
                        break;
                    case UMI3DPropertyKeys.LineLoop:
                        extension.loop = (bool)data.property.value;
                        line.loop = extension.loop;
                        break;
                    case UMI3DPropertyKeys.LinePositions:
                        switch (data.property)
                        {
                            case SetEntityListAddPropertyDto addProperty:
                                extension.positions.Insert(addProperty.index, (SerializableVector3)addProperty.value);
                                line.positionCount++;
                                line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                                break;
                            case SetEntityListRemovePropertyDto removeProperty:
                                extension.positions.RemoveAt(removeProperty.index);
                                line.positionCount--;
                                line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                                break;
                            case SetEntityListPropertyDto innerListProperty:
                                extension.positions[innerListProperty.index] = (Vector3)innerListProperty.value;
                                line.SetPosition(innerListProperty.index, (Vector3)innerListProperty.value);
                                break;
                            case SetEntityPropertyDto setList:
                                extension.positions = (List<SerializableVector3>)setList.value;
                                line.positionCount = extension.positions.Count();
                                line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                                break;
                        }
                        break;
                    case UMI3DPropertyKeys.LineStartColor:
                        extension.startColor = (Color)data.property.value;
                        line.startColor = extension.startColor;
                        break;
                    case UMI3DPropertyKeys.LineStartWidth:
                        extension.startWidth = (float)data.property.value;
                        line.startWidth = extension.startWidth;
                        break;
                    case UMI3DPropertyKeys.LineUseWorldSpace:
                        extension.useWorldSpace = (bool)data.property.value;
                        line.useWorldSpace = extension.useWorldSpace;
                        break;

                    default:
                        return false;
                }
                UpdateModelCollider(node, line);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (await base.SetUMI3DProperty(data)) 
                return true;
            if (data.entity == null) 
                return false;
            var extension = (data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DLineDto;
            if (extension == null) 
                return false;

            var node = data.entity as UMI3DNodeInstance;

            line = GetOrCreateLine(node.gameObject);
            if (line == null) return false;

            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.LineEndColor:
                    extension.endColor = UMI3DSerializer.Read<SerializableColor>(data.container);
                    line.endColor = extension.endColor;
                    break;
                case UMI3DPropertyKeys.LineEndWidth:
                    extension.endWidth = UMI3DSerializer.Read<float>(data.container);
                    line.endWidth = extension.endWidth;
                    break;
                case UMI3DPropertyKeys.LineLoop:
                    extension.loop = UMI3DSerializer.Read<bool>(data.container);
                    line.loop = extension.loop;
                    break;
                case UMI3DPropertyKeys.LinePositions:
                    int index;
                    switch (data.operationId)
                    {
                        case UMI3DOperationKeys.SetEntityListAddProperty:
                            index = UMI3DSerializer.Read<int>(data.container);
                            extension.positions.Insert(index, UMI3DSerializer.Read<SerializableVector3>(data.container));
                            line.positionCount++;
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                        case UMI3DOperationKeys.SetEntityListRemoveProperty:
                            index = UMI3DSerializer.Read<int>(data.container);
                            extension.positions.RemoveAt(index);
                            line.positionCount--;
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                        case UMI3DOperationKeys.SetEntityListProperty:
                            index = UMI3DSerializer.Read<int>(data.container);
                            extension.positions[index] = UMI3DSerializer.Read<SerializableVector3>(data.container);
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                        case UMI3DOperationKeys.SetEntityProperty:
                            extension.positions = UMI3DSerializer.ReadList<SerializableVector3>(data.container);
                            line.positionCount = extension.positions.Count();
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                    }
                    break;
                case UMI3DPropertyKeys.LineStartColor:
                    extension.startColor = UMI3DSerializer.Read<SerializableColor>(data.container);
                    line.startColor = extension.startColor;
                    break;
                case UMI3DPropertyKeys.LineStartWidth:
                    extension.startWidth = UMI3DSerializer.Read<float>(data.container);
                    line.startWidth = extension.startWidth;
                    break;
                case UMI3DPropertyKeys.LineUseWorldSpace:
                    extension.useWorldSpace = UMI3DSerializer.Read<bool>(data.container);
                    line.useWorldSpace = extension.useWorldSpace;
                    break;
                default:
                    return false;
            }
            UpdateModelCollider(node, line);
            return true;
        }

        /// <inheritdoc/>
        protected override void SetModelCollider(ulong id, UMI3DNodeInstance node, ColliderDto dto)
        {
            if (node == null) return;

            line = GetOrCreateLine(node.gameObject);
            if (line == null) return;

            MeshCollider meshCollider = node.gameObject?.AddComponent<MeshCollider>();
            node.colliders.Add(meshCollider);
            UpdateModelCollider(node, line, meshCollider);

            if (line.useWorldSpace)
            {
                UMI3DLogger.LogWarning("Collider is not supported for now with LineRendere.useWorldSpace", DebugScope.CDK);
            }
        }

        private async void UpdateModelCollider(UMI3DNodeInstance nodeInstance, LineRenderer line, MeshCollider meshCollider = null)
        {
            if (nodeInstance == null || line == null || nodeInstance.gameObject == null) return;
            if (meshCollider == null)
                meshCollider = nodeInstance.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null) return;

            Mesh mesh = new Mesh();
            mesh.name = "line-renderer-mesh";

            await UMI3DAsyncManager.Yield();

            line.BakeMesh(mesh, true);

            await UMI3DAsyncManager.Yield();

            if (AtLeast3DistinctVertice(mesh))
                meshCollider.sharedMesh = mesh;
        }

        bool AtLeast3DistinctVertice(Mesh mesh)
        {
            if (mesh != null && mesh.vertices != null && mesh.vertices.Count() < 3)
                return false;

            Vector3? a = null, b = null;

            foreach (var v in mesh.vertices)
            {
                if (a == null)
                    a = v;
                else if (b == null)
                {
                    if (Vector3.Distance(a ?? Vector3.zero, v) > 0.01)

                        b = v;
                }
                else if (Vector3.Distance(a ?? Vector3.zero, v) > 0.01 && Vector3.Distance(b ?? Vector3.zero, v) > 0.01)
                {
                    return true;
                }
            }

            return false;
        }

    }
}