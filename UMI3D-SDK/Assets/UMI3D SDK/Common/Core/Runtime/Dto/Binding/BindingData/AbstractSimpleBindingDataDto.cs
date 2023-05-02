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
        /// <summary>
        /// Do we sync the Rotation of the binding with the rest of the system
        /// </summary>
        public bool syncRotation { get; set; }

        /// <summary>
        /// Do we sync the Scale of the binding with the rest of the system
        /// </summary>
        public bool syncScale { get; set; }

        /// <summary>
        /// Do we sync the position of the binding with the rest of the system
        /// </summary>
        public bool syncPosition { get; set; }

        /// <summary>
        /// offSet Position of the binding
        /// </summary>
        public SerializableVector3 offSetPosition { get; set; }

        /// <summary>
        /// offset rotation of the binding
        /// </summary>
        public SerializableVector4 offSetRotation { get; set; }

        /// <summary>
        /// offSet Scale of the binding
        /// </summary>
        public SerializableVector3 offSetScale { get; set; }

        /// <summary>
        /// referencial position in which the binding is applied. Defined in parent referential.
        /// </summary>
        public SerializableVector3 anchorPosition { get; set; }
    }
}