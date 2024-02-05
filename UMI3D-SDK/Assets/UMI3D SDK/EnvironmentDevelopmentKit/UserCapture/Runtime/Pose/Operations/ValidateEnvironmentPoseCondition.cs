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
    /// <summary>
    /// Operation to validate/invalidate a pose condition.
    /// </summary>
    public class ValidateEnvironmentPoseCondition : Operation
    {
        /// <summary>
        /// Id of the pose condition to validate/invalidate.
        /// </summary>
        public ulong id;

        /// <summary>
        /// If the pose condition to validate/invalidate.
        /// </summary>
        public bool shouldBeValidated;

        public ValidateEnvironmentPoseCondition(ulong id, bool shouldBeValidated)
        {
            this.id = id;
            this.shouldBeValidated = shouldBeValidated;
        }

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(id)
                + UMI3DSerializer.Write(shouldBeValidated);
        }

        /// <inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            ValidateEnvironmentPoseConditionDto requestDto = CreateDto();
            WriteProperties(requestDto, user.Id());
            return requestDto;
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.ValidatePoseConditionRequest;
        }

        protected virtual ValidateEnvironmentPoseConditionDto CreateDto()
        {
            return new ValidateEnvironmentPoseConditionDto();
        }

        protected virtual void WriteProperties(ValidateEnvironmentPoseConditionDto dto, ulong userID)
        {
            dto.Id = id;
            dto.ShouldBeValidated = shouldBeValidated;
        }
    }
}