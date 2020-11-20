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
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.edk.collaboration
{

    /// <summary>
    /// Add this class to launch an environment on start.
    /// Handle batchmode.
    /// </summary>
    public class UMI3DLauncher : MonoBehaviour
    {
        public const string Separator = "-";
        /// <summary>
        /// Set the name of the environment.
        /// </summary>
        public const string nameParam = Separator + "name";
        /// <summary>
        /// Set the public ip of the server.
        /// </summary>
        public const string ipParam = Separator + "ip";
        /// <summary>
        /// Set the Authentification type. 
        /// <see cref="common.AuthenticationType"/>
        /// </summary>
        public const string authParam = Separator + "auth";
        /// <summary>
        /// Set the token life span.
        /// </summary>
        public const string tokenLifeParam = Separator + "tokenlife";
        /// <summary>
        /// Set the http port.
        /// </summary>
        public const string httpPortParam = Separator + "httpPort";
        /// <summary>
        /// Set the websocket port.
        /// </summary>
        public const string wsPortParam = Separator + "wsPort";
        /// <summary>
        /// Set the fake reliable rtc port.
        /// </summary>
        public const string fakeReliablePortParam = Separator + "fakertcreliableport";
        /// <summary>
        /// Set the fake unreliable rtc port.
        /// </summary>
        public const string fakeUnreliablePortParam = Separator + "fakertcunreliableport";
        /// <summary>
        /// Set the ice servers.
        /// </summary>
        public const string iceParam = Separator + "iceconfig";

        /// <summary>
        /// Should the server be launch at start.
        /// if set to true, the server will be lauch about 3 second after the <see cref="Start"/> is called.
        /// </summary>
        public bool LaunchServerOnStart = false;

        /// <summary>
        /// method called when param <see cref="nameParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetName(string arg) { UMI3DCollaborationEnvironment.Instance.environmentName = arg; }

        /// <summary>
        /// method called when param <see cref="ipParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetIp(string arg) { UMI3DCollaborationServer.Instance.SetIP(arg); }

        /// <summary>
        /// method called when param <see cref="authParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetAuth(string arg)
        {
            common.AuthenticationType result;
            if (Enum.TryParse(arg, out result))
                UMI3DCollaborationServer.Instance.Authentication = result;
        }

        /// <summary>
        /// method called when param <see cref="tokenParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetTokenLife(string arg) {
            float result;
            if (float.TryParse(arg, out result))
                UMI3DCollaborationServer.Instance.tokenLifeTime = result;
        }

        /// <summary>
        /// method called when param <see cref="httpPortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetHttpPort(string arg) {
            int result;
            if (int.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.useRandomHttpPort = result == 0;
                UMI3DCollaborationServer.Instance.httpPort = result;
            }
            else 
                UMI3DCollaborationServer.Instance.useRandomHttpPort = true;
        }

        /// <summary>
        /// method called when param <see cref="wsPortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetWsPort(string arg) {
            int result;
            if (int.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.useRandomWebsocketPort = result == 0;
                UMI3DCollaborationServer.Instance.websocketPort = result;
            }
            else
                UMI3DCollaborationServer.Instance.useRandomWebsocketPort = true;
        }

        /// <summary>
        /// method called when param <see cref="fakeReliablePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetFakeReliable(string arg) {
            int result;
            if (int.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.useRandomFakeRTCReliablePort = result == 0;
                UMI3DCollaborationServer.Instance.fakeRTCReliablePort = result;
            }
            else
                UMI3DCollaborationServer.Instance.useRandomFakeRTCReliablePort = true;
        }

        /// <summary>
        /// method called when param <see cref="fakeUnreliablePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetFakeUnreliable(string arg) {
            int result;
            if (int.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.useRandomFakeRTCUnreliablePort = result == 0;
                UMI3DCollaborationServer.Instance.fakeRTCUnreliablePort = result;
            }
            else
                UMI3DCollaborationServer.Instance.useRandomFakeRTCUnreliablePort = true;
        }

        /// <summary>
        /// method called when param <see cref="iceParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetIceServer(string arg) {
            try
            {
                string file = File.ReadAllText(arg);
                var serv = JsonConvert.DeserializeObject<IceServer[]>(file, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None
                });
                var ice = ScriptableObject.CreateInstance<IceServers>();
                ice.iceServers = serv;
                UMI3DCollaborationServer.Instance.iceServers = ice;
            }
            catch (Exception e)
            {
                Debug.Log("Error on reading Ice Server file "+e);
            }
        }

        /// <summary>
        /// method called if a parameter wasn't catch.
        /// </summary>
        /// <param name="i">current index. 
        /// It should be set to last argument used.
        /// 
        /// </param>
        /// <param name="args"></param>
        protected virtual void OtherParam(ref int i, string[] args) { }

        IEnumerator _launchServer()
        {
            yield return new WaitForSeconds(3f);
            if (LaunchServerOnStart)
                LaunchServer();
        }

        /// <summary>
        /// Launch the server.
        /// Called automaticaly, 3f second after <see cref="Start"/> is called, if <see cref="LaunchServerOnStart"/> is true.
        /// </summary>
        public virtual void LaunchServer() { UMI3DCollaborationServer.Instance.Init(); }


        // Start is called before the first frame update
        protected virtual void Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int length = args.Length;
            for (int i = 0; i < length; i++)
            {
                if (args[i].CompareTo(nameParam) == 0)
                {
                    if (++i < length)
                        SetName(args[i]);
                }
                else if (args[i].CompareTo(ipParam) == 0)
                {
                    if (++i < length)
                        SetIp(args[i]);
                }
                else if (args[i].CompareTo(authParam) == 0)
                {
                    if (++i < length)
                        SetAuth(args[i]);
                }
                else if (args[i].CompareTo(tokenLifeParam) == 0)
                {
                    if (++i < length)
                        SetTokenLife(args[i]);
                }
                else if (args[i].CompareTo(httpPortParam) == 0)
                {
                    if (++i < length)
                        SetHttpPort(args[i]);
                }
                else if (args[i].CompareTo(wsPortParam) == 0)
                {
                    if (++i < length)
                        SetWsPort(args[i]);
                }
                else if (args[i].CompareTo(fakeReliablePortParam) == 0)
                {
                    if (++i < length)
                        SetFakeReliable(args[i]);
                }
                else if (args[i].CompareTo(fakeUnreliablePortParam) == 0)
                {
                    if (++i < length)
                        SetFakeUnreliable(args[i]);
                }
                else if (args[i].CompareTo(iceParam) == 0)
                {
                    if (++i < length)
                        SetIceServer(args[i]);
                }
                else
                    OtherParam(ref i, args);

                StartCoroutine(_launchServer());
            }
        }
    }
}