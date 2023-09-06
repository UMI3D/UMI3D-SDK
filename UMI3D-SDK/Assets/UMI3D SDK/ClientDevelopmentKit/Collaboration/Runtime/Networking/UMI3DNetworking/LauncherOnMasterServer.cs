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
using System.Threading.Tasks;
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Used to connect to a Master Server, when a World Controller is not used.
    /// </summary>
    internal static class LauncherOnMasterServer
    {
        #region Private

        const string requestServerInfoKey = "info";
        const string requestSessionInfoKey = "get";

        const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration;
        static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(LauncherOnMasterServer)}");

        /// <summary>
        /// The Master Server communicates over TCP
        /// </summary>
        static TCPMasterClient client;

        static Queue<(
                NetworkingPlayer player, 
                Text frame, 
                NetWorker sender
            )> request = new();

        static UnityEngine.Coroutine RequestInfoCoroutine;

        static void OnReceiveInfo(
            NetworkingPlayer player, 
            Text frame, 
            NetWorker sender
        )
        {
            request.Enqueue((player, frame, sender));
        }

        static IEnumerator WaitForInfo()
        {
            if (client == null)
            {
                MasterServerException.LogException(
                   $"Client null when trying to {nameof(WaitForInfo)}",
                   inner: null,
                   MasterServerException.ExceptionTypeEnum.ClientNull
               );

                yield break;
            }

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
                        Disconnect();

                        MasterServerException.LogException(
                            "Trying to get the received information cause an exception.",
                            inner: e,
                            MasterServerException.ExceptionTypeEnum.ReceiveEcxeption
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


        #region Events

        /// <summary>
        /// Event raise when the connection failed.
        /// </summary>
        public static event Action connectFailed;

        /// <summary>
        /// Event raise when the connection succeeded.
        /// </summary>
        public static event Action connectSucceeded;

        /// <summary>
        /// Event raise when a request info has succeeded.
        /// </summary>
        public static event Action<(string serverName, string icon)> requestServerInfSucceeded;

        /// <summary>
        /// Event raise when a request info has succeeded.
        /// </summary>
        public static event Action<MasterServerResponse.Server> requestSessionInfSucceeded;

        #endregion


        /// <summary>
        /// Try to connect to a master server asyncronously.
        /// 
        /// <para>
        /// The connection is established in another thread. The <see cref="connectFailed"/> and <see cref="connectSucceeded"/> events are raised in the main-thread.
        /// </para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static AsyncOperation ConnectAsync(string url)
        {
            Disconnect();

            if (client == null)
            {
                client = new TCPMasterClient();
                client.textMessageReceived += OnReceiveInfo;
            }

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var asyncOperation = new AsyncOperation();

            asyncOperation.completed += operation =>
            {
                if (client.IsConnected)
                {
                    connectSucceeded?.Invoke();
                    RequestInfoCoroutine = CoroutineManager.Instance.AttachCoroutine(WaitForInfo());
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
                        logger.Assertion($"{nameof(ConnectAsync)}", $"url: {url} has not the right format. It should be: 000.000.000.000:00000");
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
        /// Disconnect a master server.
        /// </summary>
        public static void Disconnect()
        {
            if (client != null)
            {
                client.textMessageReceived -= OnReceiveInfo;
                if (RequestInfoCoroutine != null)
                {
                    CoroutineManager.Instance.DetachCoroutine(RequestInfoCoroutine);
                }
                request.Clear();

                var asyncOperation = new AsyncOperation();

                asyncOperation.Start(
                    () =>
                    {
                        client.Disconnect(forced: true);
                    }
                );
            }
        }

        /// <summary>
        /// Get the requested information about this master server asyncronously.
        /// 
        /// <para>
        /// The request is performed in another thread.
        /// </para>
        /// </summary>
        /// <param name="requestFailed"></param>
        public static void RequestServerInfo(Action requestFailed)
        {
            if (client == null)
            {
                MasterServerException.LogException(
                    $"Client null when trying to {nameof(RequestServerInfo)}", 
                    inner: null, 
                    MasterServerException.ExceptionTypeEnum.ClientNull
                );
                return;
            }

            if (!client.IsConnected)
            {
                MasterServerException.LogException(
                   $"Client is disconnected when trying to {nameof(RequestServerInfo)}",
                   inner: null,
                   MasterServerException.ExceptionTypeEnum.ClientDisconnected
               );
                return;
            }

            var asyncOperation = new AsyncOperation();

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
                        Disconnect();
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
        /// Get the requested information about this master server's <paramref name="sessionId"/> asyncronously.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="requestFailed"></param>
        public static void RequestSessionInfo(string sessionId, Action requestFailed)
        {
            if (client == null)
            {
                MasterServerException.LogException(
                    $"Client null when trying to {nameof(RequestSessionInfo)}",
                    inner: null,
                    MasterServerException.ExceptionTypeEnum.ClientNull
                );
                return;
            }

            if (!client.IsConnected)
            {
                MasterServerException.LogException(
                   $"Client is disconnected when trying to {nameof(RequestSessionInfo)}",
                   inner: null,
                   MasterServerException.ExceptionTypeEnum.ClientDisconnected
               );
                return;
            }

            var asyncOperation = new AsyncOperation();

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
                        Disconnect();
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
                ReceiveEcxeption
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