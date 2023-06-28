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

namespace umi3d.common.userCapture.tracking
{
    /// <summary>
    /// <see cref="AbstractOperationDto"/> to control the frequency of target tracking updates.
    /// </summary>
    /// Using this operation enables to lower the number of tracked frame per second when required,
    /// reducing the load on the networking system.
    /// A higher FPS will result with a better tracking of the movement, but will have a high impact on the networking load.
    public class SetTrackingTargetFPSDto : AbstractOperationDto
    {
        /// <summary>
        /// Number for tracked frames per second (FPS) sent to the environement
        /// </summary>
        /// Lower FPS leads to less load on the networking part but worse tracking of the user, while higher FPS will produce the opposite.
        public float targetFPS;
    }

    public class SetTrackingBoneTargetFPSDto : SetTrackingTargetFPSDto
    {
        public uint boneType;
    }
}