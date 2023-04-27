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

using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract client support for simple bindings.
    /// </summary>
    public abstract class AbstractSimpleBinding : AbstractBinding
    {
        protected AbstractSimpleBindingDataDto dto;

        public AbstractSimpleBinding(AbstractSimpleBindingDataDto dto, Transform boundTransform)
        {
            this.dto = dto;
            this.boundTransform = boundTransform;
        }

        /// <summary>
        /// Move the children object based on the parent position.
        /// </summary>
        /// <param name="parentTransform"></param>
        protected virtual void Compute((Vector3 position, Quaternion rotation, Vector3 scale) parentTransform)
        {
            if (dto.syncPosition && dto.syncRotation)
            {
                Quaternion rotation = parentTransform.rotation * dto.offSetRotation;
                Vector3 position = parentTransform.position + dto.anchorPosition + rotation * ((Vector3)dto.offSetPosition - dto.anchorPosition);
                boundTransform.SetPositionAndRotation(position, rotation);
            }
            else if (dto.syncPosition)
            {
                boundTransform.position = parentTransform.position + dto.offSetPosition;
            }
            else if (dto.syncRotation)
            {
                boundTransform.rotation = parentTransform.rotation * dto.offSetRotation;
            }
            if (dto.syncScale)
                boundTransform.localScale = parentTransform.scale + dto.offSetScale;
        }
    }
}