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

using inetum.unityUtils;

using System;

using umi3d.common.userCapture;
using umi3d.common.userCapture.pose;

using UnityEngine;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// Pose condition computed on browsers.
    /// </summary>
    [Serializable]
    public abstract class AbstractBrowserPoseAnimatorActivationCondition : IPoseAnimatorActivationCondition
    {
        public abstract AbstractPoseConditionDto ToDto();
    }

    [Serializable]
    public class MagnitudeCondition : AbstractBrowserPoseAnimatorActivationCondition
    {
        /// <summary>
        /// Related user bone.
        /// </summary>
        [Tooltip("Related user bone.")]
        [ConstEnum(typeof(BoneType), typeof(uint))]
        public uint Bone;

        /// <summary>
        /// Related node.
        /// </summary>
        [Tooltip("Related node.")]
        public UMI3DNode RelativeNode;

        /// <summary>
        /// Maximum distance between the bone and the related node.
        /// </summary>
        [Tooltip("Maximum distance between the bone and the related node.")]
        public float Distance;

        /// <summary>
        /// Compare only distance on the XY plane.
        /// </summary>
        [Tooltip("Compare only distance on the XY plane.")]
        public bool IgnoreHeight;

        public override AbstractPoseConditionDto ToDto()
        {
            return new MagnitudeConditionDto()
            {
                Magnitude = Distance,
                BoneOrigin = Bone,
                TargetNodeId = RelativeNode.Id(),
                IgnoreHeight = IgnoreHeight
            };
        }
    }

    [Serializable]
    public class DirectionCondition : AbstractBrowserPoseAnimatorActivationCondition
    {
        /// <summary>
        /// Direction from the bone to the node.
        /// </summary>
        [Tooltip("Direction from the bone to the node.")]
        public Vector3 Direction;

        /// <summary>
        /// Related bone. Origin for direction.
        /// </summary>
        [Tooltip("Related bone. Origin for direction.")]
        [ConstEnum(typeof(BoneType), typeof(uint))]
        public uint Bone;

        /// <summary>
        /// Threshold under which the condition is still validated. In degrees.
        /// </summary>
        [Tooltip("Threshold under which the condition is still validated. In degrees.")]
        public float Threshold;

        /// <summary>
        /// Related node.
        /// </summary>
        [Tooltip("Related node.")]
        public UMI3DNode RelativeNode;

        public override AbstractPoseConditionDto ToDto()
        {
            return new DirectionConditionDto()
            {
                Direction = Direction.Dto(),
                BoneId = Bone,
                Threshold = Threshold,
                TargetNodeId = RelativeNode.Id()
            };
        }
    }

    [Serializable]
    public class ScaleCondition : AbstractBrowserPoseAnimatorActivationCondition
    {
        /// <summary>
        /// Target scale for the node to have.
        /// </summary>
        [Tooltip("Target scale for the node to have.")]
        public Vector3 Scale;

        /// <summary>
        /// Object that should have such a scale.
        /// </summary>
        [Tooltip("Object that should have such a scale.")]
        public UMI3DNode Node;

        public override AbstractPoseConditionDto ToDto()
        {
            return new ScaleConditionDto()
            {
                Scale = Scale.Dto(),
                TargetId = Node.Id()
            };
        }
    }

    [Serializable]
    public class BoneRotationCondition : AbstractBrowserPoseAnimatorActivationCondition
    {
        /// <summary>
        /// Related bone.
        /// </summary>
        [Tooltip("Related bone.")]
        [ConstEnum(typeof(BoneType), typeof(uint))]
        public uint Bone;

        /// <summary>
        /// Rotation to validate the condition
        /// </summary>
        [Tooltip("Rotation to validate the condition.")]
        public Quaternion Rotation;

        /// <summary>
        /// Range of rotation in which the condition is still validated. In degrees.
        /// </summary>
        [Tooltip("Range of rotation in which the condition is still validated. In degrees.")]
        public float Threshold;

        public override AbstractPoseConditionDto ToDto()
        {
            return new BoneRotationConditionDto()
            {
                BoneId = Bone,
                Rotation = Rotation.Dto(),
                Threshold = Threshold
            };
        }
    }
}