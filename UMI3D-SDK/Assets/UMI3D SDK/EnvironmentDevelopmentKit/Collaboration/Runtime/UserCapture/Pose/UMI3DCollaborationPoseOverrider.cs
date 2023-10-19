﻿/*
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
using System.Linq;
using umi3d.common.collaboration.userCapture.pose.dto;
using umi3d.common.userCapture.pose;
using umi3d.edk.interaction;
using UnityEngine;

namespace umi3d.edk.collaboration.userCapture.pose
{
    public class UMI3DCollaborationPoseOverrider : UMI3DPoseOverrider
    {
        [Header("- Projection condition")]
        public bool HasProjectionCondition;

        public UMI3DInteractable interactable;

        public override AbstractPoseConditionDto[] GetPoseConditions()
        {
            AbstractPoseConditionDto[] baseCopy = base.GetPoseConditions();

            List<AbstractPoseConditionDto> copy = new();

            if (HasProjectionCondition)
            {
                copy.Add(new ProjectedPoseConditionDto()
                {
                    interactableId = interactable.Id()
                });
            }
            return baseCopy.Union(copy).ToArray();
        }
    }
}