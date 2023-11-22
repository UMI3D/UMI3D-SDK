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
    /// Condition to validate the activation of a pose animator.
    /// </summary>
    public interface IPoseAnimatorActivationCondition
    {
        public AbstractPoseConditionDto ToDto();

        #region operators

        #region OR

        public static OrPoseCondition operator |(IPoseAnimatorActivationCondition conditionA, IPoseAnimatorActivationCondition conditionB)
        {
            return new OrPoseCondition()
            {
                conditionsA = new IPoseAnimatorActivationCondition[] { conditionA },
                conditionsB = new IPoseAnimatorActivationCondition[] { conditionB }
            };
        }

        public static OrPoseCondition operator |(IEnumerable<IPoseAnimatorActivationCondition> conditionsA, IPoseAnimatorActivationCondition conditionB)
        {
            return new OrPoseCondition()
            {
                conditionsA = conditionsA,
                conditionsB = new IPoseAnimatorActivationCondition[] { conditionB }
            };
        }

        public static OrPoseCondition operator |(IPoseAnimatorActivationCondition conditionA, IEnumerable<IPoseAnimatorActivationCondition> conditionsB)
        {
            return new OrPoseCondition()
            {
                conditionsA = new IPoseAnimatorActivationCondition[] { conditionA },
                conditionsB = conditionsB
            };
        }

        #endregion OR

        #region NOT

        public static NotPoseCondition operator !(IPoseAnimatorActivationCondition condition)
        {
            return new NotPoseCondition()
            {
                conditionsToNegate = new IPoseAnimatorActivationCondition[] { condition }
            };
        }

        #endregion NOT

        #endregion operators
    }
}