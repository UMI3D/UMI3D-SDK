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

using inetum.unityUtils;

using umi3d.common.core;
using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Tracks a part of the curreent user's skeleton and provides its data.
    /// </summary>
    public class Tracker : MonoBehaviour, ITracker, IController
    {
        [EditorReadOnly, SerializeField, ConstEnum(typeof(common.userCapture.BoneType), typeof(uint))]
        protected uint boneType;

        /// <inheritdoc/>
        public uint BoneType => boneType;

        /// <inheritdoc/>
        public bool isActive { get; set; } = true;

        /// <inheritdoc/>
        public bool isOverrider { get; set; } = false;

        /// <inheritdoc/>
        public virtual IController Controller => this;

        /// <inheritdoc/>
        uint IController.boneType => boneType;

        /// <inheritdoc/>
        public Vector3 position => transform.position;

        /// <inheritdoc/>
        public Quaternion rotation => transform.rotation;

        /// <inheritdoc/>
        public Vector3 scale => transform.localScale;

        /// <inheritdoc/>
        public ITransformation transformation => _transformation;

        /// <summary>
        ///  Wrapper for unity transformation.
        /// </summary>
        public UnityTransformation _transformation;

        #region Lifecycle

        public virtual void Init(uint boneType)
        {
            this.boneType = boneType;
            _transformation = new(transform);
        }

        protected void Awake()
        {
            Init(boneType);
        }

        /// <inheritdoc/>
        public event System.Action Destroyed;

        /// <inheritdoc/>
        public void Destroy()
        {
            Destroyed?.Invoke();
            UnityEngine.GameObject.Destroy(this);
        }

        #endregion Lifecycle

        /// <inheritdoc/>
        public ControllerDto ToControllerDto()
        {
            if (boneType is umi3d.common.userCapture.BoneType.None)
                return null;

            return new ControllerDto { boneType = boneType, position = position.Dto(), rotation = rotation.Dto(), isOverrider = isOverrider };
        }
    }
}