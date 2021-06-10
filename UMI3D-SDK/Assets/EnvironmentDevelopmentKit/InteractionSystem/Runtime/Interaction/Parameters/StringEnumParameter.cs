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

using System.Collections.Generic;
using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.edk.interaction
{
    public class StringEnumParameter : AbstractParameter
    {
        /// <summary>
        /// Current value.
        /// </summary>
        public string value = null;

        /// <summary>
        /// Availables options.
        /// </summary>
        public List<string> options = new List<string>();

        [System.Serializable]
        public class OnChangeListener : ParameterEvent<string> { }

        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public OnChangeListener onChange = new OnChangeListener();

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new EnumParameterDto<string>();
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
            var epDto = dto as EnumParameterDto<string>;
            epDto.possibleValues = options;
            epDto.value = value;
        }

        ///<inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ParameterSettingRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is EnumParameterDto<string>)
                    {
                        value = (settingRequestDto.parameter as EnumParameterDto<string>).value;
                        onChange.Invoke(new ParameterEventContent<string>(user, settingRequestDto, value));
                    }
                    else
                        throw new System.Exception($"parameter of type {settingRequestDto.parameter.GetType()}");
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }

        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, byte[] array, int position, int length)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.ParameterSettingRequest:
                    var parameterId = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                    if (UMI3DParameterKeys.Enum == parameterId)
                    {
                        value = UMI3DNetworkingHelper.Read<string>(array, position, length);
                        onChange.Invoke(new ParameterEventContent<string>(user, toolId, interactionId, hoverredId, boneType, value));
                    }
                    else
                        throw new System.Exception($"parameter of type {parameterId}");
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }
    }
}