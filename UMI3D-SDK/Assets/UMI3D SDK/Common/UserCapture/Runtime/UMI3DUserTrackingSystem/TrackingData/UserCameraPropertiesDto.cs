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

using System;

namespace umi3d.common.userCapture
{
    /// <summary>
    /// A request to inform the media about the properties of the user's camera.
    /// Must be sent at least once after the user connection.
    /// </summary>
    [Serializable]
    public class UserCameraPropertiesDto : AbstractBrowserRequestDto
    {
        /// <summary>
        /// Scale of the camera.
        /// </summary>
        public float scale;

        /// <summary>
        /// The projection matrix of the user's camera.
        /// </summary>
        public SerializableMatrix4x4 projectionMatrix;

        /// <summary>
        /// The bone corresponding to the camera's position
        /// </summary>
        public uint boneType;

        /// <inheritdoc/>
        protected override uint GetOperationId() { return UMI3DOperationKeys.UserCameraProperties; }

        /// <inheritdoc/>
        public override Bytable ToBytableArray(params object[] parameters)
        {
            return base.ToBytableArray(parameters)
                + UMI3DSerializer.Write(scale)
                + UMI3DSerializer.Write(projectionMatrix)
                + UMI3DSerializer.Write(boneType);

        }
    }
}