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

namespace umi3d.common.userCapture.tracking
{
    public class UserCameraPropertiesSerializer : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return typeof(T) == typeof(UserCameraPropertiesDto) ? true : null;
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            if (typeof(T) == typeof(UserCameraPropertiesDto))
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
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (value is UserCameraPropertiesDto c)
            {
                bytable = UMI3DSerializer.Write(c.scale)
                        + UMI3DSerializer.Write(c.projectionMatrix)
                        + UMI3DSerializer.Write(c.boneType);
                return true;
            }

            bytable = null;
            return false;
        }
    }
}