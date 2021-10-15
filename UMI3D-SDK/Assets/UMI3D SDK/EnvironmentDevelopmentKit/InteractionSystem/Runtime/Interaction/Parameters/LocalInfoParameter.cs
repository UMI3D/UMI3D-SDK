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
using UnityEngine;

namespace umi3d.edk.interaction
{
    public class LocalInfoParameter : AbstractParameter
    {
        public static Dictionary<(UMI3DUser, string), LocalInfoRequestParameterValue> userResponses { get; private set; } = new Dictionary<(UMI3DUser, string), LocalInfoRequestParameterValue>();

        /// <summary>
        /// Current input value for reading authorization.
        /// </summary>
        public bool readValue = false;

        /// <summary>
        /// Current input value for writing authorization.
        /// </summary>
        public bool writeValue = false;

        /// <summary>
        /// The reason of the request info. Should explain what are the use case of datas.
        /// </summary>
        public string reason = "";

        /// <summary>
        /// The server name displayed by clients in the request.
        /// </summary>
        public string serverName = "";

        /// <summary>
        /// The key used by clients to identify the correct data between other datas send by the same app.
        /// </summary>
        public string key = "";

        private string appName;

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new LocalInfoRequestParameterDto();
        }

        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.LocalInfoParameter;
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
            LocalInfoRequestParameterDto LIRPdto = dto as LocalInfoRequestParameterDto;
            LIRPdto.app_id = appName;
            LIRPdto.serverName = serverName;
            LIRPdto.reason = reason;
            LIRPdto.key = key;
            LIRPdto.value = new LocalInfoRequestParameterValue(readValue, writeValue);
        }

        public override Bytable ToByte(UMI3DUser user)
        {
            return base.ToByte(user)
                + UMI3DNetworkingHelper.Write(appName)
                + UMI3DNetworkingHelper.Write(serverName)
                + UMI3DNetworkingHelper.Write(reason)
                + UMI3DNetworkingHelper.Write(key)
                + UMI3DNetworkingHelper.Write(readValue)
                + UMI3DNetworkingHelper.Write(writeValue);
        }

        ///<inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ParameterSettingRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is LocalInfoRequestParameterValue parameter )
                    {
                        ChageUserLocalInfo(user, parameter); 
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
            throw new System.NotImplementedException();
            //change user access authorization isn't supported after connexion.
        }

        public void ChageUserLocalInfo(UMI3DUser user, LocalInfoRequestParameterValue value)
        {
            if (userResponses.ContainsKey((user, key)))
            {
                userResponses[(user, key)] = value;
            }
            else
            {
                userResponses.Add((user, key), value);
            }
        }


        private void Start()
        {
            appName = Application.productName;
        }
    }
}