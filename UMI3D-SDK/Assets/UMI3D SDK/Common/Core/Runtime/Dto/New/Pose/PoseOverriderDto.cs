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
        public PoseOverriderDto(PoseDto pose, PoseConditionDto[] poseConditionDtos, DurationDto duration, bool interpolationable, bool composable)
        {
            this.pose = pose;
            this.poseConditions = poseConditionDtos;
            this.duration = duration;
            this.interpolationable = interpolationable;
            this.composable = composable;
        }

        public PoseDto pose { get; private set; }
        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public PoseConditionDto[] poseConditions { get; private set; }
        public DurationDto duration { get; private set; }
        public bool interpolationable { get; private set; }
        public bool composable { get; private set; }
    }
}

