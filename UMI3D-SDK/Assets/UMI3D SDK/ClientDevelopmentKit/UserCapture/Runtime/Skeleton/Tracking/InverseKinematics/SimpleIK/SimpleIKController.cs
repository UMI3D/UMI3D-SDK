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

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking.ik
{
    /// <summary>
    /// 3 points IK controller, compute the rotations of two joints based on the position of the first one, two bone lengths, a goal position and a hint posiion.
    /// </summary>
    public class SimpleIKController
    {
        /// <summary>
        /// Define the plane of the IK solution as well as the direction of it.
        /// </summary>
        public Transform hint;

        /// <summary>
        /// Define the position to attempt to attain with the two bones.
        /// </summary>
        public Transform goal;

        /// <summary>
        /// Origin of the first bone.
        /// </summary>
        public Transform firstJoint;

        /// <summary>
        /// Joint between the first bone and the second one.
        /// </summary>
        public Transform intermediaryJoint;

        /// <summary>
        /// End of the second bone.
        /// </summary>
        public Transform finalJoint;

        /// <summary>
        /// Rotation offset to apply if the bones are not aligned on the Z-axis.
        /// </summary>
        public Quaternion rotationOffset;

        public SimpleIKController(Transform firstJoint, Transform intermediaryJoint, Transform finalJoint, Transform hint, Transform goal, Quaternion rotationOffset)
        {
            this.firstJoint = firstJoint ?? throw new System.ArgumentNullException(nameof(firstJoint));
            this.intermediaryJoint = intermediaryJoint ?? throw new System.ArgumentNullException(nameof(intermediaryJoint));
            this.finalJoint = finalJoint ?? throw new System.ArgumentNullException(nameof(finalJoint));
            this.hint = hint ?? throw new System.ArgumentNullException(nameof(hint));
            this.goal = goal ?? throw new System.ArgumentNullException(nameof(goal));

            this.rotationOffset = rotationOffset != default ? rotationOffset : Quaternion.identity;
        }

        public void Apply()
        {
            // lengthes should not change, but joints may be not at the right places at start
            float firstBoneLength = (firstJoint.transform.position - intermediaryJoint.transform.position).magnitude;
            float secondBoneLength = (intermediaryJoint.transform.position - finalJoint.transform.position).magnitude;

            // use two bones IK to apply IK outside of Unity IK loop.
            var ikResults = TwoBonesInverseKinematics.CalculateRotations(firstJoint.transform.position,
                                                                                    firstBoneLength,
                                                                                    secondBoneLength,
                                                                                    goal.transform.position,
                                                                                    hint.transform.position, rotationOffset);

            firstJoint.transform.rotation = ikResults.originJointRotation;
            firstJoint.transform.localRotation = firstJoint.transform.localRotation * rotationOffset; // correct the rotation, as the calculus assumes that joints are initially aligned on the Z-axis

            intermediaryJoint.transform.rotation = ikResults.intermediaryJointRotation;
            intermediaryJoint.transform.localRotation = intermediaryJoint.transform.localRotation * rotationOffset;
        }
    }
}