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
using System;
using System.Linq;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class SkeletonMapper : MonoBehaviour
    {
        const DebugScope scope = DebugScope.Common | DebugScope.UserCapture;

        public BonePoseDto BoneAnchor;
        public SkeletonMapping[] Mappings;

        public virtual PoseDto GetPose()
        {
            try
            {
                var pose = new PoseDto(
                    boneAnchor : BoneAnchor,
                    bones : Mappings.Select(m => m.GetPose()).ToList()
                );

                return pose;
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e,scope);
            }
            return null;
        }
    }
}