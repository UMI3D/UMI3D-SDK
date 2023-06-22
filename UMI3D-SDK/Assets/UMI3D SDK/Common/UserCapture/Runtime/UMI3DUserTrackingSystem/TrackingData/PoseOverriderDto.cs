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

namespace umi3d.common.userCapture
{
    [System.Serializable]
    public class PoseOverriderDto
    {
        public PoseOverriderDto() { }

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

        public int poseIndexinPoseManager { get; set; }
        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public PoseConditionDto[] poseConditions { get; set; }
        public DurationDto duration { get; set; }
        public bool interpolationable { get; set; }
        public bool composable { get; set; }
        public bool isHoverEnter { get; set; }
        public bool isHoverExit { get; set;}
        public bool isRelease { get; set; } 
        public bool isTrigger { get; set; }
    }
}

