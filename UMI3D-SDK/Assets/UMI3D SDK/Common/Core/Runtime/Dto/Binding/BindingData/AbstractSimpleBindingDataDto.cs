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

namespace umi3d.common
{
    [System.Serializable]
    public abstract class AbstractSimpleBindingDataDto : AbstractBindingDataDto
    {
        public AbstractSimpleBindingDataDto()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="syncRotation">Do we sync the Rotation of the binding with the rest of the system</param>
        /// <param name="syncScale">Do we sync the Scale of the binding with the rest of the system</param>
        /// <param name="syncPosition">Do we sync the position of the binding with the rest of the system</param>
        /// <param name="offSetPosition">offSet Position of the binding</param>
        /// <param name="offSetRotation">offset rotation of the binding</param>
        /// <param name="offSetScale">offSet Scale of the binding</param>
        /// <param name="priority">level of priority of this binding [impact the order in which it is applied]</param>
        /// <param name="partialFit"> State if the binding can be applied partialy or not. A partial fit can happen in MultyBinding when it's not the binding with the highest priority.</param>
        public AbstractSimpleBindingDataDto(bool syncRotation, bool syncScale, bool syncPosition,
                                SerializableVector3 offSetPosition, SerializableVector4 offSetRotation, SerializableVector3 offSetScale, SerializableVector3 anchorPosition,
                                int priority, bool partialFit)
            : base(priority, partialFit)
        {
            this.syncRotation = syncRotation;
            this.syncScale = syncScale;
            this.syncPosition = syncPosition;
            this.offSetPosition = offSetPosition;
            this.offSetRotation = offSetRotation;
            this.offSetScale = offSetScale;
            this.anchorPosition = anchorPosition;
        }

        /// <summary>
        /// Do we sync the Rotation of the binding with the rest of the system
        /// </summary>
        public bool syncRotation { get; private set; }

        /// <summary>
        /// Do we sync the Scale of the binding with the rest of the system
        /// </summary>
        public bool syncScale { get; private set; }

        /// <summary>
        /// Do we sync the position of the binding with the rest of the system
        /// </summary>
        public bool syncPosition { get; private set; }

        /// <summary>
        /// offSet Position of the binding
        /// </summary>
        public SerializableVector3 offSetPosition { get; private set; }

        /// <summary>
        /// offset rotation of the binding
        /// </summary>
        public SerializableVector4 offSetRotation { get; private set; }

        /// <summary>
        /// offSet Scale of the binding
        /// </summary>
        public SerializableVector3 offSetScale { get; private set; }

        /// <summary>
        /// referencial position in which the binding is applied. Defined in parent referential.
        /// </summary>
        public SerializableVector3 anchorPosition { get; private set; }
    }
}