using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {
        public override bool GetSize<T>(T value, out int size)
        {
            throw new System.NotImplementedException();
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
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }

        public override bool Write<T>(T value, byte[] array, int position, out int size)
        {
            throw new System.NotImplementedException();
        }
    }
}