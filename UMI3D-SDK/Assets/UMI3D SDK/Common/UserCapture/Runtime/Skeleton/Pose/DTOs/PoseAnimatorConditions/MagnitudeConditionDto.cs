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
    /// <inheritdoc/><br/>
    /// A condition to check the distance from a bone to a node
    /// </summary>
    [System.Serializable]
    public class MagnitudeConditionDto : AbstractPoseConditionDto
    {
        /// <summary>
        /// distance
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// bone id 
        /// </summary>
        public uint BoneOrigin { get; set; }

        /// <summary>
        /// UMI3D Id of the target object 
        /// </summary>
        public ulong TargetNodeId { get; set; }

        /// <summary>
        /// If true, the magnitude comparison will be done on the XZ plane.
        /// </summary>
        public bool IgnoreHeight { get; set; }
    }
}

