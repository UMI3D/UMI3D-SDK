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
    public class MagnitudeConditionDto : PoseConditionDto
    {
        public MagnitudeConditionDto() { }

        public MagnitudeConditionDto(float magnitude, uint boneOrigine, uint targetObjectId)
        {
            this.Magnitude = magnitude;
            this.BoneOrigine = boneOrigine;
            this.TargetObjectId = targetObjectId;
        }

        private float magnitude;
        public float Magnitude { get => magnitude; set => magnitude = value; }

        private uint boneOrigin;
        public uint BoneOrigine { get => boneOrigin;  set => boneOrigin = value; }

        private uint targetObjectId;
        public uint TargetObjectId { get => targetObjectId; set => targetObjectId = value; }
    }
}

