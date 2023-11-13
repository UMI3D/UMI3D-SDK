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
using umi3d.common.collaboration.dto.signaling;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Environment API to handle HTTP requests comming from a distant world controller.
    /// </summary>
    public class UMI3DEnvironmentFromWorldControllerApi : UMI3DAbstractEnvironmentApi
    {

        /// <summary>
        /// POST "/register"
        /// Register a user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpPost(UMI3DNetworkingKeys.register, WebServiceMethodAttribute.Security.Public, WebServiceMethodAttribute.Type.Method)]
        public async void Register(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DLogger.Log($"Register", scope);
            HttpListenerResponse res = e.Response;

            byte[] bytes = ReadObject(e.Request);

            RegisterIdentityDto dto;

            try
            {
                dto = UMI3DDtoSerializer.FromBson(bytes) as RegisterIdentityDto;
            }
            catch
            {
                dto = UMI3DDtoSerializer.FromJson<RegisterIdentityDto>(System.Text.Encoding.UTF8.GetString(bytes));
            }

            await UMI3DCollaborationServer.Instance.Register(dto,true);
        }
    }
}