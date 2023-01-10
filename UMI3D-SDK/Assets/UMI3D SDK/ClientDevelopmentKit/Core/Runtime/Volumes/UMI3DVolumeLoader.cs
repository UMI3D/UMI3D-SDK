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
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Loader for <see cref="AbstractVolumeCellDto"/>.
    /// </summary>
	public class UMI3DVolumeLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is AbstractVolumeDescriptorDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as AbstractVolumeDescriptorDto;
            switch (dto)
            {
                case AbstractPrimitiveDto prim:
                    var p = await VolumePrimitiveManager.CreatePrimitive(prim);
                    UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, p, () => VolumePrimitiveManager.DeletePrimitive(dto.id));

                    break;
                case OBJVolumeDto obj:
                    var objVolume = await ExternalVolumeDataManager.Instance.CreateOBJVolume(obj);
                    UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, objVolume, () => ExternalVolumeDataManager.Instance.DeleteOBJVolume(dto.id));

                    break;
                default:
                    throw (new Umi3dException("Unknown Dto Type"));
            }
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.VolumePrimitive_Box_Center:
                    var box_1 = value.entity.Object as Box;
                    var newCenter = (Vector3)value.property.value;
                    Vector3 size = box_1.bounds.size;
                    box_1.SetBounds(new Bounds(newCenter, size));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Box_Size:
                    var box_2 = value.entity.Object as Box;
                    Vector3 center = box_2.bounds.center;
                    var newsize = (Vector3)value.property.value;
                    box_2.SetBounds(new Bounds(center, newsize));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Height:
                    var cyl_1 = value.entity.Object as Cylinder;
                    cyl_1.SetHeight((float)value.property.value);
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Radius:
                    var cyl_2 = value.entity.Object as Cylinder;
                    cyl_2.SetRadius((float)value.property.value);
                    return true;

                default:
                    return false;
            }
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.VolumePrimitive_Box_Center:
                    var box_1 = value.entity.Object as Box;
                    Vector3 newCenter = UMI3DSerializer.Read<Vector3>(value.container);
                    Vector3 size = box_1.bounds.size;
                    box_1.SetBounds(new Bounds(newCenter, size));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Box_Size:
                    var box_2 = value.entity.Object as Box;
                    Vector3 center = box_2.bounds.center;
                    Vector3 newsize = UMI3DSerializer.Read<Vector3>(value.container);
                    box_2.SetBounds(new Bounds(center, newsize));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Height:
                    var cyl_1 = value.entity.Object as Cylinder;
                    cyl_1.SetHeight(UMI3DSerializer.Read<float>(value.container));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Radius:
                    var cyl_2 = value.entity.Object as Cylinder;
                    cyl_2.SetRadius(UMI3DSerializer.Read<float>(value.container));
                    return true;

                default:
                    return false;
            }
        }
    }
}