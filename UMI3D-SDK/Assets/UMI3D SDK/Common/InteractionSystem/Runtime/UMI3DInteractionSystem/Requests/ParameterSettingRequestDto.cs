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
        public object parameter;

        protected override uint GetOperationId() { return UMI3DOperationKeys.ParameterSettingRequest; }

        public override Bytable ToBytableArray(params object[] parameters)
        {
            return base.ToBytableArray(parameters) + UMI3DNetworkingHelper.Write(parameter);
        }
    }
}
