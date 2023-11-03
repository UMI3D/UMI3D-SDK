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
    public class PlayPoseClipDto : AbstractOperationDto
    {
        /// <summary>
        /// Id of the user to apply the pose to. 
        /// Specify <see cref="UMI3DGlobalID.EnvironementId"/> to set an environment pose.
        /// </summary>
        public ulong userID { get; set; }

        /// <summary>
        /// Index in the list of poses
        /// </summary>
        public ulong poseId { get; set; }

        /// <summary>
        /// Is it a message to stop or to start the related pose
        /// </summary>
        public bool stopPose { get; set; }
    }
}