using System;
using umi3d.common.interaction;
using umi3d.common.userCapture;

namespace umi3d.common.collaboration
{
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {
        public override bool GetSize<T>(T value, out int size)
        {
            switch (value)
            {
                case UserCameraPropertiesDto camera:
                    size = sizeof(float) + sizeof(int) + UMI3DNetworkingHelper.GetSize(camera.projectionMatrix);
                    break;
                case BooleanParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case FloatParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case IntegerParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case StringParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case IntegerRangeParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value)*4;
                    break;
                case FloatRangeParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value)*4;
                    break;
                default:
                    size = 0;
                    return false;
            }
            return true;
        }

        public override bool Read<T>(byte[] array, ref int position, ref int length, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserCameraPropertiesDto):
                    readable = length >= 17 * sizeof(float) + sizeof(uint);
                    if (readable)
                    {
                        var usercam = new UserCameraPropertiesDto();
                        usercam.scale = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                        usercam.projectionMatrix = UMI3DNetworkingHelper.Read<SerializableMatrix4x4>(array, ref position, ref length);
                        usercam.boneType = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                        result = (T)Convert.ChangeType(usercam, typeof(T));
                    }
                    else
                        result = default(T);
                    return true;
                case true when typeof(T) == typeof(BooleanParameterDto):
                    readable = length >= sizeof(bool);
                    if (readable)
                    {
                        result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<bool>(array, ref position, ref length), typeof(T));
                    }
                    else
                        result = default(T);
                    break;
                case true when typeof(T) == typeof(FloatParameterDto):
                    readable = length >= sizeof(float);
                    if (readable)
                    {
                        result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<float>(array, ref position, ref length), typeof(T));
                    }
                    else
                        result = default(T);
                    break;
                case true when typeof(T) == typeof(IntegerParameterDto):
                    readable = length >= sizeof(int);
                    if (readable)
                    {
                        result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<int>(array, ref position, ref length), typeof(T));
                    }
                    else
                        result = default(T);
                    break;
                case true when typeof(T) == typeof(StringParameterDto):
                    string value;
                    readable = UMI3DNetworkingHelper.TryRead<string>(array, position, length, out value);
                    result = (T)Convert.ChangeType(value, typeof(T));
                    break;
                case true when typeof(T) == typeof(IntegerRangeParameterDto):
                    readable = length >= sizeof(int)*4;
                    if (readable)
                    {
                        var irp = new IntegerRangeParameterDto();
                        irp.value = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                        irp.min = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                        irp.max = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                        irp.increment = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                        result = (T)Convert.ChangeType(irp, typeof(T));
                    }
                    else
                        result = default(T);
                    break;
                case true when typeof(T) == typeof(FloatRangeParameterDto):
                    readable = length >= sizeof(float) * 4;
                    if (readable)
                    {
                        var frp = new FloatRangeParameterDto();
                        frp.value = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                        frp.min = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                        frp.max = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                        frp.increment = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                        result = (T)Convert.ChangeType(frp, typeof(T));
                    }
                    else
                        result = default(T);
                    break;
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
            return true;
        }

        public override bool Write<T>(T value, byte[] array, int position, out int size)
        {
            switch (value)
            {
                case UserCameraPropertiesDto camera:
                    size = sizeof(float) + sizeof(int) + UMI3DNetworkingHelper.GetSize(camera.projectionMatrix);
                    position += UMI3DNetworkingHelper.Write(camera.scale, array, position);
                    position += UMI3DNetworkingHelper.Write(camera.projectionMatrix, array, position);
                     UMI3DNetworkingHelper.Write(camera.boneType, array, position);
                    break;
                case BooleanParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case FloatParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case IntegerParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case StringParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case IntegerRangeParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value) * 4;
                    position += UMI3DNetworkingHelper.Write(param.value, array, position);
                    position += UMI3DNetworkingHelper.Write(param.min, array, position);
                    position += UMI3DNetworkingHelper.Write(param.max, array, position);
                    UMI3DNetworkingHelper.Write(param.increment, array, position);
                    break;
                case FloatRangeParameterDto param:
                    size = UMI3DNetworkingHelper.GetSize(param.value) * 4;
                    position += UMI3DNetworkingHelper.Write(param.value, array, position);
                    position += UMI3DNetworkingHelper.Write(param.min, array, position);
                    position += UMI3DNetworkingHelper.Write(param.max, array, position);
                    UMI3DNetworkingHelper.Write(param.increment, array, position);
                    break;
                default:
                    size = 0;
                    return false;
            }
            return true;
        }
    }
}