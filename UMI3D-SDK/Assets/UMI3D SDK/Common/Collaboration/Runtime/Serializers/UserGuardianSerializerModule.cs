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
using umi3d.common.lbe.description;

namespace umi3d.common.lbe
{
    public class UserGuardianSerializerModule : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(UserGuardianRequestDto) => true,
                true when typeof(T) == typeof(LBEUserRegisterRequestDto) => true,
                true when typeof(T) == typeof(LBELeaderRegisterRequestDto) => true,
                _ => null
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when (typeof(T) == typeof(UserGuardianRequestDto)):
                    {
                        List<ARAnchorDto> ARAnchors = UMI3DSerializer.ReadList<ARAnchorDto>(container);

                        if (ARAnchors != null)
                        {
                            UserGuardianRequestDto userguardian = new UserGuardianRequestDto
                            {
                                aRAnchors = ARAnchors,
                            };

                            readable = true;
                            result = (T)Convert.ChangeType(userguardian, typeof(T));
                            return true;
                        }

                        result = default(T);
                        readable = false;
                        return false;
                    }
                case true when (typeof(T) == typeof(LBEUserRegisterRequestDto)):
                    {
                        readable = UMI3DSerializer.TryRead<ulong>(container, out ulong groupId);

                        if (readable)
                        {
                            LBEUserRegisterRequestDto lBEUserRegister = new LBEUserRegisterRequestDto
                            {
                                groupId = groupId,
                            };

                            readable = true;
                            result = (T)Convert.ChangeType(lBEUserRegister, typeof(T));
                            return true;
                        }

                        result = default(T);
                        readable = false;
                        return false;
                    }
                case true when (typeof(T) == typeof(LBELeaderRegisterRequestDto)):
                    {
                        readable = UMI3DSerializer.TryRead<ulong>(container, out ulong groupId);

                        if (readable)
                        {
                            LBELeaderRegisterRequestDto lBELeaderRegister = new LBELeaderRegisterRequestDto
                            {
                                groupId = groupId,
                            };

                            readable = true;
                            result = (T)Convert.ChangeType(lBELeaderRegister, typeof(T));
                            return true;
                        }

                        result = default(T);
                        readable = false;
                        return false;
                    }
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case UserGuardianRequestDto guardianRequestdto:

                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.MDMGuardianBrowserRequest)
                        + UMI3DSerializer.WriteCollection(guardianRequestdto.aRAnchors);
                    return true;

                case LBEUserRegisterRequestDto userRegisterRequestdto:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.LBEUserRegisterRequest)
                        + UMI3DSerializer.Write(userRegisterRequestdto.groupId);
                    return true;

                case LBELeaderRegisterRequestDto leaderRegisterRequestdto:

                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.LBELeaderRegisterRequest)
                        + UMI3DSerializer.Write(leaderRegisterRequestdto.groupId);
                    return true;

                default:
                    if (typeof(T) == typeof(ResourceDto))
                    {
                        // value is null
                        bytable = UMI3DSerializer.WriteCollection(new System.Collections.Generic.List<FileDto>());
                        return true;
                    }
                    bytable = null;
                    return false;
            }
        }
    }
}