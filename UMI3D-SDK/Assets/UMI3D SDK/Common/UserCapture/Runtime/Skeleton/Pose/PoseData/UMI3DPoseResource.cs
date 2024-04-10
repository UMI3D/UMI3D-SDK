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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    public class UMI3DPoseResource : IUMI3DPoseData
    {
        public PoseAnchorDto Anchor => anchor;
        private PoseAnchorDto anchor;

        public IList<BoneDto> Bones => bones;
        private IList<BoneDto> bones;

        public UMI3DPoseResource(UMI3DPose_so pose_so)
        {
            anchor = pose_so.Anchor;
            bones = pose_so.Bones;
        }

        public PoseDto ToPoseDto()
        {
            return new PoseDto() { bones = new List<BoneDto>(), anchor = new PoseAnchorDto() };
        }

        public PoseDto ToPoseDto(PoseAnchoringType anchoringType, ulong poseRelativeNodeId, uint poseRelativeBone, Vector3 posOffset, Quaternion rotOffset)
        {
            PoseAnchorDto newAnchor;

            switch (anchoringType)
            {
                case PoseAnchoringType.Node:
                    newAnchor = new NodePoseAnchorDto() { bone = Anchor.bone, position = posOffset.Dto(), rotation = rotOffset.Dto(), node = poseRelativeNodeId };
                    break;

                case PoseAnchoringType.Bone:
                    newAnchor = new BonePoseAnchorDto() { bone = Anchor.bone, position = posOffset.Dto(), rotation = rotOffset.Dto(), otherBone = poseRelativeBone };
                    break;

                case PoseAnchoringType.Floor:
                    newAnchor = new FloorPoseAnchorDto() { bone = Anchor.bone, position = posOffset.Dto(), rotation = rotOffset.Dto() };
                    break;

                default:
                    throw new System.Exception("AnchoringType not supported.");
            }

            return new PoseDto { bones = bones.ToList(), anchor = newAnchor };
        }
    }
}
