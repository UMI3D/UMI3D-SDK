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
    /// Links a collection of condtion to a specific pose 
    /// </summary>
    [System.Serializable]
    public class PoseOverriderDto
    {
        public PoseOverriderDto()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="poseConditionDtos">The different condition that are needed for the overrider to get activated</param>
        /// <param name="duration"></param>
        /// <param name="interpolationable"></param>
        /// <param name="composable"></param>
        public PoseOverriderDto(int poseIndexinPoseManager, PoseConditionDto[] poseConditionDtos, DurationDto duration,
            bool interpolationable, bool composable, bool isHoverEnter, bool isHoverExit, bool isRelease, bool isTrigger)
        {
            this.poseIndexinPoseManager = poseIndexinPoseManager;
            this.poseConditions = poseConditionDtos;
            this.duration = duration;
            this.interpolationable = interpolationable;
            this.composable = composable;
            this.isHoverEnter = isHoverEnter;
            this.isHoverExit = isHoverExit;
            this.isRelease = isRelease;
            this.isTrigger = isTrigger;
        }

        /// <summary>
        /// The is a server pose, so this is its index in the list of poses of the user 0
        /// </summary>
        public int poseIndexinPoseManager { get; set; }

        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public PoseConditionDto[] poseConditions { get; set; }

        /// <summary>
        /// How long the pose should last [Not Implemented]
        /// </summary>
        public DurationDto duration { get; set; }
        /// <summary>
        /// If the pose can be interpolated
        /// </summary>
        public bool interpolationable { get; set; }
        /// <summary>
        /// If the pose can be added to  other poses
        /// </summary>
        public bool composable { get; set; }
        /// <summary>
        /// If the pose is activated by a HoverEnter interaction
        /// </summary>
        public bool isHoverEnter { get; set; }
        /// <summary>
        /// If the pose is activated by a HoverExit interaction
        /// </summary>
        public bool isHoverExit { get; set; }
        /// <summary>
        /// If the pose is activated by a Release interaction
        /// </summary>
        public bool isRelease { get; set; }
        /// <summary>
        /// If the pose is activated by a Trigger interaction
        /// </summary>
        public bool isTrigger { get; set; }
    }
}