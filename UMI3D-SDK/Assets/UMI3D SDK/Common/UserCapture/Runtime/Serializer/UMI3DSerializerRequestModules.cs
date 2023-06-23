/*
Copyright 2019 - 2023 Inetum

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

using System;
using System.Collections.Generic;

namespace umi3d.common.userCapture
{
    public class UMI3DSerializerRequestModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(UserCameraPropertiesDto) => true,
                true when typeof(T) == typeof(UserTrackingBoneDto) => true,
                true when typeof(T) == typeof(UserTrackingFrameDto) => true,
                _ => null,
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserCameraPropertiesDto):
                    {
                        readable = container.length >= (17 * sizeof(float)) + sizeof(uint);
                        if (readable)
                        {
                            var usercam = new UserCameraPropertiesDto
                            {
                                scale = UMI3DSerializer.Read<float>(container),
                                projectionMatrix = UMI3DSerializer.Read<Matrix4x4Dto>(container),
                                boneType = UMI3DSerializer.Read<uint>(container)
                            };
                            result = (T)Convert.ChangeType(usercam, typeof(T));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }
                case true when typeof(T) == typeof(UserTrackingBoneDto):
                    {
                        if (UMI3DSerializer.TryRead(container, out uint idKey)
                            && UMI3DSerializer.TryRead(container, out ulong userId)
                            && UMI3DSerializer.TryRead(container, out ControllerDto boneDto))
                        {
                            var trackingBone = new UserTrackingBoneDto
                            {
                                userId = userId,
                                bone = boneDto
                            };
                            readable = true;
                            result = (T)Convert.ChangeType(trackingBone, typeof(T));

                            return true;
                        }

                        readable = false;
                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(UserTrackingFrameDto):
                    {
                        if (
                            UMI3DSerializer.TryRead(container, out uint idKey)
                            && UMI3DSerializer.TryRead(container, out ulong userId)
                            && UMI3DSerializer.TryRead(container, out ulong parentId)

                            && UMI3DSerializer.TryRead(container, out Vector3Dto position)
                            && UMI3DSerializer.TryRead(container, out Vector4Dto rotation)
                            )
                        {
                            List<ControllerDto> trackedBones = UMI3DSerializer.ReadList<ControllerDto>(container);
                            List<int> playerServerPoses = UMI3DSerializer.ReadList<int>(container);
                            List<int> playerUserPoses = UMI3DSerializer.ReadList<int>(container);

                            if (trackedBones != default)
                            {
                                var trackingFrame = new UserTrackingFrameDto
                                {
                                    userId = userId,
                                    parentId = parentId,

                                    position = position,
                                    rotation = rotation,

                                    trackedBones = trackedBones,

                                    playerServerPoses = playerServerPoses,

                                    playerUserPoses = playerUserPoses
                                };
                                readable = true;
                                result = (T)Convert.ChangeType(trackingFrame, typeof(T));

                                return true;
                            }
                        }

                        result = default(T);
                        readable = false;
                        return false;
                    }
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
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