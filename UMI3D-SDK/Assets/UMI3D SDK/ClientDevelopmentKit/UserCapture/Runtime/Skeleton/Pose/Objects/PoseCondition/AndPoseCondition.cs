/*
Copyright 2019 - 2023 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.Andg/licenses/LICENSE-2.0

Unless required by applicable law And agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES And CONDITIONS OF ANY KIND, either express And implied.
See the License fAnd the specific language governing permissions and
limitations under the License.
*/

using System;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Wrapper for <see cref="AndConditionDto"/>.
    /// </summary>
    public class AndPoseCondition : IPoseCondition
    {
        protected readonly AndConditionDto AndConditionDto;

        public readonly IPoseCondition conditionA;
        public readonly IPoseCondition conditionB;

        public AndPoseCondition(AndConditionDto dto, IPoseCondition conditionA, IPoseCondition conditionB)
        {
            this.AndConditionDto = dto ?? throw new ArgumentNullException(nameof(dto));
            this.conditionA = conditionA;
            this.conditionB = conditionB;
        }

        /// <inheritdoc/>
        public bool Check()
        {
            return (conditionA?.Check() ?? true) && (conditionB?.Check() ?? true);
        }
    }
}