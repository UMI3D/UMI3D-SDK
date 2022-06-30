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
    public class BoardedVehicleRequest : VehicleRequest
    {
        public ulong BodyAnimationId = 0;
        public bool ChangeBonesToStream = false;
        public List<uint> BonesToStream = new List<uint>();

        public BoardedVehicleRequest(Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), bool reliable = true) : base(position, rotation, reliable)
        {
            this.BodyAnimationId = 0;
            this.ChangeBonesToStream = false;
            this.BonesToStream = new List<uint>();
        }

        public BoardedVehicleRequest(ulong bodyAnimationId = 0, bool changeBonesToStream = false, List<uint> bonesToStream = null, ulong vehicleId = 0, bool stopNavigation = false, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), bool reliable = true) : base(vehicleId, stopNavigation, position, rotation, reliable)
        {
            this.BodyAnimationId = bodyAnimationId;
            this.ChangeBonesToStream = changeBonesToStream;
            this.BonesToStream = (bonesToStream == null) ? new List<uint>() : bonesToStream;
        }

        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.BoardedVehicleRequest;
        }

        protected override Bytable ToBytable()
        {
            if (rotation == null) rotation = new SerializableVector4();
            return base.ToBytable()
                + UMI3DNetworkingHelper.Write(BodyAnimationId)
                + UMI3DNetworkingHelper.Write(ChangeBonesToStream)
                + UMI3DNetworkingHelper.WriteCollection(BonesToStream);
        }

        protected override NavigateDto CreateDto() { return new BoardedVehicleDto(); }

        protected override void WriteProperties(NavigateDto dto)
        {
            base.WriteProperties(dto);
            if (dto is BoardedVehicleDto vDto)
            {
                vDto.BodyAnimationId = BodyAnimationId;
                vDto.ChangeBonesToStream = ChangeBonesToStream;
                vDto.BonesToStream = BonesToStream;
            }
        }
    }
}
