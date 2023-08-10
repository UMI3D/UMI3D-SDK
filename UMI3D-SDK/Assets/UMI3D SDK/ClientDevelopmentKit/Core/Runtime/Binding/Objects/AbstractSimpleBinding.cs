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

using umi3d.common.dto.binding;

using UnityEngine;

namespace umi3d.cdk.binding
{
    /// <summary>
    /// Abstract client support for simple bindings.
    /// </summary>
    public abstract class AbstractSimpleBinding : AbstractBinding
    {
        #region DTO access

        protected AbstractSimpleBindingDataDto SimpleBindingData => data as AbstractSimpleBindingDataDto;

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.syncPosition"/>.
        /// </summary>
        public bool SyncPosition => SimpleBindingData.syncPosition;

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.syncRotation"/>.
        /// </summary>
        public bool SyncRotation => SimpleBindingData.syncRotation;

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.syncScale"/>.
        /// </summary>
        public bool SyncScale => SimpleBindingData.syncScale;

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.offSetPosition"/>.
        /// </summary>
        public Vector3 OffSetPosition => SimpleBindingData.offSetPosition.Struct();

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.offSetRotation"/>.
        /// </summary>
        public Quaternion OffSetRotation => SimpleBindingData.offSetRotation.Quaternion();

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.offSetScale"/>.
        /// </summary>
        public Vector3 OffSetScale => SimpleBindingData.offSetRotation.Struct();

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.anchorPosition"/>.
        /// </summary>
        public Vector3 AnchorPosition => SimpleBindingData.anchorPosition.Struct();

        #endregion DTO access

        private Quaternion originalRotationOffset;

        public AbstractSimpleBinding(AbstractSimpleBindingDataDto dto, Transform boundTransform) : base(boundTransform, dto)
        {
            originalRotationOffset = boundTransform.rotation;
        }

        /// <summary>
        /// Move the children object based on the parent position.
        /// </summary>
        /// <param name="parentTransform"></param>
        protected virtual void Compute((Vector3 position, Quaternion rotation, Vector3 scale) parentTransform)
        {
            if (SyncPosition && SyncRotation)
            {
                Quaternion rotation = parentTransform.rotation * OffSetRotation * originalRotationOffset;
                Vector3 position = parentTransform.position + AnchorPosition + parentTransform.rotation * (OffSetPosition - AnchorPosition);
                boundTransform.SetPositionAndRotation(position, rotation);
            }
            else if (SyncPosition)
            {
                boundTransform.position = parentTransform.position + OffSetPosition;
            }
            else if (SyncRotation)
            {
                boundTransform.rotation = parentTransform.rotation * OffSetRotation * originalRotationOffset;
            }
            if (SyncScale)
                boundTransform.localScale = Vector3.Scale(parentTransform.scale, OffSetScale);
        }
    }
}