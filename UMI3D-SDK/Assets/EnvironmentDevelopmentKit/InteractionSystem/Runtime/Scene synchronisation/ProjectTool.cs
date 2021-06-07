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
using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.edk.interaction
{

    public class ProjectTool : Operation
    {
        public AbstractTool tool;
        public bool releasable = true;

        public override (int, Func<byte[], int, int, (int, int)>) ToBytes(int baseSize, UMI3DUser user)
        {
            int size = baseSize+ sizeof(uint) + sizeof(ulong) + sizeof(bool);
            Func<byte[], int, int, (int, int)> func = (b, i, bs) => {
                bs += UMI3DNetworkingHelper.Write(UMI3DOperationKeys.ProjectTool, b, ref i);
                bs += UMI3DNetworkingHelper.Write(tool.Id(), b,ref i);
                bs += UMI3DNetworkingHelper.Write(releasable, b,ref i);
                return (i,bs);
            };
            return (size, func); 
        }

        ///<inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new ProjectToolDto() { toolId = tool.Id(), releasable = releasable };
        }
    }
}