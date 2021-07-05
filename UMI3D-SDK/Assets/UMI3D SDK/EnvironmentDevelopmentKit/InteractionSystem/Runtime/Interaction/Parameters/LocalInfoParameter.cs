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
        public static Dictionary<(UMI3DUser, string), (bool, bool)> userResponses { get; private set; } = new Dictionary<(UMI3DUser, string), (bool, bool)>();

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

        /// <summary>
        /// Event raised when value changes to true.
        /// </summary>
        public InteractionEvent onChangeTrue = new InteractionEvent();

        /// <summary>
        /// Event raised when value changes to false.
        /// </summary>
        public InteractionEvent onChangeFalse = new InteractionEvent();

        private string appName;

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new LocalInfoRequestParameterDto();
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
            LIRPdto.value = (readValue, writeValue);
        }

        ///<inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case ParameterSettingRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is LocalInfoRequestParameterDto parameter)
                    {
                        ChageUserLocalInfo(user, parameter); 
                        
                        Debug.Log("receive new authorisaation : " + key + "   " + parameter.value);
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
        }

        public static void ChageUserLocalInfo(UMI3DUser user, LocalInfoRequestParameterDto parameter)
        {
            if (userResponses.ContainsKey((user, parameter.key)))
            {
                userResponses[(user, parameter.key)] = parameter.value;
            }
            else
            {
                userResponses.Add((user, parameter.key), parameter.value);
            }
        }


        private void Start()
        {
            appName = Application.productName;
        }


    }
}