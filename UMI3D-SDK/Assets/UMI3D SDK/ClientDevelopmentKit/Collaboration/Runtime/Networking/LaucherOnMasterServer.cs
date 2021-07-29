﻿/*
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

using BeardedManStudios.Forge.Networking;
using BeardedManStudios.SimpleJSON;
using System;
using UnityEngine;

public class LaucherOnMasterServer
{

    TCPMasterClient client = null;
    /// <summary>
    /// Try to connect to master server. callback is invoked if the server accepts the connection. ip_port format is 000.000.000.000:00000
    /// if only ip is given, the default port (15940) will be used
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="ip_port"></param>
    public void ConnectToMasterServer(Action callback, string ip_port)
    {
        var tab = ip_port.Split(':');
        if (tab.Length == 1)
        {
            ConnectToMasterServer(callback, tab[0], 15940); // use default port
        }
        else
        {
            ushort port;
            if (ushort.TryParse(tab[1], out port))
            {
                ConnectToMasterServer(callback, tab[0], port);
            }
            else
            {
                ConnectToMasterServer(callback, tab[0], 15940); // use default port
            }
        }
    }

    /// <summary>
    /// Try to connect to master server. callback is invoked if the server accepts the connection. host format is 000.000.000.000
    /// 
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public void ConnectToMasterServer(Action callback, string host, ushort port)
    {

        // The Master Server communicates over TCP
        client = new TCPMasterClient();

        // Just call the connect method and you are ready to go
        client.Connect(host, port);

        client.serverAccepted += (netWorker) =>
        {
            Debug.Log("server accepted");
            callback.Invoke();
        };

    }

    public void SendDataSession(string sessionId, Action<MasterServerResponse.Server> UIcallback)
    {
        try
        {
            // The overall game id to select from
            string gameId = sessionId;

            // The game type to choose from, if "any" then all types will be returned
            string gameType = "any";

            // The game mode to choose from, if "all" then all game modes will be returned
            string gameMode = "all";

            // Create the get request with the desired filters
            JSONNode sendData = JSONNode.Parse("{}");
            JSONClass getData = new JSONClass();

            // The id of the game to get
            getData.Add("id", gameId);
            getData.Add("type", gameType);
            getData.Add("mode", gameMode);

            sendData.Add("get", getData);

            // Send the request to the server
            Debug.Log("send request to master server");
            //client.binaryMessageReceived += (x,y,z) => { Debug.Log("bin massage received"); };
            client.textMessageReceived += (player, frame, sender) => { Debug.Log("Receive message from master server"); ReceiveMasterDatas(player, frame, sender, UIcallback); };
            client.Send(BeardedManStudios.Forge.Networking.Frame.Text.CreateFromString(client.Time.Timestep, sendData.ToString(), true, Receivers.Server, MessageGroupIds.MASTER_SERVER_GET, true));
            Debug.Log("request send to master server... ");

        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            // If anything fails, then this client needs to be disconnected
            client.Disconnect(true);
            client = null;

        }

    }

    private void ReceiveMasterDatas(NetworkingPlayer player, BeardedManStudios.Forge.Networking.Frame.Text frame, NetWorker sender, Action<MasterServerResponse.Server> UICallback)
    {
        Debug.Log("Receive datas from master server");
        try
        {
            // Get the list of hosts to iterate through from the frame payload
            JSONNode data = JSONNode.Parse(frame.ToString());
            if (data["hosts"] != null)
            {
                // Create a C# object for the response from the master server
                MasterServerResponse response = new MasterServerResponse(data["hosts"].AsArray);

                if (response != null && response.serverResponse.Count > 0)
                {
                    // Go through all of the available hosts and add them to the server browser
                    foreach (MasterServerResponse.Server server in response.serverResponse)
                    {
                        Debug.Log("Name: " + server.Name);
                        Debug.Log("Address: " + server.Address);
                        Debug.Log("Port: " + server.Port);
                        /*    Debug.Log("Comment: " + server.Comment);
                            Debug.Log("Type: " + server.Type);
                            Debug.Log("Mode: " + server.Mode);
                            Debug.Log("Players: " + server.PlayerCount);
                            Debug.Log("Max Players: " + server.MaxPlayers);
                            Debug.Log("Protocol: " + server.Protocol);*/

                        // Update UI or something with the above data
                        UICallback.Invoke(server);

                    }
                }
            }
        }
        finally
        {
            if (client != null)
            {
                // If we succeed or fail the client needs to disconnect from the Master Server
                client.Disconnect(true);
                client = null;
            }
        }

    }



    // disconnect TCPMasterClient
    ~LaucherOnMasterServer()
    {
        if (client != null)
        {
            // If anything fails, then this client needs to be disconnected
            client.Disconnect(true);
            client = null;
        }
    }



}
