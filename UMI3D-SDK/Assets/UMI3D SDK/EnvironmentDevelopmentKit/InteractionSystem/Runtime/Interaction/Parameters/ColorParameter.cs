/*
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

using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.interaction
{

    public class ColorParameter : AbstractParameter
    {
        /// <summary>
        /// Current input value.
        /// </summary>
        public Color value = Color.white;

        [System.Serializable]
        public class ColorListener : ParameterEvent<Color> { }

        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public ColorListener onChange = new ColorListener();


        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new ColorParameterDto();
        }

        /// <summary>
        /// Write the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            (dto as ColorParameterDto).value = value;
        }

        /// <inheritdoc/>
        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.ColorParameter;
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DSerializer.Write(value);
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ParameterSettingRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is ColorParameterDto parameter)
                    {
                        value = parameter.value;
                        onChange.Invoke(new ParameterEventContent<Color>(user, settingRequestDto, value));
                    }
                    else
                    {
                        throw new System.Exception($"parameter of type {settingRequestDto.parameter.GetType()}");
                    }

                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.ParameterSettingRequest:
                    uint parameterId = UMI3DSerializer.Read<uint>(container);
                    UMI3DSerializer.Read<bool>(container);
                    value = UMI3DSerializer.Read<Color>(container);
                    onChange.Invoke(new ParameterEventContent<Color>(user, toolId, interactionId, hoverredId, boneType, value));
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }
    }
}
