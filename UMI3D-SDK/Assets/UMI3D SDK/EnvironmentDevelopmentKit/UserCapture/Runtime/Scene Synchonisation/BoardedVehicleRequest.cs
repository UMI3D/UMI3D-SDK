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
    /// <summary>
    /// Specfic <see cref="VehicleRequest"/> received when a user board in a vehicle.
    /// </summary>
    public class BoardedVehicleRequest : VehicleRequest
    {
        /// <summary>
        /// Id of the body animation to apply when a user is boarded in.
        /// </summary>
        /// See also <seealso cref="UMI3DBodyPoseDto"/>.
        public ulong BodyAnimationId = 0;
        /// <summary>
        /// Will the boarding change the tracked bones list?
        /// </summary>
        public bool ChangeBonesToStream = false;
        /// <summary>
        /// Bones to stop streaming.
        /// </summary>
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

        /// <inheritdoc/>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.BoardedVehicleRequest;
        }

        /// <inheritdoc/>
        protected override Bytable ToBytable()
        {
            if (rotation == null) rotation = new SerializableVector4();
            return base.ToBytable()
                + UMI3DNetworkingHelper.Write(BodyAnimationId)
                + UMI3DNetworkingHelper.Write(ChangeBonesToStream)
                + UMI3DNetworkingHelper.WriteCollection(BonesToStream);
        }

        /// <inheritdoc/>
        protected override NavigateDto CreateDto() { return new BoardedVehicleDto(); }

        /// <inheritdoc/>
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
