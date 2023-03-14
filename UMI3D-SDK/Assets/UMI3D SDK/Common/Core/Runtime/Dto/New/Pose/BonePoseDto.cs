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
    public class BonePoseDto : UMI3DDto
    {
        public BonePoseDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bone">The current bone to update</param>
        /// <param name="position">The position of the bone</param>
        /// <param name="rotation">The rotation of the bone</param>
        public BonePoseDto(uint bone, SerializableVector3 position, SerializableVector4 rotation)
        {
            this.bone = bone;
            this.position = position;
            this.rotation = rotation;
        }

        public BonePoseDto(uint bone, SerializableVector4 rotation)
        {
            this.bone = bone;
            this.rotation = rotation;
        }

        /// <summary>
        /// The current bone to update
        /// </summary>
        public readonly uint bone;
        /// <summary>
        /// The position of the bone
        /// </summary>
        public SerializableVector3 position { get; private set; }
        /// <summary>
        /// The rotation of the bone
        /// </summary>
        public SerializableVector4 rotation { get; private set; }
    }
}
