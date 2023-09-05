/*
Copyright 2019 - 2021 Inetum

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
using umi3d.common.userCapture.description;

namespace umi3d.common.userCapture.tracking
{
    public class UserTrackingFrameSerializer : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return typeof(T) == typeof(UserTrackingFrameDto) ? true : null;
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            if (typeof(T) == typeof(UserTrackingFrameDto))
            {
                if (
                    UMI3DSerializer.TryRead(container, out ulong userId)
                    && UMI3DSerializer.TryRead(container, out ulong parentId)

                    && UMI3DSerializer.TryRead(container, out Vector3Dto position)
                    && UMI3DSerializer.TryRead(container, out Vector4Dto rotation)
                    && UMI3DSerializer.TryRead(container, out Vector3Dto speed)
                    && UMI3DSerializer.TryRead(container, out bool jumping)
                    && UMI3DSerializer.TryRead(container, out bool crouching)
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

                            speed = speed,
                            jumping = jumping,
                            crouching = crouching,

                            trackedBones = trackedBones,

                            environmentPosesIndexes = playerServerPoses,

                            customPosesIndexes = playerUserPoses
                        };
                        readable = true;
                        result = (T)Convert.ChangeType(trackingFrame, typeof(T));

                        return true;
                    }
                }
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (value is UserTrackingFrameDto c)
            {
                bytable = UMI3DSerializer.Write(c.userId)
                    + UMI3DSerializer.Write(c.parentId)
                    + UMI3DSerializer.Write(c.position)
                    + UMI3DSerializer.Write(c.rotation)
                    + UMI3DSerializer.Write(c.speed)
                    + UMI3DSerializer.Write(c.jumping)
                    + UMI3DSerializer.Write(c.crouching)
                    + UMI3DSerializer.WriteCollection(c.trackedBones)
                    + UMI3DSerializer.WriteCollection(c.environmentPosesIndexes)
                    + UMI3DSerializer.WriteCollection(c.customPosesIndexes);
                return true;
            }

            bytable = null;
            return false;
        }
    }
}