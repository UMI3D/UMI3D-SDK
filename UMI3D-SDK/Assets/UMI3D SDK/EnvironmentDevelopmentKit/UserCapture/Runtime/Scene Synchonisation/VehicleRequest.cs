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
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    public class VehicleRequest : TeleportRequest
    {
        public ulong VehicleId = 0;
        public ulong BodyPoseId = 0;
        public bool StopNavigation = false;
        public bool ChangeBonesToStream = false;
        public List<uint> BonesToStream = new List<uint>();

        public VehicleRequest(Vector3 position, Quaternion rotation, ulong vehicleId, ulong bodyPoseId, bool stopNavigation, bool changeBonesToStream, List<uint> bonesToStream, bool reliable) : base(position, rotation, reliable)
        {
            this.VehicleId = vehicleId;
            this.BodyPoseId = bodyPoseId;
            this.StopNavigation = stopNavigation;
            this.ChangeBonesToStream = changeBonesToStream;
            this.BonesToStream = bonesToStream;
        }

        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.VehicleRequest;
        }

        protected override Bytable ToBytable()
        {
            if (rotation == null) rotation = new SerializableVector4();
            return base.ToBytable()
                + UMI3DNetworkingHelper.Write(VehicleId)
                + UMI3DNetworkingHelper.Write(VehicleId)
                + UMI3DNetworkingHelper.Write(VehicleId)
                + UMI3DNetworkingHelper.Write(VehicleId)
                + UMI3DNetworkingHelper.Write(VehicleId);
        }

        protected override NavigateDto CreateDto() { return new VehicleDto(); }
        protected override void WriteProperties(NavigateDto dto)
        {
            base.WriteProperties(dto);
            if (dto is VehicleDto vDto)
            {
                vDto.VehicleId = VehicleId;
                vDto.BodyPoseId = BodyPoseId;
                vDto.StopNavigation = StopNavigation;
                vDto.ChangeBonesToStream = ChangeBonesToStream;
                vDto.BonesToStream = BonesToStream;
            }
        }
    }
}
