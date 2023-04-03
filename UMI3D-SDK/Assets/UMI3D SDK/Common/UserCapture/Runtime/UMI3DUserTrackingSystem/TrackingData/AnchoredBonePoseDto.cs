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


namespace umi3d.common.userCapture
{
    [System.Serializable]
    public class AnchoredBonePoseDto : BonePoseDto
    {
        public AnchoredBonePoseDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherBone">The anchor bone</param>
        /// <param name="bone">The current bone to update</param>
        /// <param name="position">The position of the bone</param>
        /// <param name="rotation">The rotation of the bone</param>
        public AnchoredBonePoseDto(uint otherBone, uint bone, SerializableVector3 position, SerializableVector4 rotation) : base (bone, position, rotation) 
        { 
            this.otherBone = otherBone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherBone">The anchor bone</param>
        /// <param name="bonePoseDto">The bone DTO corresponding to the current anchor bone pose dto you want to do</param>
        public AnchoredBonePoseDto(uint otherBone, BonePoseDto bonePoseDto) : base (bonePoseDto.bone, bonePoseDto.position, bonePoseDto.rotation)
        {
            this.otherBone = otherBone;
        }

        public AnchoredBonePoseDto(BonePoseDto bonePoseDto) : base(bonePoseDto.bone, bonePoseDto.position, bonePoseDto.rotation)
        {
        }

        /// <summary>
        /// The anchor bone
        /// </summary>
        public uint otherBone { get; private set; }
    }
}
