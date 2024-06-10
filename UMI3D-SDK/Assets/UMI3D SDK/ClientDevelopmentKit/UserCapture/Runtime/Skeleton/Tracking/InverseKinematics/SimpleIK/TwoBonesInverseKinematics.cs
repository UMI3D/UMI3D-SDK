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
    /// Contains utilities to compute Inverse Kinematics with only two bones.
    /// </summary>
    /// This model is a simplified IK model that only manages two bones, but is exact and very lightweight.
    public static class TwoBonesInverseKinematics
    {
        /// <summary>
        /// Provide the rotations to apply to the two bones to attempt to attain a certain goal.
        /// </summary>
        /// <param name="firstBonePosition"></param>
        /// <param name="firstBoneLength"></param>
        /// <param name="secondBoneLength"></param>
        /// <param name="goal"></param>
        /// <param name="hint"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static (Quaternion originJointRotation, Quaternion intermediaryJointRotation) CalculateRotations(Vector3 firstBonePosition, float firstBoneLength, float secondBoneLength, Vector3 goal, Vector3 hint, Quaternion offset)
        {
            (Vector3 intermediaryJointPosition, Vector3 finalJointPosition) = CalculatePositions(firstBonePosition, firstBoneLength, secondBoneLength, goal, hint);

            Vector3 forwardDirectionFirst = (intermediaryJointPosition - firstBonePosition);
            Vector3 upwardDiretionFirst = hint - Vector3.Project(intermediaryJointPosition - firstBonePosition, hint);

            Vector3 forwardDirectionSecond = (finalJointPosition - intermediaryJointPosition);
            Vector3 upwardDiretionSecond = hint - Vector3.Project(finalJointPosition - intermediaryJointPosition, hint);

            Quaternion rotationFirst = Quaternion.LookRotation(forwardDirectionFirst, upwardDiretionFirst);
            Quaternion rotationSecond = Quaternion.LookRotation(forwardDirectionSecond, upwardDiretionSecond);

            return (rotationFirst, rotationSecond);
        }

        /// <summary>
        /// Provide the positions of the two bones ends to attempt to attain a certain goal.
        /// </summary>
        /// <param name="firstBonePosition"></param>
        /// <param name="firstBoneLength"></param>
        /// <param name="secondBoneLength"></param>
        /// <param name="goal"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public static (Vector3 intermediaryJointPosition, Vector3 finalJointPosition) CalculatePositions(Vector3 firstBonePosition, float firstBoneLength, float secondBoneLength, Vector3 goal, Vector3 hint)
        {
            float goalDistance = (firstBonePosition - goal).magnitude;

            Vector3 intermediaryJointPosition;
            Vector3 finalJointPosition;

            if (goalDistance >= firstBoneLength + secondBoneLength) // goal too far
            {
                // align 3 points with last point as far as possible from first
                Vector3 goalDirecton = (goal - firstBonePosition).normalized;
                intermediaryJointPosition = firstBonePosition + goalDirecton * firstBoneLength;
                finalJointPosition = intermediaryJointPosition + goalDirecton * secondBoneLength;

                return (intermediaryJointPosition, finalJointPosition);
            }
            else if (goalDistance < Mathf.Abs(firstBoneLength - secondBoneLength)) // goal to close from origin
            {
                // align 3 points with last point as close as possible from first
                Vector3 goalDirecton = (goal - firstBonePosition).normalized;
                intermediaryJointPosition = firstBonePosition + goalDirecton * firstBoneLength;
                finalJointPosition = intermediaryJointPosition - goalDirecton * secondBoneLength;

                return (intermediaryJointPosition, finalJointPosition);
            }
            else // two bones IK with 3 points
            {
                // IK with two bones is basically finding the intersections of two spheres.
                // One sphere is centered on the start joint of the first bone and the other is centered on the goal position.
                var intersectionDisc = GeometryUtility.SphereSphereIntersection((center: firstBonePosition, radius: firstBoneLength),
                                                                                                                                         (center: goal, radius: secondBoneLength));

                if (!intersectionDisc.HasValue) // should not happen because already checked by the two previous cases
                    return default;

                // project in plane
                Vector3 planeNormal = Vector3.Cross(firstBonePosition - goal, hint - goal).normalized;

                // find intersection with plane though the center of the circle
                var intersections = GeometryUtility.CirclePlaneIntersection(intersectionDisc.Value, planeNormal);

                if (!intersections.HasValue)
                    return default; // should not happen, because spheres intersects

                var (root1Position, root2Position) = intersections.Value;

                // get the intersection point that is closer to the hint point
                Vector3 closestIntersection = ((hint - root1Position).magnitude <= (hint - root1Position).magnitude) ? root1Position : root2Position;

                intermediaryJointPosition = closestIntersection;
                finalJointPosition = goal; // perfectly match the goal

                return (intermediaryJointPosition, finalJointPosition);
            }
        }
    }

    /// <summary>
    /// Class containing 3D geometry utilities.
    /// </summary>
    public static class GeometryUtility
    {
        /// <summary>
        /// Compute the two point of intersection between a plane and a circle in a 3D space, assuming that the plane goes through the circle center.
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="planeNormal"></param>
        /// <returns>Null if circle and plane are collinear</returns>
        public static (Vector3 root1, Vector3 root2)? CirclePlaneIntersection((Vector3 center, Vector3 normal, float radius) circle, Vector3 planeNormal)
        {
            if (Mathf.Abs(Vector3.Dot(circle.center.normalized, planeNormal.normalized)) == 1) // circle and plane are collinear
                return null;

            // the roots are the two points of the plane that intersects with a intersection disc.
            Vector3 planeBaseX = circle.normal;
            Vector3 planeBaseY = Vector3.Cross(planeNormal.normalized, planeBaseX).normalized;

            Vector3 root1Position = circle.center + circle.radius * planeBaseY;
            Vector3 root2Position = circle.center - circle.radius * planeBaseY;

            return (root1Position, root2Position);
        }

        /// <summary>
        /// Compute the intersection disc between two sphere.
        /// </summary>
        /// <param name="sphere1"></param>
        /// <param name="sphere2"></param>
        /// <returns>Null if no overlap.</returns>
        public static (Vector3 intersectionDiscCenter, Vector3 intersectionDiscNormal, float intersectionDiscRadius)? SphereSphereIntersection((Vector3 center, float radius) sphere1, (Vector3 center, float radius) sphere2)
        {
            float d = (sphere1.center - sphere2.center).magnitude;

            // check overlapping occurs
            if (d < Mathf.Abs(sphere1.radius - sphere2.radius) // Spheres one in another
                || d > sphere1.radius + sphere2.radius // Spheres one outside another
                || d == 0) // Spheres with same center
            {
                return null;
            }

            // h is distance to intersection disc center from circle 2 (formula retrieved from 2IH . AB = (r_A)^2 - (r_B)^2 )
            float h = (0.5f) * (-Mathf.Abs(sphere1.radius * sphere1.radius - sphere2.radius * sphere2.radius) / d + d);
            Vector3 sphereIntersectionDiscCenter = sphere2.center - h * (sphere2.center - sphere1.center).normalized;

            // pythagoreas give the intersection disc radius :
            float intersectionDiscRadius = Mathf.Sqrt(Mathf.Abs((sphere2.radius * sphere2.radius) - h * h));

            return (sphereIntersectionDiscCenter, (sphere1.center - sphere2.center).normalized, intersectionDiscRadius);
        }
    }
}