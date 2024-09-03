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

using umi3d.common;
using UnityEngine;

namespace umi3d.edk.core
{
    /// <summary>
    /// Operation to apply orthographic projection on the camera.
    /// </summary>
    public class OrthographicCameraProperties : AbstractCameraProperties
    {
        /// <summary>
        /// The new camera size.
        /// </summary>
        protected float size;

        public OrthographicCameraProperties(float size, float nearPlane, float farPlane, Vector3 localPos) : base(nearPlane, farPlane, localPos)
        {
            this.size = size;
        }

        /// <summary>
        /// Create an operation for orthographic camera with default settings.
        /// </summary>
        public static AbstractCameraProperties GetDefault()
        {
            return new OrthographicCameraProperties(5, 0.3f, 1000, new Vector3(0, 0.198f, 0.1243f));
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.OrthographicCameraProperties)
                    + UMI3DSerializer.Write(localPosition)
                    + UMI3DSerializer.Write(nearPlane)
                    + UMI3DSerializer.Write(farPlane)
                    + UMI3DSerializer.Write(size);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new OrthographicCameraPropertiesDto()
            {
                localPosition = this.localPosition.Dto(),
                nearPlane = this.nearPlane,
                farPlane = this.farPlane,
                size = this.size
            };
        }
    }
}
