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

using System.Collections.Generic;
using System.Linq;

using umi3d.common.userCapture.pose;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// Pose condition true when both of the two contained conditions are true.
    /// </summary>
    public class AndPoseCondition : IPoseAnimatorActivationCondition
    {
        public IPoseAnimatorActivationCondition conditionA;

        public IPoseAnimatorActivationCondition conditionB;

        public AndPoseCondition(IPoseAnimatorActivationCondition conditionA, IPoseAnimatorActivationCondition conditionB)
        {
            this.conditionA = conditionA;
            this.conditionB = conditionB;
        }

        public AbstractPoseConditionDto ToDto()
        {
            return new AndConditionDto()
            {
                ConditionA = conditionA?.ToDto(),
                ConditionB = conditionB?.ToDto()
            };
        }

        public static implicit operator AndPoseCondition(List<IPoseAnimatorActivationCondition> conditions)
        {
            if (conditions == null)
                return null;

            IPoseAnimatorActivationCondition first = conditions.FirstOrDefault();
            IPoseAnimatorActivationCondition last = conditions.LastOrDefault();

            if (conditions.Count <= 2)
                return new AndPoseCondition(first, last);

            return new AndPoseCondition(first, conditions.Skip(1).Reverse().Aggregate(last, (x, y) => x & y));
        }

        /// <summary>
        /// Flatten a tree of nested <see cref="AndPoseCondition"/>.
        /// </summary>
        /// <returns></returns>
        public List<IPoseAnimatorActivationCondition> ToList()
        {
            List<IPoseAnimatorActivationCondition> result = new ();

            result.AddRange((conditionA is AndPoseCondition andConditionA) ? andConditionA.ToList() : new List<IPoseAnimatorActivationCondition>(1) { conditionA });
            result.AddRange((conditionB is AndPoseCondition andConditionB) ? andConditionB.ToList() : new List<IPoseAnimatorActivationCondition>(1) { conditionB });

            return result;
        }
    }
}