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
using System.Linq;
using umi3d.common;

namespace umi3d.edk.userCapture
{
    public class SetTrackingTargetFPS : Operation
    {
        public int targetFPS;

        public override (int, Func<byte[], int, int, (int, int)>) ToBytes(int baseSize, UMI3DUser user)
        {
            int size = sizeof(uint) + sizeof(int);
            Func<byte[], int, int, (int, int)> func = (b, i, bs) => {
                bs += UMI3DNetworkingHelper.Write(UMI3DOperationKeys.SetEntityProperty, b, ref i);
                bs += UMI3DNetworkingHelper.Write(targetFPS, b, ref i);
                return (i,bs);
            };
            return (size, func);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var targetFPS = new SetTrackingTargetFPSDto()
            {
                targetFPS = this.targetFPS
            };
            return targetFPS;
        }
    }
}
