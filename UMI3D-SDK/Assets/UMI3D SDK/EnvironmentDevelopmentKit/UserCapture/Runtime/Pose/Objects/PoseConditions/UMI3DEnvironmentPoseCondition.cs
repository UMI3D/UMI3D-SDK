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
using umi3d.edk.core;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// Pose condition that is validated by the environment.
    /// </summary>
    public class UMI3DEnvironmentPoseCondition : AbstractLoadableEntity
    {
        /// <summary>
        /// If true, the condition is validated.
        /// </summary>
        public bool isValidated;

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new EnvironmentPoseConditionDto()
            {
                Id = Id(),
                IsValidated = isValidated
            };
        }

        public IEntity ToEntityDto() => ToEntityDto(null);

        /// <summary>
        /// Validate the condition.
        /// </summary>
        /// <returns>Request to validate condition.</returns>
        public ValidateEnvironmentPoseCondition Validate(UMI3DUser user)
        {
            if (isValidated)
                return null;

            isValidated = true;
            return new ValidateEnvironmentPoseCondition(Id(), true)
            {
                users = new() { user }
            };
        }

        /// <summary>
        /// Invalidate the condition.
        /// </summary>
        /// <returns>Request to invalidate condition.</returns>
        public ValidateEnvironmentPoseCondition Invalidate(UMI3DUser user)
        {
            if (!isValidated)
                return null;

            isValidated = false;
            return new ValidateEnvironmentPoseCondition(Id(), false)
            {
                users = new() { user }
            };
        }
    }
}