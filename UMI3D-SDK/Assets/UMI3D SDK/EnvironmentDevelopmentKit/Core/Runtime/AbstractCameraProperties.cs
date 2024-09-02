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
    public abstract class AbstractCameraProperties : Operation
    {
        protected Vector3 localPosition;

        protected float nearPlane;

        protected float farPlane;

        public AbstractCameraProperties(float nearPlane, float farPlane, Vector3 localPos)
        {
            this.localPosition = localPos;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
        }
    }
}