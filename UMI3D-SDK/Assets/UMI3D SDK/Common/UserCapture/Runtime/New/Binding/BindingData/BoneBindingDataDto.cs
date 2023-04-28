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
    public class BoneBindingDataDto : AbstractSimpleBindingDataDto
    {
        public BoneBindingDataDto()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="userId">The user to which the object i going to be binded ID</param>
        /// <param name="boneType">The bone to which the object is going to be binded</param>
        /// <param name="syncRotation">Do we sync the Rotation of the binding with the rest of the system</param>
        /// <param name="syncScale">Do we sync the Scale of the binding with the rest of the system</param>
        /// <param name="syncPosition">Do we sync the position of the binding with the rest of the system</param>
        /// <param name="offSetPosition">offSet Position of the binding</param>
        /// <param name="offSetRotation">offset rotation of the binding</param>
        /// <param name="offSetScale">offSet Scale of the binding</param>
        /// <param name="priority">level of priority of this binding [impact the order in which it is applied]</param>
        /// <param name="partialFit"> State if the binding can be applied partialy or not. A partial fit can happen in MultyBinding when it's not the binding with the highest priority.</param>
        public BoneBindingDataDto(ulong userId, uint boneType,
                        bool syncRotation, bool syncScale, bool syncPosition,
                        SerializableVector3 offSetPosition, SerializableVector4 offSetRotation, SerializableVector3 offSetScale, SerializableVector3 anchorPosition,
                        int priority, bool partialFit)
            : base(syncRotation, syncScale, syncPosition,
                    offSetPosition, offSetRotation, offSetScale, anchorPosition,
                    priority, partialFit)
        {
            this.userId = userId;
            this.boneType = boneType;
        }

        /// <summary>
        /// The user to which the object i going to be binded ID
        /// </summary>
        public ulong userId { get; set; }

        /// <summary>
        /// The bone to which the object is going to be binded
        /// </summary>
        public uint boneType { get; set; }
    }
}