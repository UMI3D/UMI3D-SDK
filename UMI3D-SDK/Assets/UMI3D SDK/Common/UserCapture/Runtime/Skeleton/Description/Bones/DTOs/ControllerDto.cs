/*
Copyright 2019 - 2021 Inetum

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
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    [Serializable]
    public class ControllerDto : BoneDto
    {
        /// <summary>
        /// Position relative to the tracked node.
        /// </summary>
        public Vector3Dto position { get; set; }

        public bool isOverrider { get; set; }
    }

    public readonly struct ControllerPose
    {
        /// <summary>
        /// Defines the type of the bone.
        /// </summary>
        public readonly uint boneType;

        /// <summary>
        /// Rotation of the bone in world space
        /// </summary>
        public readonly UnityEngine.Quaternion rotation;

        /// <summary>
        /// Position relative to the tracked node.
        /// </summary>
        public readonly UnityEngine.Vector3 position;

        public readonly bool isOverrider;

        public ControllerPose(uint boneType, Vector3 position, Quaternion rotation, bool isOverrider = false)
        {
            this.boneType = boneType;
            this.rotation = rotation;
            this.position = position;
            this.isOverrider = isOverrider;
        }
    }
}