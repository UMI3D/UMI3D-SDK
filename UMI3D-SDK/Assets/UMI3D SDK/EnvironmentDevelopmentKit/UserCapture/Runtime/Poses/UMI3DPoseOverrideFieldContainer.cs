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
using UnityEngine;
using umi3d.edk.userCapture;

namespace umi3d.common.collaboration
{
    public class UMI3DPoseOverrideFieldContainer : UMI3DPoseContainer, IPoseOverriderFieldContainer
    {
        private void Start()
        {
            _ = UMI3DPoseManager.Instance;
        }


        [SerializeField] private List<OverriderContainerField> allPoseOverriders = new List<OverriderContainerField>();
        public List<OverriderContainerField> GetAllPoseOverriders()
        {
            return allPoseOverriders;
        }
    }
}

