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

using System.Collections.Generic;

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Specialized <see cref="VehicleDto"/> for avatar boarded in vehicles.
    /// </summary>
    /// This DTO describes how a skeletoon should react when boarding in a vehicle, 
    /// typically by being applyied a body pose (see <seealso cref="UMI3DBodyPoseDto"/>)
    /// or changing the number of streamed bones.
    [System.Serializable]
    public class BoardedVehicleDto : VehicleDto
    {
        /// <summary>
        /// Id of the body animation to apply when a user is boarded in.
        /// </summary>
        /// See also <seealso cref="UMI3DBodyPoseDto"/>.
        public ulong BodyAnimationId = 0;

        /// <summary>
        /// Should streamed bones been overidded by the sent list?
        /// </summary>
        public bool ChangeBonesToStream = false;

        /// <summary>
        /// Bones NOT to stream list using bones id from <see cref="BoneType"/>.
        /// </summary>
        //! Rename to a more explicit name
        public List<uint> BonesToStream = new List<uint>();

        public BoardedVehicleDto() : base() { }
    }
}