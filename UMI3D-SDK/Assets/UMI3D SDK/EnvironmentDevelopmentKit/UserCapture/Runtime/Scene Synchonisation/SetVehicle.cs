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

namespace umi3d.edk.userCapture
{
    public class SetVehicle : Operation
    {
        public ulong VehicleId = 0;

        public SerializableVector3 RelativePosition;

        public SerializableVector4 RelativeRotation;

        public List<uint> streamedBones = new List<uint>();

        public bool MaintainNavigation = true;

        public ulong AnimationId = 0;

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.VehicleRequest)
                + UMI3DNetworkingHelper.Write(VehicleId)
                + UMI3DNetworkingHelper.Write(RelativePosition)
                + UMI3DNetworkingHelper.Write(RelativeRotation)
                + UMI3DNetworkingHelper.Write(streamedBones)
                + UMI3DNetworkingHelper.Write(MaintainNavigation)
                + UMI3DNetworkingHelper.Write(AnimationId);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var setVehicle = new SetVehicleDto()
            {
                VehicleId = this.VehicleId,
                RelativePosition = this.RelativePosition,
                RelativeRotation = this.RelativeRotation,
                streamedBones = this.streamedBones,
                MaintainNavigation = this.MaintainNavigation,
                AnimationId = this.AnimationId
            };
            return setVehicle;
        }
    }
}