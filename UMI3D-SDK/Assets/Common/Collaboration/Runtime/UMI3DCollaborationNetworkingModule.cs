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
                case EnumParameterDto<string> param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value) + UMI3DNetworkingHelper.GetSizeArray(param.possibleValues);
                    break;
                case BooleanParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case FloatParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case IntegerParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case StringParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    break;
                case IntegerRangeParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value)*4;
                    break;
                case FloatRangeParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value)*4;
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
                //case true when typeof(T) == typeof(AbstractParameterDto):
                //    readable = length >= sizeof(uint);
                //    if (readable)
                //    {
                //        var parameter = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                //        switch (parameter)
                //        {
                //            case UMI3DParameterKeys.Enum:
                //                string value;

                //                if (UMI3DNetworkingHelper.TryRead<string>(array, ref position, ref length, out value))
                //                {
                //                    var dto = new EnumParameterDto<string>() { value = value };
                //                    result = (T)Convert.ChangeType(dto, typeof(T));
                //                    readable = true;
                //                }
                //                else { readable = false; result = default(T); return false; }
                //                break;
                //            case UMI3DParameterKeys.Bool:
                //                readable = length >= sizeof(bool);
                //                if (readable)
                //                {
                //                    result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<bool>(array, ref position, ref length), typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            case UMI3DParameterKeys.Float:
                //                readable = length >= sizeof(float);
                //                if (readable)
                //                {
                //                    result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<float>(array, ref position, ref length), typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            case UMI3DParameterKeys.Int:
                //                readable = length >= sizeof(int);
                //                if (readable)
                //                {
                //                    result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<int>(array, ref position, ref length), typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            case UMI3DParameterKeys.String:
                //                string value;
                //                readable = UMI3DNetworkingHelper.TryRead<string>(array, position, length, out value);
                //                result = (T)Convert.ChangeType(value, typeof(T));
                //                break;
                //            case UMI3DParameterKeys.IntRange:
                //                readable = length >= sizeof(int) * 4;
                //                if (readable)
                //                {
                //                    var irp = new IntegerRangeParameterDto();
                //                    irp.value = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                //                    irp.min = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                //                    irp.max = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                //                    irp.increment = UMI3DNetworkingHelper.Read<int>(array, ref position, ref length);
                //                    result = (T)Convert.ChangeType(irp, typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            case UMI3DParameterKeys.FloatRange:
                //                readable = length >= sizeof(float) * 4;
                //                if (readable)
                //                {
                //                    var frp = new FloatRangeParameterDto();
                //                    frp.value = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                //                    frp.min = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                //                    frp.max = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                //                    frp.increment = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length);
                //                    result = (T)Convert.ChangeType(frp, typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            default:
                //                result = default(T);
                //                readable = false;
                //                return false;
                //        }
                //    }
                //    else
                //        result = default(T);
                //    return true;
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
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
                case EnumParameterDto<string> param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    position += UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Enum, array, position);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case BooleanParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    position += UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Bool, array, position);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case FloatParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    position += UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Float, array, position);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case IntegerParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    position += UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Int, array, position);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case StringParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value);
                    position += UMI3DNetworkingHelper.Write(UMI3DParameterKeys.String, array, position);
                    UMI3DNetworkingHelper.Write(param.value, array, position);
                    break;
                case IntegerRangeParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value) * 4;
                    position += UMI3DNetworkingHelper.Write(UMI3DParameterKeys.IntRange, array, position);
                    position += UMI3DNetworkingHelper.Write(param.value, array, position);
                    position += UMI3DNetworkingHelper.Write(param.min, array, position);
                    position += UMI3DNetworkingHelper.Write(param.max, array, position);
                    UMI3DNetworkingHelper.Write(param.increment, array, position);
                    break;
                case FloatRangeParameterDto param:
                    size = sizeof(uint) + UMI3DNetworkingHelper.GetSize(param.value) * 4;
                    position += UMI3DNetworkingHelper.Write(UMI3DParameterKeys.FloatRange, array, position);
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