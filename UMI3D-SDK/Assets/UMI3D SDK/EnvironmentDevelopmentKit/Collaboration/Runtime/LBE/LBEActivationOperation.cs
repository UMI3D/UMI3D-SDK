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

using System.Collections.Generic;
using umi3d.common;
using umi3d.common.lbe;
using umi3d.common.lbe.description;

namespace umi3d.edk.collaboration.lbe
{
    public abstract class AbstractLBEOperation : Operation { }

    public class LBEActivationOperation : AbstractLBEOperation
    {
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.MDMActivationOperation);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LBEActivationDto();
        }
    }

    public class LBESetLeaderOperation : AbstractLBEOperation
    {
        public bool isLeader;

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.MDMLeaderOperation)
                + UMI3DSerializer.Write<bool>(isLeader);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LBELeaderDto()
            {
                isLeader = isLeader
            };
        }
    }

    public class LBESetUserGroupOperation : AbstractLBEOperation
    {
        public ulong groupId;

        public List<ulong> colocatedUserIds = new List<ulong>();

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.MDMSetGroupOperation)
                + UMI3DSerializer.Write<ulong>(groupId)
                + UMI3DSerializer.Write<List<ulong>>(colocatedUserIds);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LBESetUserGroupDto()
            {
                groupId = groupId,
                colocatedUserIds = colocatedUserIds
            };
        }
    }

    public class LBESetGuardianOperation : AbstractLBEOperation
    {
        public List<ARAnchorDto> anchors;

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.MDMSetGuardianOperation)
                + UMI3DSerializer.Write(anchors);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LBESetGuardianDto()
            {
                anchors = anchors
            };
        }
    }

    public abstract class AbstractLBEManageUserGroupOperation : AbstractLBEOperation
    {
        public ulong userId;
    }

    public class LBEAddUserGroupOperation : AbstractLBEManageUserGroupOperation
    {
        public bool isImmersive;

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.MDMAddUserOperation)
                + UMI3DSerializer.Write(userId)
                + UMI3DSerializer.Write(isImmersive);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LBEAddUserGroupOperationDto()
            {
                userId = userId
            };
        }
    }

    public class LBERemoveUserGroupOperation : AbstractLBEManageUserGroupOperation
    {
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.MDMRemoveUserOperation)
                   + UMI3DSerializer.Write(userId);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LBERemoveUserGroupOperationDto()
            {
                userId = userId
            };
        }
    }
}