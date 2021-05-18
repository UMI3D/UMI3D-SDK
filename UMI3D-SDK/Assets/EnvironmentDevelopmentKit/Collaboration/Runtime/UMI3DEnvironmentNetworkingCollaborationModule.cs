using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DEnvironmentNetworkingCollaborationModule : Umi3dNetworkingHelperModule
    {
        public override bool GetSize<T>(T value, out int result)
        {
            switch (value)
            {
                case UMI3DCollaborationUser user:
                    result =  2*sizeof(uint) + 4 * sizeof(ulong);
                    return true;
            }
            result = 0;
            return false;
        }

        public override bool Read<T>(byte[] array, ref int position, ref int length, out bool readable, out T result)
        {
            //switch (true)
            //{
            //    case true when typeof(T) == typeof(UserDto):
            //        //result = (T)Convert.ChangeType(BitConverter.ToBoolean(array, (int)position), typeof(T));
            //        //position += sizeof(bool);
            //        //length -= sizeof(bool);
            //        return true;
            //}
            //switch (true)
            //{
            //    case true when typeof(T) == typeof(UMI3DCollaborationUser):
            //        result = sizeof(uint) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong) + sizeof(uint);
            //        UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
            //        UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
            //        UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
            //        UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
            //        UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);

            //        return true;
            //}
            result = default;
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, byte[] array, int position, out int result)
        {
            switch (value)
            {
                case UMI3DCollaborationUser user:
                    Debug.Log($"{position} {array.Length} {array.ToString<byte>()}");
                    position += UMI3DNetworkingHelper.Write<ulong>(user.Id(), array, position);
                    position += UMI3DNetworkingHelper.Write<uint>((uint)user.status, array, position);
                    position += UMI3DNetworkingHelper.Write<ulong>(user.Avatar == null ? 0 : user.Avatar.Id(), array, position);
                    position += UMI3DNetworkingHelper.Write<ulong>(user.audioPlayer?.Id() ?? 0, array, position);
                    position += UMI3DNetworkingHelper.Write<ulong>(user.videoPlayer?.Id() ?? 0, array, position);
                    position += UMI3DNetworkingHelper.Write<uint>(user.networkPlayer?.NetworkId ?? 0, array, position);
                    result = 2 * sizeof(uint) + 4 * sizeof(ulong);
                    Debug.Log($"{position} {array.Length} {array.ToString<byte>()}");
                    return true;
            }
            result = 0;
            return false;
        }
    }
}