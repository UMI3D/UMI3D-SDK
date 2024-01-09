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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Networking;
using umi3d.common;
using umi3d.common.collaboration;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Send HTTP requests to the environment server.
    /// </summary>
    /// Usually used before connection or to retrieve DTOs.
    public class HttpClient1 : AbstractHttpClient<UMI3DEnvironmentClient1>
    {
        public HttpClient1(UMI3DEnvironmentClient1 environmentClient) : base(environmentClient)
        {
        }

        protected override string httpUrl => environmentClient.connectionDto.httpUrl;

        public async Task<PendingTransactionDto> SendPostRegisterDistantUser(RegisterIdentityDto identity, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            PendingTransactionDto result = null;
            UMI3DLogger.Log($"Send PostRegisterDistantUser", scope | DebugScope.Connection);
            using (UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.resources_server_register, null, identity.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {

            }
            return result;
        }

    }
}