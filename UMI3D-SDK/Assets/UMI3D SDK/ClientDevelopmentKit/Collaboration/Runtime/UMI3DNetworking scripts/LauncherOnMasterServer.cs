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

using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.SimpleJSON;
using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Used to connect to a Master Server, when a World Controller is not used.
    /// </summary>
    internal class LauncherOnMasterServer : IUMI3DMasterServerConnection, IUMI3DMasterServerDisconnection
    {
        #region Private

        const string requestServerInfoKey = "info";
        const string requestSessionInfoKey = "get";

        static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(LauncherOnMasterServer)}");

        /// <summary>
        /// The Master Server communicates over TCP
        /// </summary>
        TCPMasterClient client;

        Queue<(
                NetworkingPlayer player, 
                Text frame, 
                NetWorker sender
            )> request = new();

        UnityEngine.Coroutine RequestInfoCoroutine;

        /// <summary>
        /// Action raise when a request info has succeeded.
        /// </summary>
        Action<(string serverName, string icon)> requestServerInfSucceeded;
        /// <summary>
        /// Action raise when a request info has succeeded.
        /// </summary>
        Action<MasterServerResponse.Server> requestSessionInfSucceeded;

        void OnReceiveInfo(
            NetworkingPlayer player, 
            Text frame, 
            NetWorker sender
        )
        {
            request.Enqueue((player, frame, sender));
        }

        IEnumerator WaitForInfo()
        {
            if (!client.IsConnected)
            {
                MasterServerException.LogException(
                   $"Client is disconnected when trying to {nameof(WaitForInfo)}",
                   inner: null,
                   MasterServerException.ExceptionTypeEnum.ClientDisconnected
               );
                
                yield break;
            }

            while (client != null && client.IsConnected)
            {
                while (request.Count > 0)
                {
                    (NetworkingPlayer player, Text frame, NetWorker sender) info = request.Dequeue();
                    JSONNode data = null;

                    try
                    {
                        // Get the list of hosts to iterate through from the frame payload
                        data = JSONNode.Parse(info.frame.ToString());
                        if (data == null)
                        {
                            throw new NullReferenceException($"Data null when trying to parse JSONNode.");
                        }
                    }
                    catch (Exception e)
                    {
                        Disconnect_MSD();

                        MasterServerException.LogException(
                            "Trying to get the received information cause an exception.",
                            inner: e,
                            MasterServerException.ExceptionTypeEnum.ReceiveException
                        );

                        yield break;
                    }

                    if (data["name"] != null)
                    {
                        requestServerInfSucceeded?.Invoke(
                        (
                                serverName: data["name"],
                                icon: data["icon"]
                            )
                        );
                    }
                    else if (data["hosts"] != null)
                    {
                        // Create a C# object for the response from the master server
                        var response = new MasterServerResponse(data["hosts"].AsArray);

                        if (response != null && response.serverResponse.Count > 0)
                        {
                            // Go through all of the available hosts and add them to the server browser
                            foreach (MasterServerResponse.Server server in response.serverResponse)
                            {
                                // Update UI or something with the above data
                                requestSessionInfSucceeded?.Invoke(server);
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        #endregion

        public LauncherOnMasterServer()
        {
            client = new TCPMasterClient();
            client.textMessageReceived += OnReceiveInfo;
        }

        #region IUMI3DMasterServerConnection

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="connectSucceeded">Action raise when the connection succeeded.</param>
        /// <param name="connectFailed">Action raise when the connection failed.</param>
        /// <returns></returns>
        public UMI3DAsyncOperation Connect_MSC(string url, Action connectSucceeded, Action connectFailed)
        {
            Disconnect_MSD();

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var asyncOperation = new UMI3DAsyncOperation();

            asyncOperation.completed += operation =>
            {
                if (client.IsConnected)
                {
                    RequestInfoCoroutine = WaitForInfo().AttachCoroutine();
                    connectSucceeded?.Invoke();
                }
                else
                {
                    connectFailed?.Invoke();
                }
            };

            asyncOperation.Start(
                () =>
                {
                    string[] ip_port = url.Split(':');
                    if (ip_port.Length > 2)
                    {
                        logger.Assertion($"{nameof(Connect_MSC)}", $"url: {url} has not the right format. It should be: 000.000.000.000:00000");
                        return;
                    }

                    string ip = ip_port[0];
                    ushort port = 15940; // default port.

                    if (ip_port.Length == 2)
                    {
                        ushort.TryParse(ip_port[1], out port);
                    }

                    client.Connect(ip, port);
                }
            );

            return asyncOperation;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestServerInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public void RequestServerInfo_MSC(Action<(string serverName, string icon)> requestServerInfSucceeded, Action requestFailed)
        {
            if (!client.IsConnected)
            {
                MasterServerException.LogException(
                   $"Client is disconnected when trying to {nameof(RequestServerInfo_MSC)}",
                   inner: null,
                   MasterServerException.ExceptionTypeEnum.ClientDisconnected
               );
                return;
            }

            this.requestServerInfSucceeded = requestServerInfSucceeded;

            var asyncOperation = new UMI3DAsyncOperation();

            asyncOperation.Start(
                () =>
                {
                    try
                    {
                        // Create the get request with the desired filters
                        var sendData = JSONNode.Parse("{}");
                        sendData.Add(requestServerInfoKey, aItem: new JSONClass());

                        client.Send(
                            frame: Text.CreateFromString(
                                client.Time.Timestep,
                                message: sendData.ToString(),
                                useMask: true,
                                Receivers.Server,
                                MessageGroupIds.MASTER_SERVER_GET,
                                isStream: true
                            )
                        );
                    }
                    catch (Exception e)
                    {
                        Disconnect_MSD();
                        requestFailed?.Invoke();

                        MasterServerException.LogException(
                            "Trying to send a request for information cause an exception.",
                            inner: e,
                            MasterServerException.ExceptionTypeEnum.SendException
                        );
                    }
                }
            );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sessionId">Id of the session.</param>
        /// <param name="requestSessionInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public void RequestSessionInfo_MSC(string sessionId, Action<MasterServerResponse.Server> requestSessionInfSucceeded, Action requestFailed)
        {
            if (!client.IsConnected)
            {
                MasterServerException.LogException(
                   $"Client is disconnected when trying to {nameof(RequestSessionInfo_MSC)}",
                   inner: null,
                   MasterServerException.ExceptionTypeEnum.ClientDisconnected
               );
                return;
            }

            var asyncOperation = new UMI3DAsyncOperation();

            asyncOperation.Start(
                () =>
                {
                    try
                    {
                        // Create the get request with the desired filters
                        var sendData = JSONNode.Parse("{}");
                        sendData.Add(
                            requestSessionInfoKey,
                            new JSONClass
                            {
                                // The id of the game to get
                                { "id", sessionId },

                                // The game type to choose from, if "any" then all types will be returned
                                { "type", "any" },

                                // The game mode to choose from, if "all" then all game modes will be returned
                                { "mode", "all" }
                            }
                        );

                        client.Send(
                            frame: Text.CreateFromString(
                                client.Time.Timestep,
                                message: sendData.ToString(),
                                useMask: true,
                                Receivers.Server,
                                MessageGroupIds.MASTER_SERVER_GET,
                                isStream: true
                            ));

                    }
                    catch (Exception e)
                    {
                        Disconnect_MSD();
                        requestFailed?.Invoke();

                        MasterServerException.LogException(
                            "Trying to send a request for information cause an exception.",
                            inner: e,
                            MasterServerException.ExceptionTypeEnum.SendException
                        );
                    }
                }
            );
        }

        #endregion

        /// <summary>
        /// Disconnect a master server asynchronously.
        /// 
        /// <para>
        /// Return an <see cref="UMI3DAsyncOperation"/> if <see cref="client"/> is not null, else return null.
        /// </para>
        /// </summary>
        public UMI3DAsyncOperation Disconnect_MSD()
        {
            if (RequestInfoCoroutine != null)
            {
                RequestInfoCoroutine.DetachCoroutine();
            }
            requestServerInfSucceeded = null;
            requestSessionInfSucceeded = null;
            request.Clear();

            client.textMessageReceived -= OnReceiveInfo;

            var asyncOperation = new UMI3DAsyncOperation();

            asyncOperation.Start(
                () =>
                {
                    client.Disconnect(forced: true);
                }
            );

            return asyncOperation;
        }

        /// <summary>
        /// An exception class to deal with <see cref="LauncherOnMasterServer"/> issues.
        /// </summary>
        [Serializable]
        public class MasterServerException : Exception
        {
            static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(MasterServerException)}");

            public enum ExceptionTypeEnum
            {
                Unknown,
                ClientNull,
                ClientDisconnected,
                SendException,
                ReceiveException
            }

            public ExceptionTypeEnum exceptionType;

            public MasterServerException(string message, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}")
            {
                this.exceptionType = exceptionType;
            }
            public MasterServerException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}", inner)
            {
                this.exceptionType = exceptionType;
            }

            public static void LogException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown)
            {
                logger.Exception(null, new MasterServerException(message, inner, exceptionType));
            }
        }
    }
}