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
    public class OmniscientViewPropertiesDto : AbstractViewModeProperties
    {
        /// <summary>
        /// The distance of the camera.
        /// </summary>
        public float distance { get; set; }
        /// <summary>
        /// The movement speed of forward, backward and lateral movements.
        /// </summary>
        public float flyingSpeed { get; set; }
        /// <summary>
        /// The limit of the camera zoom
        /// </summary>
        public Vector2 zoomLimit { get; set; }
        /// <summary>
        /// The speed of the Zoom
        /// </summary>
        public float zoomSpeed { get; set; }

        public OmniscientViewPropertiesDto(Vector3 localPosition, float nearPlane, float farPlane, float fieldOFView, Vector2 cameraXangle, float distance, float flyingSpeed, Vector2 zoomLimit, float zoomSpeed) : base(localPosition, nearPlane, farPlane, fieldOFView, cameraXangle)
        {
            this.localPosition = localPosition;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
            this.fieldOfView = fieldOFView;
            this.cameraXAngle = cameraXangle;
            this.distance = distance;
            this.flyingSpeed = flyingSpeed;
            this.zoomLimit = zoomLimit;
            this.zoomSpeed = zoomSpeed;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.OmniscientView)
                + UMI3DSerializer.Write(localPosition)
                + UMI3DSerializer.Write(nearPlane)
                + UMI3DSerializer.Write(farPlane)
                + UMI3DSerializer.Write(fieldOfView)
                + UMI3DSerializer.Write(cameraXAngle)
                + UMI3DSerializer.Write(distance)
                + UMI3DSerializer.Write(flyingSpeed)
                + UMI3DSerializer.Write(zoomLimit)
                + UMI3DSerializer.Write(zoomSpeed);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new OmniscientViewDto()
            {
                localPosition = this.localPosition.Dto(),
                nearPlane = this.nearPlane,
                farPlane = this.farPlane,
                cameraXAngle = this.cameraXAngle.Dto(),
                fieldOfView = this.fieldOfView,
                distance = this.distance,
                flyingSpeed = this.flyingSpeed,
                zoomLimit = this.zoomLimit.Dto(),
                zoomSpeed = this.zoomSpeed,
            };
        }
    }
}
