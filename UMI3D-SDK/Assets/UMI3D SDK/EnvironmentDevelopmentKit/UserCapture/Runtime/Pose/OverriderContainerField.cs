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
using System.Collections;
using System.Collections.Generic;
using umi3d.edk.userCapture;
using umi3d.edk;
using UnityEngine;

namespace umi3d.edk.userCapture.pose
{
    [Serializable]
    public class OverriderContainerField
    {
        /// <summary>
        /// A pose overrider container
        /// </summary>
        [SerializeField] UMI3DPoseOverriderContainer poseOverriderContainer;

        public UMI3DPoseOverriderContainer PoseOverriderContainer { get => poseOverriderContainer; }

        [SerializeField] UMI3DModel _uMI3DModel;
        public UMI3DModel uMI3DModel { get => _uMI3DModel; }

        public void SetNode()
        {
            PoseOverriderContainer.SetNodeId(uMI3DModel.Id());
        }
    }
}
