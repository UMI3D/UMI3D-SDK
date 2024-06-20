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

using System;

using umi3d.common.core;
using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Provide spatial data about a part of a user body.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// True when controller should have its data used as relevant ones.
        /// </summary>
        public bool isActive { set; get; }

        /// <summary>
        /// True if controller should override the tracked bone info.
        /// </summary>
        /// Used for constraints.
        public bool isOverrider { set; get; }

        /// <summary>
        /// Raised when controller is destroyed.
        /// </summary>
        event Action Destroyed;

        /// <summary>
        /// Destroy the controller.
        /// </summary>
        public void Destroy();

        /// <summary>
        /// Bone type associated with the controller.
        /// </summary>
        public uint boneType { get; }

        /// <summary>
        /// Global position of the controller.
        /// </summary>
        public Vector3 position { get; }

        /// <summary>
        /// Global rotation of the controller.
        /// </summary>
        public Quaternion rotation { get; }

        /// <summary>
        /// Local scale of the controller.
        /// </summary>
        public Vector3 scale { get; }

        /// <summary>
        /// Transformation of the controller.
        /// </summary>
        public ITransformation transformation { get; }

        /// <summary>
        /// Export the controller info as a DTO.
        /// </summary>
        /// <returns></returns>
        public ControllerDto ToControllerDto();
    }
}