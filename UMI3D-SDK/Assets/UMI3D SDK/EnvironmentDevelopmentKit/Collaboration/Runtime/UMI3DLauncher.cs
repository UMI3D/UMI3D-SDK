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
using umi3d.common;
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
        /// Set the public IP address of the server.
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
        /// Set the HTTP port.
        /// </summary>
        public const string httpPortParam = Separator + "httpport";

        /// <summary>
        /// Set the default ressource path.
        /// </summary>
        public const string resourcesDefaultUrlParam = Separator + "resourcesDefaultUrlParam";

        /// <summary>
        /// Set the websocket port.
        /// </summary>
        public const string forgePortParam = Separator + "udpport";
        /// <summary>
        /// Set the public IP address of the master server.
        /// </summary>
        public const string masterIpParam = Separator + "masterip";
        /// <summary>
        /// Set the port of the master server.
        /// </summary>
        public const string masterPortParam = Separator + "masterport";
        /// <summary>
        /// Set the public IP address of the NAT server.
        /// </summary>
        public const string natIpParam = Separator + "natip";
        /// <summary>
        /// Set the port of the NAT server.
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
        /// Set the log scope.
        /// </summary>
        public const string loggingScopeParam = Separator + "logscope";
        /// <summary>
        /// Set the log level.
        /// </summary>
        public const string loggingLevelParam = Separator + "loglevel";
        /// <summary>
        /// Set the loginfo file output path.
        /// </summary>
        public const string loginfoOutputPathParam = Separator + "infofilepath";
        /// <summary>
        /// Set the loginfo frequency.
        /// </summary>
        public const string loginfoFrequencyParam = Separator + "infofrequency";
        /// <summary>
        /// Set the log file output path.
        /// </summary>
        public const string logOutputPathParam = Separator + "logfilepath";
        /// <summary>
        /// Set the murmur server.
        /// </summary>
        public const string murmurServerParam = Separator + "murmur";

        /// <summary>
        /// Set the Murmur server.
        /// </summary>
        public const string httpMurmurServerParam = Separator + "murmurHttp";


        public const string generateconfigFileParam = Separator + "createconfig";
        /// <summary>
        /// Set the config file path.
        /// </summary>
        public const string configFileParam = Separator + "config";


        /// <summary>
        /// Should the server be launched at start?
        /// </summary>
        /// If set to true, the server will be launched about 3 second after the <see cref="Start"/> is called.
        [Tooltip("Should the server be launched when this component is started?")]
        public bool LaunchServerOnStart = false;

        /// <summary>
        /// Called when param <see cref="nameParam"/> is found.
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetName(string arg) { UMI3DCollaborationEnvironment.Instance.environmentName = arg; }

        /// <summary>
        /// Called when param <see cref="sessionId"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetSessionId(string arg) { UMI3DCollaborationServer.Instance.sessionId = arg; }

        /// <summary>
        /// Called when param <see cref="sessioncomment"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetSessionComment(string arg) { UMI3DCollaborationServer.Instance.descriptionComment = arg; }

        /// <summary>
        /// Called when param <see cref="iconUrlParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetIconServerUrl(string arg) { UMI3DCollaborationServer.Instance.iconServerUrl = arg; }

        /// <summary>
        /// Called when param <see cref="ipParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetIp(string arg) { UMI3DCollaborationServer.Instance.SetIP(arg); }

        /// <summary>
        /// Called when param <see cref="tokenParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetTokenLife(string arg)
        {
            if (float.TryParse(arg, out float result))
                UMI3DCollaborationServer.Instance.tokenLifeTime = result;
        }

        /// <summary>
        /// Called when param <see cref="httpPortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetHttpPort(string arg)
        {
            if (ushort.TryParse(arg, out ushort result))
            {
                UMI3DCollaborationServer.Instance.useRandomHttpPort = result == 0;
                UMI3DCollaborationServer.Instance.httpPort = result;
            }
            else
            {
                UMI3DCollaborationServer.Instance.useRandomHttpPort = true;
            }
        }

        /// <summary>
        /// Called when param <see cref="resourcesDefaultUrlParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetResourcesDefaultUrl(string arg)
        {
            UMI3DCollaborationServer.Instance.resourcesUrl = arg;
        }

        /// <summary>
        /// Called when param <see cref="forgePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetUdpPort(string arg)
        {
            if (ushort.TryParse(arg, out ushort result))
            {
                UMI3DCollaborationServer.Instance.useRandomForgePort = result == 0;
                UMI3DCollaborationServer.Instance.forgePort = result;
            }
            else
            {
                UMI3DCollaborationServer.Instance.useRandomForgePort = true;
            }
        }

        /// <summary>
        /// Called when param <see cref="masterIpParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetMasterServerIp(string arg) { UMI3DCollaborationServer.Instance.forgeMasterServerHost = arg; }

        /// <summary>
        /// Called when param <see cref="forgePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetMasterServerPort(string arg)
        {
            if (ushort.TryParse(arg, out ushort result))
            {
                UMI3DCollaborationServer.Instance.forgeMasterServerPort = result;
            }
        }

        /// <summary>
        /// Called when param <see cref="natIpParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetNatServerIp(string arg) { UMI3DCollaborationServer.Instance.forgeNatServerHost = arg; }

        /// <summary>
        /// Called when param <see cref="forgePortParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetNatServerPort(string arg)
        {
            if (ushort.TryParse(arg, out ushort result))
            {
                UMI3DCollaborationServer.Instance.forgeNatServerPort = result;
            }
        }

        /// <summary>
        /// Called when param <see cref="maxNbPlayerParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetMaxNbPlayers(string arg)
        {
            if (int.TryParse(arg, out int result))
            {
                UMI3DCollaborationServer.Instance.forgeMaxNbPlayer = result;
            }
        }

        /// <summary>
        /// Called when param <see cref="loggingScopeParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetLogScope(string arg)
        {
            string[] split = arg.Split('|');
            DebugScope Result = 0;
            foreach (string s in split)
            {
                if (DebugScope.TryParse(s, out DebugScope result))
                {
                    Result |= result;
                }
            }
            UMI3DLogger.LogScope = Result;
        }

        /// <summary>
        /// Called when param <see cref="loggingLevelParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetLogLevel(string arg)
        {
            if (DebugLevel.TryParse(arg, out DebugLevel result))
            {
                UMI3DLogger.LogLevel = result;
            }
        }

        /// <summary>
        /// Called when param <see cref="loginfoOutputPathParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetloginfoOutputPathParam(string arg)
        {
            UMI3DLogger.ShouldLogInfo = true;
            UMI3DLogger.LogInfoPath = arg;
        }

        /// <summary>
        /// Called when param <see cref="loginfoFrequencyParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetLoginfoFrequencyParam(string arg)
        {
            if (float.TryParse(arg, out float result))
            {
                UMI3DLogger.LogInfoDelta = result;
            }
        }


        protected virtual void SetMurmurServer(string arg)
        {
            UMI3DCollaborationServer.Instance.mumbleIp = arg;
        }

        protected virtual void SetHttpMurmurServer(string arg)
        {
            UMI3DCollaborationServer.Instance.mumbleHttpIp = arg;
        }

        /// <summary>
        /// Called when param <see cref="loginfoOutputPathParam"/> is found
        /// </summary>
        /// <param arg="arg">argument after parameter</param>
        protected virtual void SetlogOutputPathParam(string arg)
        {
            UMI3DLogger.ShouldLog = true;
            UMI3DLogger.LogPath = arg;
        }


        protected virtual ConfigServer ReadConfigFile(string arg)
        {
            if (File.Exists(arg))
            {
                return ConfigServer.ReadXml(arg);
            }
            else
            {
                return null;
            }
        }

        protected virtual void GenerateConfigFile(string arg)
        {
            if (File.Exists(arg))
            {
                File.Delete(arg);
            }
            ConfigServer.WriteXml(new ConfigServer(), arg);
        }

        /// <summary>
        /// Called when param <see cref="configFileParam"/> is found
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

                if (!string.IsNullOrEmpty(conf.resourcesDefaultUrlParam))
                    SetResourcesDefaultUrl(conf.resourcesDefaultUrlParam);

                if (conf.tokenLifeParam > 0)
                    SetTokenLife(conf.tokenLifeParam.ToString());

                SetHttpPort(conf.httpPortParam.ToString());

                SetUdpPort(conf.udpportParam.ToString());

                if (!string.IsNullOrEmpty(conf.masterIpParam))
                    SetMasterServerIp(conf.masterIpParam);

                SetMasterServerPort(conf.masterPortParam.ToString());

                if (!string.IsNullOrEmpty(conf.natIpParam))
                    SetNatServerIp(conf.natIpParam);

                SetNatServerPort(conf.natPortParam.ToString());

                SetMaxNbPlayers(conf.natPortParam.ToString());

                if (!string.IsNullOrEmpty(conf.sessionIdParam))
                    SetSessionId(conf.sessionIdParam);

                if (!string.IsNullOrEmpty(conf.sessionCommentParam))
                    SetSessionComment(conf.sessionCommentParam);

                if (!string.IsNullOrEmpty(conf.iconUrlParam))
                    SetIconServerUrl(conf.iconUrlParam);

                if (!string.IsNullOrEmpty(conf.loggingLevelParam))
                    SetLogLevel(conf.loggingLevelParam);

                if (!string.IsNullOrEmpty(conf.loggingScopeParam))
                    SetLogScope(conf.loggingScopeParam);

                if (conf.loginfoFrequencyParam > 0)
                    SetLoginfoFrequencyParam(conf.loginfoFrequencyParam.ToString());

                if (!string.IsNullOrEmpty(conf.loginfoOutputPathParam))
                    SetloginfoOutputPathParam(conf.loginfoOutputPathParam);

                if (!string.IsNullOrEmpty(conf.logOutputPathParam))
                    SetlogOutputPathParam(conf.logOutputPathParam);
            }
        }

        /// <summary>
        /// Called if a parameter wasn't catch.
        /// </summary>
        /// <param name="i">current index. 
        /// It should be set to last argument used.
        /// </param>
        /// <param name="args"></param>
        protected virtual void OtherParam(ref int i, string[] args) { }

        private IEnumerator _launchServer()
        {
            yield return new WaitForSeconds(3f);
            if (LaunchServerOnStart)
                LaunchServer();
        }

        /// <summary>
        /// Launch the server.
        /// </summary>
        /// Called automaticaly, 3 seconds after <see cref="Start"/> is called, if <see cref="LaunchServerOnStart"/> is true.
        public virtual void LaunchServer() { UMI3DCollaborationServer.Instance.Init(); }


        protected virtual void Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int length = args.Length;
            //Apply first config fileif it exists
            for (int i = 0; i < length; i++)
            {
                if (args[i].Equals(configFileParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        ApplyConfigFile(ReadConfigFile(args[i]));
                    break;
                }
                else if (args[i].Equals(generateconfigFileParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        GenerateConfigFile(args[i]);
                    Application.Quit();
                }
            }

            //then aplly other arguments
            for (int i = 0; i < length; i++)
            {
                if (args[i].Equals(nameParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetName(args[i]);
                }
                else if (args[i].Equals(ipParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetIp(args[i]);
                }
                else if (args[i].Equals(resourcesDefaultUrlParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetResourcesDefaultUrl(args[i]);
                }
                else if (args[i].Equals(tokenLifeParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetTokenLife(args[i]);
                }
                else if (args[i].Equals(httpPortParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetHttpPort(args[i]);
                }
                else if (args[i].Equals(forgePortParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetUdpPort(args[i]);
                }
                else if (args[i].Equals(masterIpParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetMasterServerIp(args[i]);
                }
                else if (args[i].Equals(masterPortParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetMasterServerPort(args[i]);
                }
                else if (args[i].Equals(natIpParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetNatServerIp(args[i]);
                }
                else if (args[i].Equals(natPortParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetNatServerPort(args[i]);
                }
                else if (args[i].Equals(maxNbPlayerParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetMaxNbPlayers(args[i]);
                }
                else if (args[i].Equals(sessionIdParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetSessionId(args[i]);
                }
                else if (args[i].Equals(sessionCommentParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetSessionComment(args[i]);
                }
                else if (args[i].Equals(iconParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetIconServerUrl(args[i]);
                }
                else if (args[i].Equals(loggingLevelParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetLogLevel(args[i]);
                }
                else if (args[i].Equals(loggingScopeParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetLogScope(args[i]);
                }
                else if (args[i].Equals(loginfoFrequencyParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetLoginfoFrequencyParam(args[i]);
                }
                else if (args[i].Equals(loginfoOutputPathParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetloginfoOutputPathParam(args[i]);
                }
                else if (args[i].Equals(murmurServerParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetMurmurServer(args[i]);
                }
                else if (args[i].Equals(httpMurmurServerParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetHttpMurmurServer(args[i]);
                }
                else if (args[i].Equals(logOutputPathParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (++i < length)
                        SetlogOutputPathParam(args[i]);
                }
                else
                {
                    OtherParam(ref i, args);
                }
            }
            StartCoroutine(_launchServer());
        }
    }
}