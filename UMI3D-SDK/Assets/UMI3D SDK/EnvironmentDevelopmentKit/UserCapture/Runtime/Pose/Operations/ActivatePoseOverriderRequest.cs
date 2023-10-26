/*
Copyright 2019 - 2023 Inetum

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
using umi3d.common.userCapture.pose;

namespace umi3d.edk.userCapture.pose
{
    public class ActivatePoseOverriderRequest : Operation
    {
        public ulong poseOverriderId;

        public ActivatePoseOverriderRequest(ulong poseOverriderId)
        {
            this.poseOverriderId = poseOverriderId;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(poseOverriderId);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            ActivatePoseOverriderDto dto = CreateDto();
            WriteProperties(dto, user.Id());
            return dto;
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.ActivatePoseOverriderRequest;
        }

        protected virtual ActivatePoseOverriderDto CreateDto()
        {
            return new ActivatePoseOverriderDto();
        }

        protected virtual void WriteProperties(ActivatePoseOverriderDto dto, ulong userID)
        {
            dto.PoseOverriderId = poseOverriderId;
        }
    }
}