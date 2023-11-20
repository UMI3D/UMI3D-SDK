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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Wrapper for <see cref="OrConditionDto"/>.
    /// </summary>
    public class OrPoseCondition : IPoseCondition
    {
        protected readonly OrConditionDto orConditionDto;

        public readonly IReadOnlyList<IPoseCondition> conditionsA;
        public readonly IReadOnlyList<IPoseCondition> conditionsB;

        public OrPoseCondition(OrConditionDto dto, IEnumerable<IPoseCondition> conditionsA, IEnumerable<IPoseCondition> conditionsB)
        {
            this.orConditionDto = dto ?? throw new ArgumentNullException(nameof(dto));
            this.conditionsA = conditionsA?.ToList() ?? throw new ArgumentNullException(nameof(conditionsA));
            this.conditionsB = conditionsB?.ToList() ?? throw new ArgumentNullException(nameof(conditionsB));
        }

        /// <inheritdoc/>
        public bool Check()
        {
            if (conditionsA.Count == 0 || conditionsB.Count == 0)
                return true;

            bool conditionsAValidated = true;
            bool conditionsBValidated = true;

            foreach (var condition in conditionsA)
            {
                if (!condition.Check())
                    conditionsAValidated = false;
            }

            if (conditionsAValidated)
                return true;

            foreach (var condition in conditionsB)
            {
                if (!condition.Check())
                    conditionsBValidated = false;
            }

            if (conditionsBValidated)
                return true;

            return false;
        }
    }
}