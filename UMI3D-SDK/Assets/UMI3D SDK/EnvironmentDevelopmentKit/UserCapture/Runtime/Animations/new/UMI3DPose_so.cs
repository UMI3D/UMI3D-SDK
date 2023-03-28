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
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using UnityEngine;  

namespace umi3d.edk.userCapture
{
    public class UMI3DPose_so : ScriptableObject
    {
        [SerializeField] List<UMI3DBonePose_so> bonePoses = new List<UMI3DBonePose_so>();
        [SerializeField, ConstEnum(typeof(BoneType), typeof(uint))] uint boneAnchor;

        public List<UMI3DBonePose_so> BonePoses { get => bonePoses; }
        public uint BoneAnchor { get => boneAnchor; }

        public void Init(List<UMI3DBonePose_so> bonePoses, uint boneAnchor)
        {
            this.bonePoses = bonePoses;
            this.boneAnchor = boneAnchor;
        }

        /// <summary>
        /// Transforms the Scriptable Object to its DTO counterpart
        /// </summary>
        /// <returns></returns>
        public PoseDto ToDTO()
        {
            List<BoneDto> boneDtos = new List<BoneDto>();

            this.bonePoses

            bonePoses.ForEach(bp =>
            {
                boneDtos.Add(bp.ToDTO());
            });
            return new PoseDto(boneDtos.ToArray(), boneAnchor);
        }
    }
}
