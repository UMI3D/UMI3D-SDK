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

using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture.description
{
    /// <summary>
    /// Controls the restriction applied by a muscle
    /// </summary>
    public class MuscleRestrictor
    {
        private IUMI3DSkeletonMusclesDefinition.Muscle muscleDefinition;

        private float lastAcceptableXAngle = 0f;
        private float lastAcceptableYAngle = 0f;
        private float lastAcceptableZAngle = 0f;

        public MuscleRestrictor(IUMI3DSkeletonMusclesDefinition.Muscle muscle)
        {
            this.muscleDefinition = muscle;
        }

        public Quaternion Restrict(Quaternion boneLocalRotation)
        {
            // compare orientations
            Quaternion referenceFrame = muscleDefinition.ReferenceFrameRotation.Quaternion();
            Vector3 rotation = (referenceFrame * boneLocalRotation).eulerAngles;

            float xAngle = inetum.unityUtils.math.RotationUtils.ProjectAngleIn180Range(rotation.x);
            float yAngle = inetum.unityUtils.math.RotationUtils.ProjectAngleIn180Range(rotation.y);
            float zAngle = inetum.unityUtils.math.RotationUtils.ProjectAngleIn180Range(rotation.z);

            // if restricted, appply restriction, do not move unless a new acceptable value is reached
            static float ClampHysteresis(float angle, ref float lastAcceptableAngle, IUMI3DSkeletonMusclesDefinition.Muscle.RotationRestriction restriction)
            {
                if (restriction.min <= angle && angle <= restriction.max)
                {
                    lastAcceptableAngle = angle;
                    return angle;
                }
                else
                {
                    if (lastAcceptableAngle != 0f)
                        return (lastAcceptableAngle <= (restriction.min + restriction.max) / 2) ? restriction.min : restriction.max;
                    else
                        return Mathf.Clamp(angle, restriction.min, restriction.max);
                }
            }

            float xAngleRestricted = muscleDefinition.XRotationRestriction.HasValue ? ClampHysteresis(xAngle, ref lastAcceptableXAngle, muscleDefinition.XRotationRestriction.Value) : xAngle;
            float yAngleRestricted = muscleDefinition.YRotationRestriction.HasValue ? ClampHysteresis(yAngle, ref lastAcceptableYAngle, muscleDefinition.YRotationRestriction.Value) : yAngle;
            float zAngleRestricted = muscleDefinition.ZRotationRestriction.HasValue ? ClampHysteresis(zAngle, ref lastAcceptableZAngle, muscleDefinition.ZRotationRestriction.Value) : zAngle;

            return Quaternion.Inverse(referenceFrame) * Quaternion.Euler(xAngleRestricted, yAngleRestricted, zAngleRestricted);
        }
    }
}