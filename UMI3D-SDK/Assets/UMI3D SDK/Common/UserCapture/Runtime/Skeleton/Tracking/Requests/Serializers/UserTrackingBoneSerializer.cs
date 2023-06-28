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
using umi3d.common.userCapture.description;

namespace umi3d.common.userCapture.tracking
{
    public class UserTrackingBoneSerializer : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return typeof(T) == typeof(UserTrackingBoneDto) ? true : null;
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            if (typeof(T) == typeof(UserTrackingBoneDto))
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
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (value is UserTrackingBoneDto c)
            {
                bytable = UMI3DSerializer.Write(UMI3DOperationKeys.UserTrackingBone)
                    + UMI3DSerializer.Write(c.userId)
                    + UMI3DSerializer.Write(c.bone);
                return true;
            }

            bytable = null;
            return false;
        }
    }
}