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
using System.Collections.Generic;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Scriptable object to contains data for PoseDto
    /// </summary>
    [Serializable]
    public class UMI3DPose_so : ScriptableObject
    {
        [SerializeField] private List<BoneDto> boneDtos = new List<BoneDto>();
        [SerializeField] private BonePoseDto bonePoseDto;

        public List<BoneDto> BoneDtos { get => boneDtos; }
        public BonePoseDto BonePoseDto { get => bonePoseDto; }

        /// <summary>
        /// An event thats called when the PoseManager has played his Start() method
        /// </summary>
        public event Action<UMI3DPose_so, int> onPoseReferencedAndIndexSetted;

        public int poseRef { get; private set; }

        public void Init(List<BoneDto> bonePoses, BonePoseDto bonePoseDto)
        {
            this.boneDtos.AddRange(bonePoses);
            this.bonePoseDto = bonePoseDto;
        }

        public void SendPoseIndexationEvent(int i)
        {
            poseRef = i;
            onPoseReferencedAndIndexSetted?.Invoke(this, i);
        }

        /// <summary>
        /// Transforms the Scriptable Object to its DTO counterpart
        /// </summary>
        /// <returns></returns>
        public PoseDto ToDTO()
        {
            return new PoseDto(GetBonesCopy(), GetBonePoseCopy());
        }

        /// <summary>
        /// Gets a copy of all the bones
        /// </summary>
        /// <returns></returns>
        public List<BoneDto> GetBonesCopy()
        {
            List<BoneDto> copy = new List<BoneDto>();
            boneDtos.ForEach(b =>
            {
                copy.Add(new BoneDto()
                {
                    boneType = b.boneType,
                    rotation = new Vector4Dto() { X = b.rotation.X, Y = b.rotation.Y, Z = b.rotation.Z, W = b.rotation.W }
                });
            });
            return copy;
        }

        /// <summary>
        /// Gets a copy of the bone pose
        /// </summary>
        /// <returns></returns>
        public BonePoseDto GetBonePoseCopy()
        {
            BonePoseDto copy = new BonePoseDto()
            {
                Bone = bonePoseDto.Bone,
                Position = new Vector3Dto() { X = bonePoseDto.Position.X, Y = bonePoseDto.Position.Y, Z = bonePoseDto.Position.Z },
                Rotation = new Vector4Dto() { X = bonePoseDto.Rotation.X, Y = bonePoseDto.Rotation.Y, Z = bonePoseDto.Rotation.Z, W = bonePoseDto.Rotation.W }
            };

            return copy;
        }
    }
}