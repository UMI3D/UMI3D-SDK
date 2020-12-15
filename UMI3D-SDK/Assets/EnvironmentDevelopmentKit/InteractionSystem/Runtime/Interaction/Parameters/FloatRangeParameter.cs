/*
Copyright 2019 Gfi Informatique

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

using System;
using umi3d.common.interaction;

namespace umi3d.edk.interaction
{
    public class FloatRangeParameter : AbstractParameter
    {
        /// <summary>
        /// Current input value.
        /// </summary>
        public float value = 0;

        /// <summary>
        /// Range's minium value.
        /// </summary>
        public float min = 0;

        /// <summary>
        /// Range's maximum value.
        /// </summary>
        public float max = 1;

        /// <summary>
        /// Range's increment value.
        /// </summary>
        public int increment = 0;

        [Serializable]
        public class FloatRangeListener : ParameterEvent<float> { }

        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public FloatRangeListener onChange = new FloatRangeListener();


        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new FloatRangeParameterDto();
        }

        /// <summary>
        /// Writte the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var frDto = dto as FloatRangeParameterDto;
            frDto.min = min;
            frDto.max = max;
            frDto.value = value;
            if (increment != 0)
            {
                frDto.increment = increment;
            }
        }

        ///<inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ParameterSettingRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is FloatRangeParameterDto)
                    {
                        var parameter = settingRequestDto.parameter as FloatRangeParameterDto;
                        float submitedValue = parameter.value;
                        if (submitedValue < min || submitedValue > max)
                        {
                            throw new Exception("Value is out of range");
                        }
                        else
                        {
                            value = submitedValue;
                            onChange.Invoke(new ParameterEventContent<float>(user, settingRequestDto, value));
                        }
                    }
                    else
                        throw new Exception($"parameter of type {settingRequestDto.parameter.GetType()}");
                    break;
                default:
                    throw new Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }
    }
}