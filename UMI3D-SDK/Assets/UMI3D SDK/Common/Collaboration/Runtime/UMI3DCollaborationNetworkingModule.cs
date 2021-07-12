using System;
using umi3d.common.interaction;
using umi3d.common.userCapture;

namespace umi3d.common.collaboration
{
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {

        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserCameraPropertiesDto):
                    readable = container.length >= 17 * sizeof(float) + sizeof(uint);
                    if (readable)
                    {
                        var usercam = new UserCameraPropertiesDto();
                        usercam.scale = UMI3DNetworkingHelper.Read<float>(container);
                        usercam.projectionMatrix = UMI3DNetworkingHelper.Read<SerializableMatrix4x4>(container);
                        usercam.boneType = UMI3DNetworkingHelper.Read<uint>(container);
                        result = (T)Convert.ChangeType(usercam, typeof(T));
                    }
                    else
                        result = default(T);
                    return true;
                case true when typeof(T) == typeof(BoneDto):
                    uint type;
                    SerializableVector4 rot;
                    if (UMI3DNetworkingHelper.TryRead(container, out type)
                        && UMI3DNetworkingHelper.TryRead(container, out rot))
                    {
                        var bone = new BoneDto() { boneType = type, rotation = rot };
                        result = (T)Convert.ChangeType(bone, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;
                case true when typeof(T) == typeof(BoneBindingDto):
                    string bindingId;
                    string rigName;
                    bool active;
                    uint boneType;
                    ulong objectId;
                    SerializableVector3 offsetPosition;
                    SerializableVector4 offsetRotation;
                    if (UMI3DNetworkingHelper.TryRead(container, out bindingId)
                        && UMI3DNetworkingHelper.TryRead(container, out rigName)
                        && UMI3DNetworkingHelper.TryRead(container, out active)
                        && UMI3DNetworkingHelper.TryRead(container, out boneType)
                        && UMI3DNetworkingHelper.TryRead(container, out objectId)
                        && UMI3DNetworkingHelper.TryRead(container, out offsetPosition)
                        && UMI3DNetworkingHelper.TryRead(container, out offsetRotation))
                    {
                        var bone = new BoneBindingDto()
                        {
                            bindingId = bindingId,
                            rigName = rigName,
                            active = active,
                            boneType = boneType,
                            objectId = objectId,
                            offsetPosition = offsetPosition,
                            offsetRotation = offsetRotation
                        };
                        result = (T)Convert.ChangeType(bone, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;
                //case true when typeof(T) == typeof(AbstractParameterDto):
                //    readable = length >= sizeof(uint);
                //    if (readable)
                //    {
                //        var parameter = UMI3DNetworkingHelper.Read<uint>(container);
                //        switch (parameter)
                //        {
                //            case UMI3DParameterKeys.Enum:
                //                string value;

                //                if (UMI3DNetworkingHelper.TryRead<string>(container, out value))
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
                //                    result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<bool>(container), typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            case UMI3DParameterKeys.Float:
                //                readable = length >= sizeof(float);
                //                if (readable)
                //                {
                //                    result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<float>(container), typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            case UMI3DParameterKeys.Int:
                //                readable = length >= sizeof(int);
                //                if (readable)
                //                {
                //                    result = (T)Convert.ChangeType(UMI3DNetworkingHelper.Read<int>(container), typeof(T));
                //                }
                //                else
                //                    result = default(T);
                //                break;
                //            case UMI3DParameterKeys.String:
                //                string value;
                //                readable = UMI3DNetworkingHelper.TryRead<string>(container, out value);
                //                result = (T)Convert.ChangeType(value, typeof(T));
                //                break;
                //            case UMI3DParameterKeys.IntRange:
                //                readable = length >= sizeof(int) * 4;
                //                if (readable)
                //                {
                //                    var irp = new IntegerRangeParameterDto();
                //                    irp.value = UMI3DNetworkingHelper.Read<int>(container);
                //                    irp.min = UMI3DNetworkingHelper.Read<int>(container);
                //                    irp.max = UMI3DNetworkingHelper.Read<int>(container);
                //                    irp.increment = UMI3DNetworkingHelper.Read<int>(container);
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
                //                    frp.value = UMI3DNetworkingHelper.Read<float>(container);
                //                    frp.min = UMI3DNetworkingHelper.Read<float>(container);
                //                    frp.max = UMI3DNetworkingHelper.Read<float>(container);
                //                    frp.increment = UMI3DNetworkingHelper.Read<float>(container);
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
                    readable = UMI3DNetworkingHelper.TryRead<ulong>(container, out mat.newMaterialId);
                    if (readable)
                    {
                        readable = UMI3DNetworkingHelper.TryRead<bool>(container, out mat.addMaterialIfNotExists);
                        if (readable)
                        {
                            mat.overridedMaterialsId = UMI3DNetworkingHelper.ReadList<string>(container);
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
                    bytable += UMI3DNetworkingHelper.WriteCollection(material.overridedMaterialsId);
                    break;
                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}