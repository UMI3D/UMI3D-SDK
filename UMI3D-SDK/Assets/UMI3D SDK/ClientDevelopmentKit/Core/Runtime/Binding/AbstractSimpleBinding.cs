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
        protected AbstractSimpleBindingDataDto SimpleBindingData => data as AbstractSimpleBindingDataDto;

        public AbstractSimpleBinding(AbstractSimpleBindingDataDto dto, Transform boundTransform) : base(boundTransform, dto)
        {
        }

        /// <summary>
        /// Move the children object based on the parent position.
        /// </summary>
        /// <param name="parentTransform"></param>
        protected virtual void Compute((Vector3 position, Quaternion rotation, Vector3 scale) parentTransform)
        {
            if (SimpleBindingData.syncPosition && SimpleBindingData.syncRotation)
            {
                Quaternion rotation = parentTransform.rotation * SimpleBindingData.offSetRotation;
                Vector3 position = parentTransform.position + SimpleBindingData.anchorPosition.Struct() + rotation * (SimpleBindingData.offSetPosition.Struct() - SimpleBindingData.anchorPosition.Struct());
                boundTransform.SetPositionAndRotation(position, rotation);
            }
            else if (SimpleBindingData.syncPosition)
            {
                boundTransform.position = parentTransform.position + SimpleBindingData.offSetPosition.Struct();
            }
            else if (SimpleBindingData.syncRotation)
            {
                boundTransform.rotation = parentTransform.rotation * SimpleBindingData.offSetRotation;
            }
            if (SimpleBindingData.syncScale)
                boundTransform.localScale = parentTransform.scale + SimpleBindingData.offSetScale.Struct();
        }
    }
}