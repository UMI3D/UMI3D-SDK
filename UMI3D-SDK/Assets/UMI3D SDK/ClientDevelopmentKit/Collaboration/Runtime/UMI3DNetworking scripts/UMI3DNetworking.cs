/*
Copyright 2019 - 2023 Inetum

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

using BeardedManStudios.Forge.Networking;
using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using umi3d.debug;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Allow the browser to communicate with a distant server.
    /// 
    /// <para>
    /// With this class the user can:
    /// <list type="bullet">
    /// <item>Send Web request (WR).</item>
    /// <item>Try to connect to a Master Server (LoMS).</item>
    /// <item>Try to connect to a World Controller (LoWC).</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class UMI3DNetworking
    {
        //static UMI3DNetworking defaultUMI3DNetworking = new UMI3DNetworking();
        //static Dictionary<string, UMI3DNetworking> UMI3DNetworkings = new();

        ///// <summary>
        ///// Get the default <see cref="UMI3DNetworking"/>.
        ///// </summary>
        //public static UMI3DNetworking GetDefault
        //{
        //    get
        //    {
        //        return defaultUMI3DNetworking;
        //    }
        //}

        ///// <summary>
        ///// Try to get a <see cref="UMI3DNetworking"/> corresponding to that <paramref name="url"/>.
        ///// </summary>
        ///// <param name="url"></param>
        ///// <param name="networking"></param>
        ///// <returns></returns>
        //public static bool TryGet(string url, out UMI3DNetworking networking)
        //{
        //    if (UMI3DNetworkings.ContainsKey(url))
        //    {
        //        networking = UMI3DNetworkings[url];
        //        return true;
        //    }
        //    else
        //    {
        //        networking = null;
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Create a <see cref="UMI3DNetworking"/> for the url: <paramref name="url"/>.
        ///// 
        ///// <para>
        ///// If this creation override a previous <see cref="UMI3DNetworking"/> <paramref name="hasBeenOverridden"/> will be set to true else false.
        ///// </para>
        ///// </summary>
        ///// <param name="url"></param>
        ///// <param name="hasBeenOverridden"></param>
        ///// <returns></returns>
        //public static UMI3DNetworking Create(string url, out bool hasBeenOverridden)
        //{
        //    var result = new UMI3DNetworking();
        //    if (UMI3DNetworkings.ContainsKey(url))
        //    {
        //        hasBeenOverridden = true;
        //        UMI3DNetworkings[url] = result;
        //        return result;
        //    }
        //    else
        //    {
        //        hasBeenOverridden = false;
        //        UMI3DNetworkings.Add(url, result);
        //        return result;
        //    }
        //}

        static CyclingEnumerator<UMI3DNetworking> networkings = new(new[] { new UMI3DNetworking() });
        public static ICyclingEnumerator<UMI3DNetworking> Networkings
        {
            get
            {
                return networkings;
            }
        }

        UMI3DNetworking()
        {
            webRequest = defaultWebRequest;
            masterServerConnection = defaultMasterServer;
            masterServerDisconnection = defaultMasterServer; 
        }

        #region WebRequest

        /// <summary>
        /// The web request interface to send or receive web requests.
        /// </summary>
        public IUMI3DWebRequest webRequest
        {
            get
            {
                return currentWebRequest;
            }
            set
            {
                currentWebRequest = value ?? defaultWebRequest;
            }
        }
        IUMI3DWebRequest defaultWebRequest = new UMI3DWebRequest();
        IUMI3DWebRequest currentWebRequest;

        #endregion

        #region LauncherOnMasterServer

        /// <summary>
        /// The master server connection interface implementation.
        /// </summary>
        public IUMI3DMasterServerConnection masterServerConnection
        {
            get
            {
                return currentMasterServerConnection;
            }
            set
            {
                currentMasterServerConnection = value ?? defaultMasterServer;
            }
        }
        /// <summary>
        /// The master server information interface implementation.
        /// </summary>
        public IUMI3DMasterServerInformation masterServerInformation
        {
            get
            {
                return currentMasterServerInformation;
            }
            set
            {
                currentMasterServerInformation = value ?? defaultMasterServer;
            }
        }
        /// <summary>
        /// The master server disconnection interface implementation.
        /// </summary>
        public IUMI3DMasterServerDisconnection masterServerDisconnection
        {
            get
            {
                return currentMasterServerDisconnection;
            }
            set
            {
                currentMasterServerDisconnection = value ?? defaultMasterServer;
            }
        }
        LauncherOnMasterServer defaultMasterServer = new LauncherOnMasterServer();
        IUMI3DMasterServerConnection currentMasterServerConnection;
        IUMI3DMasterServerInformation currentMasterServerInformation;
        IUMI3DMasterServerDisconnection currentMasterServerDisconnection;

        #endregion

        #region LauncherOnWorldController

        /// <summary>
        /// The world controller connection interface implementation.
        /// </summary>
        public IUMI3DWorldControllerConnection worldControllerConnection
        {
            get
            {
                return currentWorldControllerConnection;
            }
            set
            {
                currentWorldControllerConnection = value ?? defaultWorldController;
            }
        }
        /// <summary>
        /// The world controller information interface implementation.
        /// </summary>
        public IUMI3DWorldControllerInformation worldControllerInformation
        {
            get
            {
                return currentWorldControllerInformation;
            }
            set
            {
                currentWorldControllerInformation = value ?? defaultWorldController;
            }
        }
        LauncherOnWorldController defaultWorldController = new LauncherOnWorldController();
        IUMI3DWorldControllerConnection currentWorldControllerConnection;
        IUMI3DWorldControllerInformation currentWorldControllerInformation;

        #endregion
    }
}