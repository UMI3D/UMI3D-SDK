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
using System;
using System.Collections;
using System.IO;
using UnityEngine;

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
        public const string httpPortParam = Separator + "httpport";
        /// <summary>
        /// Set the websocket port.
        /// </summary>
        public const string forgePortParam = Separator + "udpport";
        /// <summary>
        /// Set the public ip of the master server.
        /// </summary>
        public const string masterIpParam = Separator + "masterip";
        /// <summary>
        /// Set the port of the master server.
        /// </summary>
        public const string masterPortParam = Separator + "masterport";
        /// <summary>
        /// Set the public ip of the master server.
        /// </summary>
        public const string natIpParam = Separator + "natip";
        /// <summary>
        /// Set the port of the master server.
        /// </summary>
        public const string natPortParam = Separator + "natport";
        /// <summary>
        /// Set the max number of player.
        /// </summary>
        public const string maxNbPlayerParam = Separator + "players";
        /// <summary>
        /// Set the id of the session.
        /// </summary>
        public const string sessionIdParam = Separator + "sessionId";
        /// <summary>
        /// Set the comment of the session.
        /// </summary>
        public const string sessionCommentParam = Separator + "sessioncomment";
        /// <summary>
        /// Set the icon url of the server.
        /// </summary>
        public const string iconParam = Separator + "iconurl";
        /// <summary>
        /// Set the config file path.
        /// </summary>
        public const string configFileParam = Separator + "config";

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
        /// method called when param <see cref="sessionId"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetSessionId(string arg) { UMI3DCollaborationServer.Instance.sessionId = arg; }

        /// <summary>
        /// method called when param <see cref="sessioncomment"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetSessionComment(string arg) { UMI3DCollaborationServer.Instance.descriptionComment = arg; }

        /// <summary>
        /// method called when param <see cref="sessionId"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetIconServerUrl(string arg) { UMI3DCollaborationServer.Instance.iconServerUrl = arg; }

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
        protected virtual void SetTokenLife(string arg)
        {
            float result;
            if (float.TryParse(arg, out result))
                UMI3DCollaborationServer.Instance.tokenLifeTime = result;
        }

        /// <summary>
        /// method called when param <see cref="httpPortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetHttpPort(string arg)
        {
            ushort result;
            if (ushort.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.useRandomHttpPort = result == 0;
                UMI3DCollaborationServer.Instance.httpPort = result;
            }
            else
                UMI3DCollaborationServer.Instance.useRandomHttpPort = true;
        }

        /// <summary>
        /// method called when param <see cref="forgePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetUdpPort(string arg)
        {
            ushort result;
            if (ushort.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.useRandomForgePort = result == 0;
                UMI3DCollaborationServer.Instance.forgePort = result;
            }
            else
                UMI3DCollaborationServer.Instance.useRandomForgePort = true;
        }

        /// <summary>
        /// method called when param <see cref="masterIpParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetMasterServerIp(string arg) { UMI3DCollaborationServer.Instance.forgeMasterServerHost = arg; }

        /// <summary>
        /// method called when param <see cref="forgePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetMasterServerPort(string arg)
        {
            ushort result;
            if (ushort.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.forgeMasterServerPort = result;
            }
        }

        /// <summary>
        /// method called when param <see cref="natIpParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetNatServerIp(string arg) { UMI3DCollaborationServer.Instance.forgeNatServerHost = arg; }

        /// <summary>
        /// method called when param <see cref="forgePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetNatServerPort(string arg)
        {
            ushort result;
            if (ushort.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.forgeNatServerPort = result;
            }
        }

        /// <summary>
        /// method called when param <see cref="maxNbPlayerParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetMaxNbPlayers(string arg)
        {
            int result;
            if (int.TryParse(arg, out result))
            {
                UMI3DCollaborationServer.Instance.forgeMaxNbPlayer = result;
            }
        }

        protected virtual ConfigServer ReadConfigFile(string arg)
        {
            if (File.Exists(arg))
            {
                return ConfigServer.ReadXml(arg);
            }
            else
                return null;
        }

        /// <summary>
        /// method called when param <see cref="configFileParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void ApplyConfigFile(ConfigServer conf)
        {
            if (conf != null)
            {
                if (!string.IsNullOrEmpty(conf.nameParam))
                    SetName(conf.nameParam);

                if (!string.IsNullOrEmpty(conf.ipParam))
                    SetIp(conf.ipParam);

                if (!string.IsNullOrEmpty(conf.authParam))
                    SetAuth(conf.authParam);

                if (conf.tokenLifeParam > 0)
                    UMI3DCollaborationServer.Instance.tokenLifeTime = conf.tokenLifeParam;

                UMI3DCollaborationServer.Instance.useRandomHttpPort = conf.httpPortParam == 0;
                UMI3DCollaborationServer.Instance.httpPort = conf.httpPortParam;

                UMI3DCollaborationServer.Instance.useRandomForgePort = conf.udpportParam == 0;
                UMI3DCollaborationServer.Instance.forgePort = conf.udpportParam;

                if (!string.IsNullOrEmpty(conf.masterIpParam))
                    SetMasterServerIp(conf.masterIpParam);

                UMI3DCollaborationServer.Instance.forgeMasterServerPort = conf.masterPortParam;

                if (!string.IsNullOrEmpty(conf.natIpParam))
                    SetNatServerIp(conf.natIpParam);

                UMI3DCollaborationServer.Instance.forgeNatServerPort = conf.natPortParam;

                UMI3DCollaborationServer.Instance.forgeMaxNbPlayer = conf.playersParam;

                if (!string.IsNullOrEmpty(conf.sessionIdParam))
                    SetSessionId(conf.sessionIdParam);

                if (!string.IsNullOrEmpty(conf.sessionCommentParam))
                    SetSessionComment(conf.sessionCommentParam);

                if (!string.IsNullOrEmpty(conf.iconUrlParam))
                    SetIconServerUrl(conf.iconUrlParam);
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
            //Apply first config fileif it exists
            for (int i = 0; i < length; i++)
            {
                if (args[i].CompareTo(configFileParam) == 0)
                {
                    if (++i < length)
                        ApplyConfigFile(ReadConfigFile(args[i]));
                }
            }

            //then aplly other arguments
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
                else if (args[i].CompareTo(forgePortParam) == 0)
                {
                    if (++i < length)
                        SetUdpPort(args[i]);
                }
                else if (args[i].CompareTo(masterIpParam) == 0)
                {
                    if (++i < length)
                        SetMasterServerIp(args[i]);
                }
                else if (args[i].CompareTo(masterPortParam) == 0)
                {
                    if (++i < length)
                        SetMasterServerPort(args[i]);
                }
                else if (args[i].CompareTo(natIpParam) == 0)
                {
                    if (++i < length)
                        SetNatServerIp(args[i]);
                }
                else if (args[i].CompareTo(natPortParam) == 0)
                {
                    if (++i < length)
                        SetNatServerPort(args[i]);
                }
                else if (args[i].CompareTo(maxNbPlayerParam) == 0)
                {
                    if (++i < length)
                        SetMaxNbPlayers(args[i]);
                }
                else if (args[i].CompareTo(sessionIdParam) == 0)
                {
                    if (++i < length)
                        SetSessionId(args[i]);
                }
                else if (args[i].CompareTo(sessionCommentParam) == 0)
                {
                    if (++i < length)
                        SetSessionComment(args[i]);
                }
                else if (args[i].CompareTo(iconParam) == 0)
                {
                    if (++i < length)
                        SetIconServerUrl(args[i]);
                }
                else
                    OtherParam(ref i, args);
            }
            StartCoroutine(_launchServer());
        }
    }
}