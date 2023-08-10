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

using System.Collections.Generic;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Loading helper of the UserCapture module.
    /// </summary>
    public class UMI3DUserCaptureLoadingParameters : UMI3DLoadingParameters, IUMI3DUserCaptureLoadingParameters
    {
        [Header("User Capture")]
        [SerializeField, Tooltip("Hierarchy definition used to instantiate users' skeletons.")]
        private UMI3DSkeletonHierarchyDefinition skeletonHierarchyDefinition;
        public UMI3DSkeletonHierarchyDefinition SkeletonHierarchyDefinition => skeletonHierarchyDefinition;

        [Header("Poses")]
        [SerializeField, Tooltip("Specific poses defined by the browser.")]
        private List<UMI3DPose_so> clientPoses = new List<UMI3DPose_so>();
        public List<UMI3DPose_so> ClientPoses => clientPoses;
    }
}