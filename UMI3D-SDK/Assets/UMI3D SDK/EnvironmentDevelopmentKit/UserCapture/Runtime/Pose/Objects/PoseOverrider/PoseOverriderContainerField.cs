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
using UnityEngine;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// Field to bind a pose overrider to a UMI3D model.
    /// </summary>
    [Serializable]
    public class PoseOverriderContainerField
    {
        /// <summary>
        /// A pose overrider container
        /// </summary>
        [SerializeField] 
        private UMI3DPoseOverriderContainer poseOverriderContainer;

        public UMI3DPoseOverriderContainer PoseOverriderContainer => poseOverriderContainer;

        [SerializeField, Tooltip("Model that is used for pose conditions.")] 
        private UMI3DModel model;
        /// <summary>
        /// Model that is used for pose conditions.
        /// </summary>
        public UMI3DModel Model => model;

        public void Init()
        {
            PoseOverriderContainer.Init(nodeId: Model.Id());
        }
    }
}