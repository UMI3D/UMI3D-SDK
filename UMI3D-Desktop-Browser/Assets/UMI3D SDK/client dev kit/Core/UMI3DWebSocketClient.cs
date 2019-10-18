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
using MainThreadDispatcher;
using System.Collections;
using umi3d.common;
using UnityEngine;
using WebSocketSharp;
namespace umi3d.cdk
{
    public class UMI3DWebSocketClient : PersistentSingleton<UMI3DWebSocketClient>
    {
        
        protected WebSocket ws;

        public static bool Connected()
        {
            return Instance.ws != null && Instance.ws.IsConnected;
        }

        public static void Init()
        {
            var connection = UMI3DBrowser.Media.Connection as WebsocketConnectionDto;
            var socketUrl = connection.Url;
            socketUrl = socketUrl.Replace("http", "ws");

            Instance.ws = new WebSocket(socketUrl, "echo-protocol");

            Instance.ws.OnOpen += (sender, e) =>
            {
                var msg = new RealtimeConnectionRequestDto()
                {
                    UserId = UMI3DBrowser.UserId
                };
                Send(msg);
            };

            Instance.ws.OnMessage += (sender, e) =>
            {
                if (e == null)
                    return;
                var res = DtoUtility.Deserialize(e.Data);
                UnityMainThreadDispatcher.Instance().Enqueue(onMessage(res));
                
            };

            Instance.ws.OnError += (sender, e) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(onError("websocket error " + e.Message));
            };

            Instance.ws.OnClose += (sender, e) =>
            {
                Debug.Log("fermeture websocket");
                UnityMainThreadDispatcher.Instance().Enqueue(onClosed("websocket close " + e.Reason));
            };

            Instance.ws.Connect();

        }

        IEnumerator Reconnect()
        {
            yield return new WaitForSeconds(2f);
            if(!Connected())
                Instance?.ws?.Connect();
        }

        protected static IEnumerator onMessage(object obj)
        {
            //if (obj is EnterDto)
                //UMI3DBrowser.OpenEnvironment(obj as EnterDto);

            /*else*/ if (obj is UpdateObjectDto && UMI3DBrowser.Scene)
                UMI3DBrowser.Scene.UpdateObject(obj as UpdateObjectDto);

            else if (obj is MediaUpdateDto && UMI3DBrowser.Scene)
                UMI3DBrowser.Scene.UpdateFromDTO(obj as MediaUpdateDto);

            else if (obj is UpdateInteractionDto && UMI3DBrowser.Player)
                UMI3DBrowser.Player.UpdateInteraction(obj as UpdateInteractionDto);

            else if (obj is NavigateDto && UMI3DBrowser.Navigation)
                UMI3DBrowser.Navigation.Navigate(obj as NavigateDto);

            else if (obj is TeleportDto && UMI3DBrowser.Navigation)
                UMI3DBrowser.Navigation.Teleport(obj as TeleportDto);



            yield return null;
        }

        protected static IEnumerator onError(string err)
        {
            Debug.Log(err);
            yield return null;
        }

        protected static IEnumerator onClosed(string reason)
        {
            Debug.Log(reason);
            Instance.StartCoroutine(Instance.Reconnect());
            yield return null;
        }


        public static void Interact(string id, object evt = null)
        {
            var request = new InteractionRequestDto();
            request.Id = id;
            request.Arguments = evt;
            Send(request);
        }
        
        public static void Navigate(NavigationRequestDto data)
        {
            Send(data);
        }

        public static void Send(UMI3DDto obj)
        {
            if (Connected() && obj != null)
            {
                var data = DtoUtility.Serialize(obj);
                Instance.ws.SendAsync(data, (bool completed) => { });
            }
        }

        public static void Close()
        {
            if (Instance != null)
            {
                Instance.StopAllCoroutines();
                if (Instance.ws != null)
                {
                    Instance.ws.Close();
                }
                Destroy(Instance.gameObject);
            }
        }



        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(ws != null)
                ws.Close();
        }

    }
}
 