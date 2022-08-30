﻿/*
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
using umi3d.common;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// <see cref="Operation"/> to control the tracking of camera properties.
    /// </summary>
    public class SetSendingCameraProperties : Operation
    {
        /// <summary>
        /// Should the client send its camera properties? 
        /// </summary>
        public bool activeSending;

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.SetSendingCameraProperty)
                + UMI3DNetworkingHelper.Write(activeSending);
        }

        /// <inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var sendingCamera = new SetSendingCameraPropertiesDto()
            {
                activeSending = this.activeSending
            };
            return sendingCamera;
        }
    }
}