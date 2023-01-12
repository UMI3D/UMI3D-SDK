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

using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Request relative to vehicle boarding.
    /// </summary>
    public class VehicleRequest : TeleportRequest
    {
        /// <summary>
        /// Vehicle boarded in UMI3D id.
        /// </summary>
        public ulong VehicleId = 0;
        /// <summary>
        /// Should the navigation be blocked in the vehicle ?
        /// </summary>
        public bool StopNavigation = false;

        public VehicleRequest(Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) : base(position, rotation)
        {
            this.VehicleId = 0;
            this.StopNavigation = false;
        }

        public VehicleRequest(ulong vehicleId = 0, bool stopNavigation = false, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) : base(position, rotation)
        {
            this.VehicleId = vehicleId;
            this.StopNavigation = stopNavigation;
        }

        /// <inheritdoc/>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.VehicleRequest;
        }

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            if (rotation == null) rotation = new SerializableVector4();
            return base.ToBytable(user)
                + UMI3DSerializer.Write(VehicleId)
                + UMI3DSerializer.Write(StopNavigation);
        }

        /// <inheritdoc/>
        protected override NavigateDto CreateDto() { return new VehicleDto(); }

        /// <inheritdoc/>
        protected override void WriteProperties(NavigateDto dto)
        {
            base.WriteProperties(dto);
            if (dto is VehicleDto vDto)
            {
                vDto.VehicleId = VehicleId;
                vDto.StopNavigation = StopNavigation;
            }
        }
    }
}


