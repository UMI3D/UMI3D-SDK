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
using System.Collections.Generic;
using UnityEngine;  

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Scriptable object to contains data for PoseDto
    /// </summary>
    public class UMI3DPose_so : ScriptableObject
    {
        [SerializeField] List<BoneDto> boneDtos = new List<BoneDto>();
        [SerializeField] BonePoseDto bonePoseDto;

        public List<BoneDto> BoneDtos { get => boneDtos; }
        public BonePoseDto BonePoseDto { get => bonePoseDto; }

        /// <summary>
        /// An event thats called when the PoseManager has played his Start() method
        /// </summary>
        public event Action<int> onPoseReferencedAndIndexSetted;

        public void Init(List<BoneDto> bonePoses, BonePoseDto bonePoseDto)
        {
            this.boneDtos = bonePoses;
            this.bonePoseDto = bonePoseDto;
        }

        public void SendPoseIndexationEvent(int i)
        {
            onPoseReferencedAndIndexSetted.Invoke(i);
        }

        /// <summary>
        /// Transforms the Scriptable Object to its DTO counterpart
        /// </summary>
        /// <returns></returns>
        public PoseDto ToDTO()
        {
            return new PoseDto(boneDtos, bonePoseDto);
        }
    }
}
