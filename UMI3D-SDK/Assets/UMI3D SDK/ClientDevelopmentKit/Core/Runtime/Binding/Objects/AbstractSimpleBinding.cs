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
using umi3d.common.core;
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

        protected AbstractSimpleBindingDataDto SimpleBindingData { get; }

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
        public Vector3 OffSetPosition { get; }

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.offSetRotation"/>.
        /// </summary>
        public Quaternion OffSetRotation { get; }

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.offSetScale"/>.
        /// </summary>
        public Vector3 OffSetScale { get; }

        /// <summary>
        /// See <see cref="AbstractSimpleBindingDataDto.anchorPosition"/>.
        /// </summary>
        public Vector3 AnchorPosition { get; }

        protected bool hasStartedToBeApplied = false;

        #endregion DTO access

        protected Quaternion autoComputedRotationOffset = Quaternion.identity;

        public PureTransformation OriginalTransformation { get; protected set; } = new();

        public AbstractSimpleBinding(AbstractSimpleBindingDataDto dto, Transform boundTransform) : base(boundTransform, dto)
        {
            OffSetPosition = SimpleBindingData.offSetPosition.Struct();
            OffSetRotation = SimpleBindingData.offSetRotation.Quaternion();
            OffSetScale = SimpleBindingData.offSetScale.Struct();
            AnchorPosition = SimpleBindingData.anchorPosition.Struct();
            SimpleBindingData = (AbstractSimpleBindingDataDto)data;
        }

        /// <summary>
        /// Called at the first start of each binding.
        /// </summary>
        protected virtual void Start()
        {
            if (hasStartedToBeApplied)
                return;

            if (boundTransform == null)
            {
                UMI3DLogger.LogWarning($"Bound transform is null. It may have been deleted without removing the binding first.", DebugScope.CDK | DebugScope.Core);
                return;
            }

            OriginalTransformation = PureTransformation.CopyTransform(boundTransform);

            hasStartedToBeApplied = true;
        }


        protected virtual void Compute(ITransformation parentTransformation)
        {
            Compute((parentTransformation.Position, parentTransformation.Rotation, parentTransformation.Scale));
        }

        /// <summary>
        /// Move the children object based on the parent position.
        /// </summary>
        /// <param name="parentTransform"></param>
        protected virtual void Compute((Vector3 position, Quaternion rotation, Vector3 scale) parentTransform)
        {
            if (SyncPosition && SyncRotation)
            {
                Quaternion rotation = parentTransform.rotation * autoComputedRotationOffset * OffSetRotation;
                Vector3 position = parentTransform.position + AnchorPosition + parentTransform.rotation * (OffSetPosition - AnchorPosition);
                boundTransform.SetPositionAndRotation(position, rotation);
            }
            else if (SyncPosition)
            {
                boundTransform.position = parentTransform.position + parentTransform.rotation * OffSetPosition;
            }
            else if (SyncRotation)
            {
                boundTransform.rotation = parentTransform.rotation * autoComputedRotationOffset * OffSetRotation;
            }
            if (SyncScale)
                boundTransform.localScale = Vector3.Scale(parentTransform.scale, OffSetScale);
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            if (boundTransform == null) // object destroyed before binding reset
                return;

            if (SyncPosition && SyncRotation)
                boundTransform.SetLocalPositionAndRotation(OriginalTransformation.LocalPosition, OriginalTransformation.LocalRotation);
            else if (SyncPosition)
                boundTransform.localPosition = OriginalTransformation.LocalPosition;
            else if (SyncRotation)
                boundTransform.localRotation = OriginalTransformation.LocalRotation;

            if (SyncScale)
                boundTransform.localScale = OriginalTransformation.Scale;
        }
    }
}