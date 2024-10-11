/*
Copyright 2019 - 2024 Inetum

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

using System;
using UnityEngine;

namespace umi3d.cdk.notification
{
    public static class UMI3DClientRequestKeys 
    {
        /// <summary>
        /// Request to get information about the player.
        /// </summary>
        public class PlayerRequest
        {
            /// <summary>
            /// The player transform.<br/>
            /// <br/>
            /// Value is <see cref="Transform"/>.
            /// </summary>
            public const string Transform = "Transform";

            /// <summary>
            /// The main camera of the player.<br/>
            /// <br/>
            /// Value is <see cref="Camera"/>.
            /// </summary>
            public const string Camera = "Camera";
        }

        /// <summary>
        /// Request to get information about the left tracker.<br/>
        /// <br/>
        /// See <see cref="TrackerRequest"/> to get the information.
        /// </summary>
        public class LeftTrackerRequest
        {

        }

        /// <summary>
        /// Request to get information about the right tracker.<br/>
        /// <br/>
        /// See <see cref="TrackerRequest"/> to get the information.
        /// </summary>
        public class RightTrackerRequest
        {

        }

        /// <summary>
        /// The tracker information request.
        /// </summary>
        public class TrackerRequest
        {
            /// <summary>
            /// The Bone type of this tracker.<br/>
            /// <br/>
            /// Value is <see cref="uint"/>.
            /// </summary>
            public const string BoneType = "BoneType";

            /// <summary>
            /// The position of this tracker.<br/>
            /// <br/>
            /// Value is <see cref="Vector3"/>.
            /// </summary>
            public const string Position = "Position";

            /// <summary>
            /// The rotation of this tracker.<br/>
            /// <br/>
            /// Value is <see cref="Quaternion"/>.
            /// </summary>
            public const string Rotation = "Rotation";

            /// <summary>
            /// The scale of this tracker.<br/>
            /// <br/>
            /// Value is <see cref="Vector3"/>.
            /// </summary>
            public const string Scale = "Scale";
        }
    }
}