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
using umi3d.common;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class UMI3DWebsocket
    {
        HttpServer wssv;

        public UMI3DWebsocket()
        {
            wssv = new HttpServer(UMI3DCollaborationServer.Instance.websocketPort);

            // Add the WebSocket services
            wssv.AddWebSocketService<UMI3DWebSocketConnection>(
                UMI3DNetworkingKeys.websocket,
                () =>
                    new UMI3DWebSocketConnection()
                    {
                        IgnoreExtensions = true,
                        Protocol = UMI3DNetworkingKeys.websocketProtocol,
                    }
            );
            wssv.AuthenticationSchemes = UMI3DCollaborationServer.GetAuthentication().Convert();
            wssv.Realm = "UMI3D";
            wssv.UserCredentialsFinder = id =>
            {
                var name = id.Name;
                return UMI3DCollaborationServer.Instance.Identifier.GetPasswordFor(name);
            };

            wssv.Start();
            if (wssv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", wssv.Port);
                foreach (var path in wssv.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }
        }

        /// <summary>
        /// Stop the websocket
        /// </summary>
        public void Stop()
        {
            if (wssv != null)
                wssv.Stop();
        }

    }
}