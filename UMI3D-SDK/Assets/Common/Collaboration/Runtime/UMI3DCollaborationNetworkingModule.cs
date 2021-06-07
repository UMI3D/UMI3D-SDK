using System;
using umi3d.common.interaction;
using umi3d.common.userCapture;

namespace umi3d.common.collaboration
{
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {

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

                case true when typeof(T) == typeof(UMI3DRenderedNodeDto.MaterialOverrideDto):
                    var mat = new UMI3DRenderedNodeDto.MaterialOverrideDto();
                    readable = UMI3DNetworkingHelper.TryRead<ulong>(array, ref position, ref length, out mat.newMaterialId);
                    if (readable)
                    {
                        readable = UMI3DNetworkingHelper.TryRead<bool>(array, ref position, ref length, out mat.addMaterialIfNotExists);
                        if (readable)
                        {
                            mat.overridedMaterialsId = UMI3DNetworkingHelper.ReadList<string>(array, ref position, ref length);
                            result = (T)Convert.ChangeType(mat, typeof(T));
                        }
                        else
                            result = default(T);
                    }
                    else
                        result = default(T);
                    return true;
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }

        public override bool Write<T>(T value, out Bytable bytable)
        {
            switch (value)
            {
                case UserCameraPropertiesDto camera:
                    bytable = UMI3DNetworkingHelper.Write(camera.scale);
                    bytable += UMI3DNetworkingHelper.Write(camera.projectionMatrix);
                    bytable += UMI3DNetworkingHelper.Write(camera.boneType);
                    break;
                case EnumParameterDto<string> param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Enum);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case BooleanParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Bool);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case FloatParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Float);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case IntegerParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Int);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case StringParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.String);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case IntegerRangeParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.IntRange);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    bytable += UMI3DNetworkingHelper.Write(param.min);
                    bytable += UMI3DNetworkingHelper.Write(param.max);
                    bytable += UMI3DNetworkingHelper.Write(param.increment);
                    break;
                case FloatRangeParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.FloatRange);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    bytable += UMI3DNetworkingHelper.Write(param.min);
                    bytable += UMI3DNetworkingHelper.Write(param.max);
                    bytable += UMI3DNetworkingHelper.Write(param.increment);
                    break;
                case UMI3DRenderedNodeDto.MaterialOverrideDto material:
                    bytable = UMI3DNetworkingHelper.Write(material.newMaterialId);
                    bytable += UMI3DNetworkingHelper.Write(material.addMaterialIfNotExists);
                    bytable += UMI3DNetworkingHelper.WriteArray(material.overridedMaterialsId);
                    break;
                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}