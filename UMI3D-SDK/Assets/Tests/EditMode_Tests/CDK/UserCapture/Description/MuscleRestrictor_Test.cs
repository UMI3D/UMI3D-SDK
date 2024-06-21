/*
Copyright 2019 - 2024 Inetum

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

using NUnit.Framework;
using TestUtils;
using umi3d.cdk.userCapture.description;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Description.CDK
{
    [TestFixture, TestOf(typeof(MuscleRestrictor))]
    public class MuscleRestrictor_Test
    {
        #region Restrict

        private static readonly Quaternion[] restrictTestValues = new Quaternion[]
        {
            Quaternion.identity,
            Quaternion.Euler(0, 0, 0),
            Quaternion.Euler(0, 5, 0),
            Quaternion.Euler(5, 5, 5),
            Quaternion.Euler(0, -5, 0),
            Quaternion.Euler(-5, -5, -5),
            Quaternion.Euler(0, -10, 0),
            Quaternion.Euler(0, 10, 0),
        };

        [Test, TestOf(nameof(MuscleRestrictor.Restrict))]
        public void Restrict([ValueSource(nameof(restrictTestValues))] Quaternion value)
        {
            IUMI3DSkeletonMusclesDefinition.Muscle muscle = new()
            {
                Bonetype = BoneType.Chest,
                ReferenceFrameRotation = Quaternion.identity.Dto(),
                XRotationRestriction = new()
                {
                    min = -2,
                    max = 2
                },
                YRotationRestriction = new()
                {
                    min = -2,
                    max = 2
                },
                ZRotationRestriction = new()
                {
                    min = -2,
                    max = 2
                }
            };

            // given
            MuscleRestrictor muscleRestrictor = new MuscleRestrictor(muscle);
            Quaternion muscleLocalRotation = value;

            // when
            Quaternion muscleLocalRotationRestricted = muscleRestrictor.Restrict(muscleLocalRotation);

            // then
            Vector3 muscleLocalRotationEulersInRange = inetum.unityUtils.math.RotationUtils.ProjectAngleIn180Range(muscleLocalRotation.eulerAngles);
            AssertUnityStruct.AreRotationsEqual(Quaternion.Euler(Mathf.Clamp(muscleLocalRotationEulersInRange.x, muscle.XRotationRestriction.Value.min, muscle.XRotationRestriction.Value.max),
                                                                Mathf.Clamp(muscleLocalRotationEulersInRange.y, muscle.YRotationRestriction.Value.min, muscle.YRotationRestriction.Value.max),
                                                                Mathf.Clamp(muscleLocalRotationEulersInRange.z, muscle.ZRotationRestriction.Value.min, muscle.ZRotationRestriction.Value.max)),
                                                                muscleLocalRotationRestricted);
        }

        [Test]
        public void Restrict_NoRestriction([ValueSource(nameof(restrictTestValues))] Quaternion value)
        {
            IUMI3DSkeletonMusclesDefinition.Muscle muscle = new()
            {
                Bonetype = BoneType.Chest,
                ReferenceFrameRotation = Quaternion.identity.Dto(),
                XRotationRestriction = new()
                {
                    min = -10,
                    max = 10
                },
                YRotationRestriction = new()
                {
                    min = -10,
                    max = 10
                },
                ZRotationRestriction = new()
                {
                    min = -10,
                    max = 10
                }
            };

            // given
            MuscleRestrictor muscleRestrictor = new MuscleRestrictor(muscle);
            Quaternion muscleLocalRotation = value;

            // when
            Quaternion muscleLocalRotationRestricted = muscleRestrictor.Restrict(muscleLocalRotation);

            // then
            AssertUnityStruct.AreEqual(muscleLocalRotation, muscleLocalRotationRestricted);
        }

        #endregion Restrict
    }
}