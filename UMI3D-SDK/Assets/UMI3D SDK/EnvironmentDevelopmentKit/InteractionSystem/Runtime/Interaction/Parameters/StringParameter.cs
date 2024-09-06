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

namespace umi3d.edk.interaction
{
    /// <summary>
    /// Editable <see cref="string"/> parameter.
    /// </summary>
    public class StringParameter : AbstractParameter<string>
    {
        public bool expectLongText;

        [System.Serializable]
        public class StringListener : ParameterEvent<string> { }


        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public StringListener onChange = new StringListener();

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new StringParameterDto();
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
            (dto as StringParameterDto).value = value;
            (dto as StringParameterDto).expectLongText = expectLongText;
        }

        /// <inheritdoc/>
        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.StringParameter;
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DSerializer.Write(value)
                + UMI3DSerializer.Write(expectLongText);
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ParameterSettingRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is StringParameterDto)
                    {
                        var parameter = settingRequestDto.parameter as StringParameterDto;
                        value = parameter.value;
                        expectLongText = parameter.expectLongText;
                        onChange.Invoke(new ParameterEventContent<string>(user, settingRequestDto, value));
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
        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, Vector3Dto bonePosition, Vector4Dto boneRotation, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.ParameterSettingRequest:

                    uint parameterId = UMI3DSerializer.Read<uint>(container);
                    if (UMI3DParameterKeys.String == parameterId)
                    {
                        UMI3DSerializer.Read<bool>(container);
                        value = UMI3DSerializer.Read<string>(container);
                        expectLongText = UMI3DSerializer.Read<bool>(container);
                        onChange.Invoke(new ParameterEventContent<string>(user, toolId, interactionId, hoverredId, boneType, bonePosition, boneRotation, value));
                    }
                    else
                    {
                        throw new System.Exception($"parameter of type {parameterId}");
                    }

                    break;
                default:
                    throw new System.Exception($"User interaction not supported {operationId} ");
            }
        }
    }
}