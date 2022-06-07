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

        /// <summary>
        /// Load a mesh node.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {
            var lineDto = dto as UMI3DLineDto;
            if (node == null)
            {
                failed.Invoke(new Umi3dException(0, "dto should be an  UMI3DAbstractNodeDto"));
                return;
            }

            base.ReadUMI3DExtension(dto, node, () =>
            {
                line = node.GetComponent<LineRenderer>();
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

                finished?.Invoke();
            }, failed);
        }

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity?.dto is UMI3DLineDto)
            {
                if (base.SetUMI3DProperty(entity, property)) return true;
                var extension = (entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DLineDto;
                if (extension == null) return false;
                switch (property.property)
                {
                    case UMI3DPropertyKeys.LineEndColor:
                        extension.endColor = (Color)property.value;
                        line.endColor = extension.endColor;
                        break;
                    case UMI3DPropertyKeys.LineEndWidth:
                        extension.endWidth = (float)property.value;
                        line.endWidth = extension.endWidth;
                        break;
                    case UMI3DPropertyKeys.LineLoop:
                        extension.loop = (bool)property.value;
                        line.loop = extension.loop;
                        break;
                    case UMI3DPropertyKeys.LinePositions:
                        switch (property)
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
                        extension.startColor = (Color)property.value;
                        line.startColor = extension.startColor;
                        break;
                    case UMI3DPropertyKeys.LineStartWidth:
                        extension.startWidth = (float)property.value;
                        line.startWidth = extension.startWidth;
                        break;
                    case UMI3DPropertyKeys.LineUseWorldSpace:
                        extension.useWorldSpace = (bool)property.value;
                        line.useWorldSpace = extension.useWorldSpace;
                        break;

                    default:
                        return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (base.SetUMI3DProperty(entity, operationId, propertyKey, container)) return true;
            if (entity == null) return false;
            var extension = (entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DLineDto;
            if (extension == null) return false;

            var node = entity as UMI3DNodeInstance;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.LineEndColor:
                    extension.endColor = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    line.endColor = extension.endColor;
                    break;
                case UMI3DPropertyKeys.LineEndWidth:
                    extension.endWidth = UMI3DNetworkingHelper.Read<float>(container);
                    line.endWidth = extension.endWidth;
                    break;
                case UMI3DPropertyKeys.LineLoop:
                    extension.loop = UMI3DNetworkingHelper.Read<bool>(container);
                    line.loop = extension.loop;
                    break;
                case UMI3DPropertyKeys.LinePositions:
                    int index;
                    switch (operationId)
                    {
                        case UMI3DOperationKeys.SetEntityListAddProperty:
                            index = UMI3DNetworkingHelper.Read<int>(container);
                            extension.positions.Insert(index, UMI3DNetworkingHelper.Read<SerializableVector3>(container));
                            line.positionCount++;
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                        case UMI3DOperationKeys.SetEntityListRemoveProperty:
                            index = UMI3DNetworkingHelper.Read<int>(container);
                            extension.positions.RemoveAt(index);
                            line.positionCount--;
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                        case UMI3DOperationKeys.SetEntityListProperty:
                            index = UMI3DNetworkingHelper.Read<int>(container);
                            extension.positions[index] = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                        case UMI3DOperationKeys.SetEntityProperty:
                            extension.positions = UMI3DNetworkingHelper.ReadList<SerializableVector3>(container);
                            line.positionCount = extension.positions.Count();
                            line.SetPositions(extension.positions.ConvertAll<Vector3>(v => v).ToArray());
                            break;
                    }
                    break;
                case UMI3DPropertyKeys.LineStartColor:
                    extension.startColor = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    line.startColor = extension.startColor;
                    break;
                case UMI3DPropertyKeys.LineStartWidth:
                    extension.startWidth = UMI3DNetworkingHelper.Read<float>(container);
                    line.startWidth = extension.startWidth;
                    break;
                case UMI3DPropertyKeys.LineUseWorldSpace:
                    extension.useWorldSpace = UMI3DNetworkingHelper.Read<bool>(container);
                    line.useWorldSpace = extension.useWorldSpace;
                    break;
                default:
                    return false;
            }
            return true;
        }

    }
}