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

using System;

namespace umi3d.common.interaction
{
    /// <summary>
    /// Dto to request the setting of one parameter.
    /// </summary>
    public class ParameterSettingRequestDto : InteractionRequestDto
    {
        /// <summary>
        /// The parameter with the requested value.
        /// </summary>
        public AbstractParameterDto parameter;

        protected override uint GetOperationId() { return UMI3DOperationKeys.ParameterSettingRequest; }

        public override (int, Func<byte[], int, int, (int, int)>) ToByteArray(int baseSize, params object[] parameters)
        {
            var fb = base.ToByteArray(baseSize,parameters);

            int size = UMI3DNetworkingHelper.GetSize(parameter) + fb.Item1;
            Func<byte[], int, int, (int, int)> func = (b, i, bs) =>
            {
                (i, bs) = fb.Item2(b, i,bs);
                bs += UMI3DNetworkingHelper.Write(parameter, b, ref i);
                return (i,bs);
            };
            return (size, func);
        }
    }
}
 