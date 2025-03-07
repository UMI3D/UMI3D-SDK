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
using umi3d.common.lbe.description;

namespace umi3d.common.lbe
{
    public class AbstractLBEOperationDto : AbstractOperationDto { }

    public class LBEActivationDto : AbstractLBEOperationDto { }

    public class LBELeaderDto : AbstractLBEOperationDto
    {
        public bool isLeader { get; set; }
    }

    public class LBESetUserGroupDto : AbstractLBEOperationDto
    {
        public ulong groupId { get; set; }
        
        public List<ulong> colocatedUserIds { get; set; }
    }

    public class LBESetGuardianDto : AbstractLBEOperationDto
    {
        public List<ARAnchorDto> anchors { get; set; }
    }

    public abstract class AbstractLBEManageUserGroupOperationDto : AbstractLBEOperationDto
    {
        public ulong userId { get; set; }
    }

    public class LBEAddUserGroupOperationDto : AbstractLBEManageUserGroupOperationDto { }

    public class LBERemoveUserGroupOperationDto : AbstractLBEManageUserGroupOperationDto { }
}
