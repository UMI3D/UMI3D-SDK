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

using UnityEngine;

namespace umi3d.edk.core
{
    /// <summary>
    /// Abstract operation to describe a camera operation.
    /// </summary>
    public abstract class AbstractCameraProperties : Operation
    {
        /// <summary>
        /// The new camera local position.
        /// </summary>
        protected Vector3 localPosition;

        /// <summary>
        /// The new camera near plane.
        /// </summary>
        protected float nearPlane;

        /// <summary>
        /// The new camera far plane.
        /// </summary>
        protected float farPlane;

        public AbstractCameraProperties(float nearPlane, float farPlane, Vector3 localPos)
        {
            this.localPosition = localPos;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
        }
    }
}