using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Rendering;


namespace umi3d.common
{
    public class UMI3DSerializerRequestModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(FrameConfirmationDto) => true,
                true when typeof(T) == typeof(ConferenceBrowserRequestDto) => true,
                true when typeof(T) == typeof(VolumeUserTransitDto) => true,
                _ => null,
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(FrameConfirmationDto):
                    throw new NotImplementedException("need to implement FrameConfirmationDto case");
                case true when typeof(T) == typeof(ConferenceBrowserRequestDto):
                    throw new NotImplementedException("need to implement ConferenceBrowserRequestDto case");
                case true when typeof(T) == typeof(VolumeUserTransitDto):
                    throw new NotImplementedException("need to implement VolumeUserTransitDto case");
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case FrameConfirmationDto c:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.FrameConfirmation)
                        + UMI3DSerializer.Write(c.userId);
                    return true;
                case ConferenceBrowserRequestDto c:
                    bytable = UMI3DSerializer.Write(c.operation);
                    if (c.operation != UMI3DOperationKeys.MuteAllAttentionStatus && c.operation != UMI3DOperationKeys.MuteAllAvatarStatus && c.operation != UMI3DOperationKeys.MuteAllMicrophoneStatus)
                        bytable +=
                            UMI3DSerializer.Write(c.id)
                            + UMI3DSerializer.Write(c.value);
                    return true;
                case VolumeUserTransitDto v:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.VolumeUserTransit)
                        + UMI3DSerializer.Write(v.volumeId)
                        + UMI3DSerializer.Write(v.direction);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}