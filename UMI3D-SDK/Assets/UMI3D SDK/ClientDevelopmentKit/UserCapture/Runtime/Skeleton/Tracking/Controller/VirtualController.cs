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

using umi3d.common.core;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Provide spatial data about a part of a user body, but does not exist for this browser.
    /// </summary>
    public class VirtualController : IController
    {
        /// <inheritdoc/>
        public uint boneType { get; set; }

        /// <summary>
        /// Local storage for transformation info.
        /// </summary>
        public PureTransformation _transformation { get; set; } = new();

        /// <inheritdoc/>
        public ITransformation transformation
        {
            get
            {
                return _transformation;
            }
            set
            {
                if (value == null)
                    return;

                _transformation.Scale = value.Scale;
                _transformation.Position = value.Position;
                _transformation.Rotation = value.Rotation;
            }
        }

        /// <inheritdoc/>
        public Vector3 position
        {
            get => _transformation.Position;
            set => _transformation.Position = value;
        }

        /// <inheritdoc/>
        public Quaternion rotation
        {
            get => _transformation.Rotation;
            set => _transformation.Rotation = value;
        }

        /// <inheritdoc/>
        public Vector3 scale
        {
            get => _transformation.Scale;
            set => _transformation.Scale = value;
        }

        /// <inheritdoc/>
        public bool isActive { get; set; }

        /// <inheritdoc/>
        public bool isOverrider { get; set; }

        /// <inheritdoc/>
        public event System.Action Destroyed;

        /// <inheritdoc/>
        public void Destroy()
        {
            Destroyed?.Invoke();
        }

        /// <inheritdoc/>
        public virtual ControllerDto ToControllerDto()
        {
            if (boneType is BoneType.None)
                return null;

            return new ControllerDto { boneType = boneType, position = position.Dto(), rotation = rotation.Dto(), isOverrider = isOverrider };
        }
    }
}