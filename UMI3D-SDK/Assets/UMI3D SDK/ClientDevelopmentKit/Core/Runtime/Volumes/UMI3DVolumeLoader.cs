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
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Loader for <see cref="AbstractVolumeCellDto"/>.
    /// </summary>
	public static class UMI3DVolumeLoader
    {
        public static async Task ReadUMI3DExtension(AbstractVolumeDescriptorDto dto)
        {
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

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            switch (property.property)
            {
                case UMI3DPropertyKeys.VolumePrimitive_Box_Center:
                    var box_1 = entity.Object as Box;
                    var newCenter = (Vector3)property.value;
                    Vector3 size = box_1.bounds.size;
                    box_1.SetBounds(new Bounds(newCenter, size));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Box_Size:
                    var box_2 = entity.Object as Box;
                    Vector3 center = box_2.bounds.center;
                    var newsize = (Vector3)property.value;
                    box_2.SetBounds(new Bounds(center, newsize));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Height:
                    var cyl_1 = entity.Object as Cylinder;
                    cyl_1.SetHeight((float)property.value);
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Radius:
                    var cyl_2 = entity.Object as Cylinder;
                    cyl_2.SetRadius((float)property.value);
                    return true;

                default:
                    return false;
            }
        }

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.VolumePrimitive_Box_Center:
                    var box_1 = entity.Object as Box;
                    Vector3 newCenter = UMI3DNetworkingHelper.Read<Vector3>(container);
                    Vector3 size = box_1.bounds.size;
                    box_1.SetBounds(new Bounds(newCenter, size));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Box_Size:
                    var box_2 = entity.Object as Box;
                    Vector3 center = box_2.bounds.center;
                    Vector3 newsize = UMI3DNetworkingHelper.Read<Vector3>(container);
                    box_2.SetBounds(new Bounds(center, newsize));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Height:
                    var cyl_1 = entity.Object as Cylinder;
                    cyl_1.SetHeight(UMI3DNetworkingHelper.Read<float>(container));
                    return true;

                case UMI3DPropertyKeys.VolumePrimitive_Cylinder_Radius:
                    var cyl_2 = entity.Object as Cylinder;
                    cyl_2.SetRadius(UMI3DNetworkingHelper.Read<float>(container));
                    return true;

                default:
                    return false;
            }
        }
    }
}