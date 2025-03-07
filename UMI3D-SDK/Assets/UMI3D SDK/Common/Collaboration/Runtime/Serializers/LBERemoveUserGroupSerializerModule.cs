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

namespace umi3d.common.lbe
{
    public class LBERemoveUserGroupSerializerModule : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return typeof(T) == typeof(LBERemoveUserGroupOperationDto) ? true : null;
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            if (typeof(T) == typeof(LBERemoveUserGroupOperationDto))
            {
                ulong userId = UMI3DSerializer.Read<ulong>(container);
                readable = UMI3DSerializer.TryRead(container, out uint Key);

                if (readable)
                {
                    LBERemoveUserGroupOperationDto dellUserLBEGroup = new LBERemoveUserGroupOperationDto()
                    {
                        userId = userId,
                    };
                    readable = true;
                    result = (T)Convert.ChangeType(dellUserLBEGroup, typeof(T));
                    return true;
                }
            }
            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (value is LBERemoveUserGroupOperationDto dto)
            {
                bytable = UMI3DSerializer.Write(UMI3DOperationKeys.MDMRemoveUserOperation)
                    + UMI3DSerializer.Write(dto.userId);
                return true;
            }

            bytable = null;
            return false;
        }
    }
}