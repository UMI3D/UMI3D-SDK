/*
Copyright 2019 - 2024 Inetum

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
using umi3d.common.collaboration;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Send HTTP requests to the environment server.
    /// </summary>
    /// Usually used before connection or to retrieve DTOs.
    public class WorldHttpClient : AbstractHttpClient<UMI3DWorldControllerClient>
    {
        public WorldHttpClient(UMI3DWorldControllerClient environmentClient) : base(environmentClient)
        {
        }

        protected override string httpUrl => throw new NotImplementedException();


        internal string HeaderToken
        {
            get => _HeaderToken;

            set { _HeaderToken = value; }
        }

        public override string SendGetPrivate(string url)
        {
            if (UMI3DResourcesManager.HasUrlGotParameters(url))
                url += "&" + UMI3DNetworkingKeys.ResourceServerAuthorization + "=" + UMI3DNetworkingKeys.bearer + _HeaderToken;
            else
                url += "?" + UMI3DNetworkingKeys.ResourceServerAuthorization + "=" + UMI3DNetworkingKeys.bearer + _HeaderToken;
            return url;
        }

        protected override async Task<UnityWebRequest> Sub__GetRequest(UnityWebRequest www, DateTime date, string HeaderToken, string url, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            if (UMI3DClientServer.Exists && await UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, tryCount, date, ShouldTryAgain)))
                return await _GetRequest(this, HeaderToken, url, ShouldTryAgain, UseCredential, headers, tryCount + 1);
            else
                throw new Umi3dNetworkingException(www, "Failed to get ");
        }
        protected override async Task<UnityWebRequest> Sub_PostRequest(UnityWebRequest www, DateTime date, string HeaderToken, string url, string contentType, byte[] bytes, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            if (UMI3DClientServer.Exists && await UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, tryCount, date, ShouldTryAgain)))
                return await _PostRequest(this, HeaderToken, url, contentType, bytes, ShouldTryAgain, UseCredential, headers, tryCount + 1);
            else
            {
                UnityEngine.Debug.Log(System.Text.Encoding.ASCII.GetString(bytes));
                throw new Umi3dNetworkingException(www, " Failed to post\n" + www.downloadHandler.text);
            }
        }
    }
}