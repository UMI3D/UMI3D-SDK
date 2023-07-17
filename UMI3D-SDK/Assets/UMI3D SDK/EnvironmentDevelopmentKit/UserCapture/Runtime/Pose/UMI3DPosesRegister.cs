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

using inetum.unityUtils;
using System.Collections.Generic;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.edk.userCapture.pose
{
    public sealed class UMI3DPosesRegister : MonoBehaviour, IPosesRegister, IPoseOverridersRegister
    {
        /// <summary>
        /// All the server poses
        /// </summary>
        [SerializeField]
        private List<UMI3DPose_so> environmentPoses = new();

        public IList<UMI3DPose_so> EnvironmentPoses => environmentPoses;

        /// <summary>
        /// The container fields, the init to set up the pose containers
        /// </summary>
        [SerializeField] private List<PoseOverriderContainerField> poseOverriderFields = new();

        public IList<PoseOverriderContainerField> PoseOverriderFields => poseOverriderFields;

        private void Start()
        {
            _ = UMI3DPoseManager.Instance;
        }
    }
}