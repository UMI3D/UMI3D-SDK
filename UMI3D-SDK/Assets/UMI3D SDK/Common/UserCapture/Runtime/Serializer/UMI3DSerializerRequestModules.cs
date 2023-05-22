using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.common.userCapture
{
    public class UMI3DSerializerRequestModules : UMI3DSerializerModule
    {
        public override bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(UserCameraPropertiesDto) => true,
                true when typeof(T) == typeof(UserTrackingBoneDto) => true,
                true when typeof(T) == typeof(UserTrackingFrameDto) => true,
                _ => null,
            };
        }

        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(UserCameraPropertiesDto):
                    throw new NotImplementedException("need to implement UserCameraPropertiesDto case");
                case true when typeof(T) == typeof(UserTrackingBoneDto):
                    throw new NotImplementedException("need to implement UserTrackingBoneDto case");
                case true when typeof(T) == typeof(UserTrackingFrameDto):
                    throw new NotImplementedException("need to implement UserTrackingFrameDto case");
            }

            result = default(T);
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case UserCameraPropertiesDto c:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.UserCameraProperties)
                        + UMI3DSerializer.Write(c.scale)
                        + UMI3DSerializer.Write(c.projectionMatrix)
                        + UMI3DSerializer.Write(c.boneType);
                    return true;
                case UserTrackingBoneDto c:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.UserTrackingBone)
                        + UMI3DSerializer.Write(c.userId)
                        + UMI3DSerializer.Write(c.bone);
                    return true;
                case UserTrackingFrameDto c:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.UserTrackingFrame)
                        + UMI3DSerializer.Write(c.userId)
                        + UMI3DSerializer.Write(c.parentId)
                        + UMI3DSerializer.Write(c.position)
                        + UMI3DSerializer.Write(c.rotation)
                        + UMI3DSerializer.WriteCollection(c.trackedBones)
                        + UMI3DSerializer.WriteCollection(c.playerServerPoses)
                        + UMI3DSerializer.WriteCollection(c.playerUserPoses);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
