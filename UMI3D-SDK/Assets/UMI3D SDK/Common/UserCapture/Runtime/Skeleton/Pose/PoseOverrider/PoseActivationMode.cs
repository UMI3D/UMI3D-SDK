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
    /// How the pose could be activated by the user.
    /// </summary>
    public enum PoseActivationMode : ushort
    {
        /// <summary>
        /// The pose does not depend on a user interaction.
        /// </summary>
        NONE,

        /// <summary>
        /// The pose require a trigger interaction from the user.
        /// </summary>
        TRIGGER,

        /// <summary>
        /// The pose require a release interaction from the user.
        /// </summary>
        RELEASE,

        /// <summary>
        /// The pose require a hover interaction from the user.
        /// </summary>
        HOVER_ENTER,

        /// <summary>
        /// The pose require exiting a hover interaction from the user.
        /// </summary>
        HOVER_EXIT
    }
}