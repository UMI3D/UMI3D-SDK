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

namespace umi3d.common.userCapture.animation
{
    /// <summary>
    /// Keys used to request browsers to compute themselves the animator parameters for a skeleton. <br/>
    /// Each key result in a different computation formula in browsers.
    /// </summary>
    /// It ensure smooth animations on clients.
    public enum SkeletonAnimatorParameterKeys : uint
    {
        /// <summary>
        /// Magnitude of the speed on all axes.
        /// </summary>
        SPEED,

        /// <summary>
        /// Speed on the X axis.
        /// </summary>
        SPEED_X,

        /// <summary>
        /// Absolute speed on the X axis.
        /// </summary>
        SPEED_ABS_X,

        /// <summary>
        /// Speed on the Y axis.
        /// </summary>
        SPEED_Y,

        /// <summary>
        /// Absolute speed on the Y axis.
        /// </summary>
        SPEED_ABS_Y,

        /// <summary>
        /// Speed on the Z axis.
        /// </summary>
        SPEED_Z,

        /// <summary>
        /// Absolute speed on the Z axis.
        /// </summary>
        SPEED_ABS_Z,

        /// <summary>
        /// Magnitude of the speed on the XY plane.
        /// </summary>
        SPEED_X_Z,

        /// <summary>
        /// State if the avatar is jumping.
        /// </summary>
        JUMP,

        /// <summary>
        /// State if the avatar is jumping.
        /// </summary>
        CROUCH,
    }
}