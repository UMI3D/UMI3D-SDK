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

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Links a collection of condition to a specific pose 
    /// </summary>
    [System.Serializable]
    public class PoseOverriderDto
    {
        /// <summary>
        /// The is a server pose, so this is its index in the list of poses of the user 0
        /// </summary>
        public int poseIndexInPoseManager { get; set; }

        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public AbstractPoseConditionDto[] poseConditions { get; set; }

        /// <summary>
        /// How long the pose should last [Not Implemented]
        /// </summary>
        public DurationDto duration { get; set; }
        /// <summary>
        /// If the pose can be interpolated
        /// </summary>
        public bool isInterpolable { get; set; }
        /// <summary>
        /// If the pose can be added to  other poses
        /// </summary>
        public bool isComposable { get; set; }
        /// <summary>
        /// How the pose is activated.
        /// </summary>
        public ushort activationMode { get; set; }
    }
}