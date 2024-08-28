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

namespace umi3d.edk.core
{
    public class OrthographicCameraProperties : AbstractCameraProperties
    {
        protected float size;

        public OrthographicCameraProperties(float size, float nearPlane, float farPlane) : base(nearPlane, farPlane)
        {
            this.size = size;
        }

        public override AbstractCameraProperties SetDefault()
        {
            return new OrthographicCameraProperties(5, 0.3f, 1000);
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.PerspectiveCameraProperties)
                    + UMI3DSerializer.Write(nearPlane)
                    + UMI3DSerializer.Write(farPlane)
                    + UMI3DSerializer.Write(size);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new OrthographicCameraPropertiesDto()
            {
                nearPlane = this.nearPlane,
                farPlane = this.farPlane,
                size = this.size
            };
        }
    }
}
