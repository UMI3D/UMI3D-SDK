﻿/*
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

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.collaboration;
using UnityEngine;

namespace umi3d.worldController
{
    public class UPDWorldControllerAPI : WorldControllerAPI
    {
        private UMI3DWorldControllerForgeClient forgeClient;

        private bool disconected;

        public string httpUrl;
        public string host;
        public string ResourcesUrl;
        public string forgeMasterServerHost;
        public string forgeNatServerHost;
        public ushort forgeServerPort;
        public ushort forgeMasterServerPort;
        public ushort forgeNatServerPort;

        public string token = "token";

        public override void Setup()
        {
            base.Setup();

            disconected = false;

            Join();
        }

        void Join()
        {
            forgeClient = UMI3DWorldControllerForgeClient.Create(this);
            forgeClient.ip = host;
            forgeClient.port = forgeServerPort;
            forgeClient.masterServerHost = forgeMasterServerHost;
            forgeClient.masterServerPort = forgeMasterServerPort;
            forgeClient.natServerHost = forgeNatServerHost;
            forgeClient.natServerPort = forgeNatServerPort;

            var Auth = new common.collaboration.UMI3DAuthenticator((a)=>a.Invoke(token));
            Join(Auth);
        }

        async void Join(BeardedManStudios.Forge.Networking.IUserAuthenticator authenticator)
        {
            forgeClient.Join(authenticator);
            await Task.Delay(10000);
            if (!forgeClient.IsConnected && !disconected)
            {
                forgeClient.Stop();
                Join();
            }
        }

        public async void OnMessage(int groupId, byte[] bytes) {
           var b = new ByteContainer(bytes);
            var id = UMI3DNetworkingHelper.Read<uint>(b);
            switch (id)
            {
                case UMI3DWorldControllerMessageKeys.RegisterUser:
                    var identityDto = UMI3DNetworkingHelper.Read<RegisterIdentityDto>(b);
                    await UMI3DCollaborationServer.Instance.Register(identityDto);
                    break;
                default:
                    Debug.LogError($"Missing case {id}");
                    break;
            }
           
        }

        public void ConnectionLost()
        {
            if (!disconected)
            {
                forgeClient.Stop();
                Join();
            }
        }

        public override void Stop()
        {
            disconected = true;
        }


        public override Task NotifyUserJoin(string uid)
        {
            forgeClient.Send((new WorldControllerUserJoinMessage(uid)).ToBytable().ToBytes());
            return Task.CompletedTask;
        }

        public override Task NotifyUserLeave(string uid)
        {
            forgeClient.Send((new WorldControllerUserLeaveMessage(uid)).ToBytable().ToBytes());
            return Task.CompletedTask;
        }
    }
}