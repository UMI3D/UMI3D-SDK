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

using umi3d.common.volume;
using umi3d.common;
using System;
using System.Collections.Generic;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Loader for volume parts.
    /// </summary>
	static public class UMI3DVolumeLoader 
	{
        static public void ReadUMI3DExtension(AbstractVolumeDescriptorDto dto, Action finished, Action<Umi3dExecption> failed)
        {
            switch (dto)
            {
                case PointDto pointDto:
                    VolumeSliceGroupManager.Instance.CreatePoint(pointDto, p =>
                    {
                        UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, p, () => VolumeSliceGroupManager.Instance.DeletePoint(dto.id));
                        finished.Invoke();
                    });                    
                    break;

                case FaceDto faceDto:
                    VolumeSliceGroupManager.Instance.CreateFace(faceDto, f =>
                    {
                        UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, f, () => VolumeSliceGroupManager.Instance.DeleteFace(dto.id));
                        finished.Invoke();
                    });                   
                    break;

                case VolumeSliceDto slice:
                    VolumeSliceGroupManager.Instance.CreateVolumeSlice(slice, s =>
                    {
                        UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, s, () => VolumeSliceGroupManager.Instance.DeleteVolumeSlice(dto.id));
                        finished.Invoke();
                    });
                    break;

                case VolumeSlicesGroupDto group:
                    VolumeSliceGroupManager.Instance.CreateVolumeSliceGroup(group, v =>
                    {
                        UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, v, () => VolumeSliceGroupManager.Instance.DeleteVolumeSliceGroup(dto.id));
                        finished.Invoke();
                    });                    
                    break;

                case AbstractPrimitiveDto prim:
                    VolumePrimitiveManager.Instance.CreatePrimitive(prim, p =>
                    {
                        UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, p, () => VolumePrimitiveManager.Instance.DeletePrimitive(dto.id));
                        finished.Invoke();
                    });
                    break;

                default:
                    failed(new Umi3dExecption("Unknown dto type"));
                    break;
            }
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            switch (property.property)
            {
                

                //TODO : Take the desktop version.

                default:
                    return false;
            }
        }
    }
}