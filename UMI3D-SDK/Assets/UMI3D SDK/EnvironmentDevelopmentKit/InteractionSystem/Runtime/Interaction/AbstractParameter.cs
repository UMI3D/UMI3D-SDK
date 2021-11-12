﻿/*
Copyright 2019 - 2021 Inetum

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

using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    public abstract class AbstractParameter : AbstractInteraction
    {
        /// <summary>
        /// Event when an interaction is performed on a parameter.
        /// </summary>
        /// <typeparam name="T">Type of the parameter value.</typeparam>
        [System.Serializable]
        public class ParameterEvent<T> : UnityEvent<ParameterEventContent<T>> { }

        /// <summary>
        /// Parameter interaction Event content.
        /// </summary>
        /// <typeparam name="T">Type of the parameter value.</typeparam>
        [System.Serializable]
        public class ParameterEventContent<T> : InteractionEventContent
        {
            /// <summary>
            /// New value.
            /// </summary>
            public T value;

            public ParameterEventContent(UMI3DUser user, ParameterSettingRequestDto dto, T value) : base(user, dto)
            {
                this.value = value;
            }

            public ParameterEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType, T value) : base(user, toolId, id, hoveredObjectId, boneType)
            {
                this.value = value;
            }
        }
    }
}
