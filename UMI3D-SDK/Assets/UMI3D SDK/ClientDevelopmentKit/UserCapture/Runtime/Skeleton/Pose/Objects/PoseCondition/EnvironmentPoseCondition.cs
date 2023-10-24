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

using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Condition validated/invalidated by a server request.
    /// </summary>
    public class EnvironmentPoseCondition : IPoseCondition
    {
        /// <summary>
        /// True when condition is validated.
        /// </summary>
        public bool IsValidated => dto.IsValidated;

        /// <summary>
        /// Id of the pose condition. Used for requests.
        /// </summary>
        public ulong Id => dto.Id;

        private EnvironmentPoseConditionDto dto;

        public EnvironmentPoseCondition(EnvironmentPoseConditionDto dto)
        {
            this.dto = dto;
        }

        /// <summary>
        /// Validate the condition.
        /// </summary>
        public void Validate()
        {
            dto.IsValidated = true;
        }

        /// <summary>
        /// Invalidate the condition.
        /// </summary>
        public void Invalidate()
        {
            dto.IsValidated = false;
        }

        /// <inheritdoc/>
        public bool Check()
        {
            return IsValidated;
        }
    }
}