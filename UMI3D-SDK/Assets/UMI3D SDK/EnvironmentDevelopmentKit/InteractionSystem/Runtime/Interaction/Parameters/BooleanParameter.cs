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

using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.edk.interaction
{
    public class BooleanParameter : AbstractParameter
    {
        /// <summary>
        /// Current input value.
        /// </summary>
        public bool value = false;

        [System.Serializable]
        public class CheckboxListener : ParameterEvent<bool> { }

        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public CheckboxListener onChange = new CheckboxListener();

        /// <summary>
        /// Event raised when value changes to true.
        /// </summary>
        public InteractionEvent onChangeTrue = new InteractionEvent();

        /// <summary>
        /// Event raised when value changes to false.
        /// </summary>
        public InteractionEvent onChangeFalse = new InteractionEvent();


        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new BooleanParameterDto();
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
            (dto as BooleanParameterDto).value = value;
        }

        public override Bytable ToByte(UMI3DUser user)
        {
            return base.ToByte(user)
                + UMI3DNetworkingHelper.Write(value);
        }

        ///<inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ParameterSettingRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is BooleanParameterDto parameter)
                    {
                        value = parameter.value;
                        onChange.Invoke(new ParameterEventContent<bool>(user, settingRequestDto, value));
                        if (parameter.value)
                            onChangeTrue.Invoke(new InteractionEventContent(user, interactionRequest));
                        else
                            onChangeFalse.Invoke(new InteractionEventContent(user, interactionRequest));
                    }
                    else
                        throw new System.Exception($"parameter of type {settingRequestDto.parameter.GetType()}");
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }

        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.ParameterSettingRequest:
                    var parameterId = UMI3DNetworkingHelper.Read<uint>(container);
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    onChange.Invoke(new ParameterEventContent<bool>(user, toolId, interactionId, hoverredId, boneType, value));
                    if (value)
                        onChangeTrue.Invoke(new InteractionEventContent(user, toolId, interactionId, hoverredId, boneType));
                    else
                        onChangeFalse.Invoke(new InteractionEventContent(user, toolId, interactionId, hoverredId, boneType));
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }


    }
}