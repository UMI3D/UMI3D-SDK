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

namespace umi3d.common.userCapture.pose
{
    [System.Serializable]
    public class BoneRotationConditionDto : PoseConditionDto
    {
        public BoneRotationConditionDto() { }

        public BoneRotationConditionDto(uint boneId, Vector4Dto rotation)
        {
            BoneId = boneId;
            Rotation = rotation;
        }

        private uint boneId;
        public uint BoneId { get => boneId; set => boneId = value; }

        private Vector4Dto rotation;
        public Vector4Dto Rotation { get => rotation; set => rotation = value; }

        private float acceptanceRange;
        public float AcceptanceRange { get => acceptanceRange; set => acceptanceRange = value; }
    }
}
