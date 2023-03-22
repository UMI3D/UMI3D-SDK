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
using UnityEngine;

namespace umi3d.common.userCapture
{
    [Serializable]
    public class UMI3DPoseOverriderMetaClass : IEntity
    {
        [SerializeField] List<UMI3DPoseOveridder_so> poseOverriders = new List<UMI3DPoseOveridder_so>();

        List<PoseOverriderDto> poseOverridersDtos = new List<PoseOverriderDto>();

        public void Init()
        {
            poseOverridersDtos.Clear();
            poseOverriders.ForEach(po =>
            {
                po.pose.onPoseReferencedAndIndexSetted += (indexInPoseManager) =>
                {
                    poseOverridersDtos.Add(po.ToDto(indexInPoseManager));
                };
            });
        }

    }
}

