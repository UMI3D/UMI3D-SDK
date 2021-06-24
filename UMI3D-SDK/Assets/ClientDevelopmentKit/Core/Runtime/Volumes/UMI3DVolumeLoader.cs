using umi3d.common.volume;
using umi3d.common;
using System;
using System.Collections.Generic;

namespace umi3d.cdk.volumes
{
	static public class UMI3DVolumeLoader 
	{
        static public void ReadUMI3DExtension(AbstractVolumeDescriptor dto, Action finished, Action<string> failed)
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
                    failed("Unknown dto type");
                    break;
            }
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            switch (property.property)
            {
                case UMI3DPropertyKeys.PointPosition:
                    Point point = entity.Object as Point;
                    if (point == null)
                        throw new Exception("Internal error : entity is not a point");

                    point.SetPosition(property.value as SerializableVector3);
                    return true;

                case UMI3DPropertyKeys.FacePointsIds:
                    Face face = entity.Object as Face;
                    if (face == null)
                        throw new Exception("Internal error : entity is not a face");

                    face.SetPoints(property.value as List<string>);
                    return true;

                case UMI3DPropertyKeys.VolumeSlices:
                    VolumeSliceGroup group = entity.Object as VolumeSliceGroup;
                    if (group == null)
                        throw new Exception("Internal error : entity is not a volume slice group");

                    group.SetSlices(property.value as List<VolumeSliceDto>);
                    return true;

                case UMI3DPropertyKeys.VolumeSlicePoints:
                    VolumeSlice slice = entity.Object as VolumeSlice;
                    if (slice == null)
                        throw new Exception("Internal error : entity is not a volume slice");

                    slice.SetPoints(property.value as List<PointDto>);
                    return true;

                case UMI3DPropertyKeys.VolumeSliceEdges:
                    VolumeSlice slice2 = entity.Object as VolumeSlice;
                    if (slice2 == null)
                        throw new Exception("Internal error : entity is not a volume slice");

                    slice2.SetEdges(property.value as List<int>);
                    return true;

                case UMI3DPropertyKeys.VolumeSliceFaces:
                    VolumeSlice slice3 = entity.Object as VolumeSlice;
                    if (slice3 == null)
                        throw new Exception("Internal error : entity is not a volume slice");

                    slice3.SetFaces(property.value as List<FaceDto>);
                    return true;

                default:
                    return false;
            }
        }
    }
}