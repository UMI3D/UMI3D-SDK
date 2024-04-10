/*
Copyright 2019 - 2023 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.Org/licenses/LICENSE-2.0

Unless required by applicable law Or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES Or CONDITIONS OF ANY KIND, either express Or implied.
See the License fOr the specific language governing permissions Or
limitations under the License.
*/

using System;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Wrapper for <see cref="OrConditionDto"/>.
    /// </summary>
    public class OrPoseCondition : IPoseCondition
    {
        protected readonly OrConditionDto OrConditionDto;

        public readonly IPoseCondition conditionA;
        public readonly IPoseCondition conditionB;

        public OrPoseCondition(OrConditionDto dto, IPoseCondition conditionA, IPoseCondition conditionB)
        {
            this.OrConditionDto = dto ?? throw new ArgumentNullException(nameof(dto));
            this.conditionA = conditionA;
            this.conditionB = conditionB;
        }

        /// <inheritdoc/>
        public bool Check()
        {
            return (conditionA?.Check() ?? true) || (conditionB?.Check() ?? true);
        }
    }
}