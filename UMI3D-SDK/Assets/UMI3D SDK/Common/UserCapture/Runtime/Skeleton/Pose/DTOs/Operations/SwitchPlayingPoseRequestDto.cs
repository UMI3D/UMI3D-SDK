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
    /// The dto that forces the client to just play or stop a specific pose
    /// </summary>
    public class SwitchPlayingPoseRequestDto : AbstractOperationDto
    {
        /// <summary>
        /// Id of the pose clip to stop playing.
        /// </summary>
        public ulong posePlayingId { get; set; }

        /// <summary>
        /// Id of the pose clip to start playing.
        /// </summary>
        public ulong poseToPlayId { get; set; }

        /// <summary>
        /// Duration in seconds of the transition between the two poses.
        /// </summary>
        public float transitionDuration;
    }
}