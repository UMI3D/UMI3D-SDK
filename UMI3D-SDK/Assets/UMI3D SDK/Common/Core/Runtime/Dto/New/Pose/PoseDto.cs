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
    public class PoseDto : UMI3DDto
    {
        public PoseDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bonePoseDtos">all the bone pose that are composing the current pose</param>
        /// <param name="boneAnchor"></param>
        public PoseDto(BonePoseDto[] bones, uint boneAnchor)
        {
            this.bones = bones;
            this.boneAnchor = boneAnchor;
        }

        /// <summary>
        /// all the bone pose that are composing the current pose
        /// </summary>
        public BonePoseDto[] bones { get; private set; }
        public void SetBonePoseDtoArray(BonePoseDto[] bones)
        {
            this.bones = bones;
        }

        public uint boneAnchor { get; private set; }
    }
}
