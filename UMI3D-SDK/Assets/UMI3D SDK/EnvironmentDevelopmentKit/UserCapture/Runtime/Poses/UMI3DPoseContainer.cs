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
    public class UMI3DPoseContainer : SingleBehaviour<UMI3DPoseContainer>, IPoseContainer
    {
        [SerializeField] private List<UMI3DPose_so> allServerPoses = new List<UMI3DPose_so>();

        public List<UMI3DPose_so> GetAllServerPoses()
        { return allServerPoses; }
    }
}