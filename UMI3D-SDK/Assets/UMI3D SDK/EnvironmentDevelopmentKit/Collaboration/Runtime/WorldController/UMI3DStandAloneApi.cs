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

using BeardedManStudios.Forge.Networking.Unity;
using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.collaboration;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

public class UMI3DStandAloneApi : IHttpApi
{

    private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;

    [HttpPost(UMI3DNetworkingKeys.connect, WebServiceMethodAttribute.Security.Public, WebServiceMethodAttribute.Type.Method)]
    public void Connection(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
    {
        byte[] bytes = default(byte[]);
        using (var memstream = new MemoryStream())
        {
            byte[] buffer = new byte[512];
            int bytesRead = default(int);
            while ((bytesRead = e.Request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                memstream.Write(buffer, 0, bytesRead);
            bytes = memstream.ToArray();
        }
        var dto = UMI3DDto.FromBson(bytes);
        bool finished = false;
        UMI3DDto result = null;
        MainThreadManager.Run(() => RegisterUser(dto as ConnectionDto, (res) => { finished = true; result = res; }));
        while (!finished)
            System.Threading.Thread.Sleep(1);

        if (result != null)
        {
            Debug.Log($"result {result} {(result as PrivateIdentityDto)?.userId} {(result as PrivateIdentityDto)?.GlobalToken}");
            HttpListenerResponse res = e.Response;
            res.WriteContent(result.ToBson());
        }

    }

    async void RegisterUser(ConnectionDto dto, Action<UMI3DDto> callback)
    {
        UMI3DDto res = await umi3d.worldController.WorldController.RegisterUser(dto);
        callback.Invoke(res);
    }

    /// <summary>
    /// Handles the GET Media Request received by the HTTP Server.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">Represents the event data for the HTTP request event</param>
    [HttpGet(UMI3DNetworkingKeys.renew_connect, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
    public void RenewConnectionRequest(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
    {
        UMI3DLogger.Log($"Get Media", scope);
        HttpListenerResponse res = e.Response;
        byte[] message = null;
        if (umi3d.worldController.WorldController.Exists)
        {
            message = umi3d.worldController.WorldController.Instance.ToDto().ToBson();
        }
        res.WriteContent(message);
    }


    #region media
    /// <summary>
    /// Handles the GET Media Request received by the HTTP Server.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">Represents the event data for the HTTP request event</param>
    [HttpGet(UMI3DNetworkingKeys.media, WebServiceMethodAttribute.Security.Public, WebServiceMethodAttribute.Type.Method)]
    public void MediaRequest(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
    {
        UMI3DLogger.Log($"Get Media", scope);
        HttpListenerResponse res = e.Response;
        byte[] message = null;
        if (umi3d.worldController.WorldController.Exists)
        {
            message = umi3d.worldController.WorldController.Instance.ToDto().ToBson();
        }
        res.WriteContent(message);
    }
    #endregion

    public bool isAuthenticated(HttpListenerRequest request)
    {
        throw new NotImplementedException();
    }
}
