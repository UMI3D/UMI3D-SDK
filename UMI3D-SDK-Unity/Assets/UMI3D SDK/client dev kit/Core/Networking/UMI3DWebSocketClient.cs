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

        /// <summary>
        /// State if the Client is connected.
        /// </summary>
        public static bool Connected()
        {
            return Instance.ws != null && Instance.ws.IsConnected;
        }

        /// <summary>
        /// Setup the client.
        /// </summary>
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
            if (obj is UpdateObjectDto && UMI3DBrowser.Scene)
                UMI3DBrowser.Scene.UpdateObject(obj as UpdateObjectDto);

            else if (obj is MediaUpdateDto && UMI3DBrowser.Scene)
                UMI3DBrowser.Scene.UpdateFromDTO(obj as MediaUpdateDto);

            else if (obj is UpdateInteractionDto && obj != null && UMI3DBrowser.interactionMapper)
                UMI3DBrowser.interactionMapper.UpdateInteraction((obj as UpdateInteractionDto).Entity);

            else if (obj is AvatarMappingDto && obj != null)
                UMI3DBrowserAvatar.Instance.LoadAvatarMapping(obj as AvatarMappingDto);

            else if (obj is NavigateDto && UMI3DBrowser.Navigation)
                UMI3DBrowser.Navigation.Navigate(obj as NavigateDto);

            else if (obj is TeleportDto && UMI3DBrowser.Navigation)
                UMI3DBrowser.Navigation.Teleport(obj as TeleportDto);

            else if (obj is ToolProjectionRequestDto)
            {
                string toolId = (obj as ToolProjectionRequestDto).toolToProjectId;
                if (AbstractInteractionMapper.Instance.ToolExists(toolId))
                {
                    AbstractInteractionMapper.Instance.SelectTool(toolId, new RequestedByEnvironment());
                }
                else
                {
                    Debug.LogWarning("No tool to project !");
                }
            }
            else if (obj is NotificationDto)
                UMI3DBrowser.notificationManager?.Notify(obj as NotificationDto);

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

        /// <summary>
        /// Send a HoverDto.
        /// </summary>
        /// <param name="dto"></param>
        public static void Interact(HoveredDto dto)
        {
            Send(dto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="evt"></param>
        public static void Interact(string id, object evt = null)
        {
            var request = new InteractionRequestDto();
            request.Id = id;
            request.Arguments = evt;
            Send(request);
        }

        /// <summary>
        /// Send a NavigationRequestDto.
        /// </summary>
        /// <param name="data"></param>
        public static void Navigate(NavigationRequestDto data)
        {
            Send(data);
        }

        /// <summary>
        /// Send a UMI3DDto.
        /// </summary>
        /// <param name="obj"></param>
        public static void Send(UMI3DDto obj)
        {
            if (Connected() && obj != null)
            {
                var data = DtoUtility.Serialize(obj);
                Instance.ws.SendAsync(data, (bool completed) => { });
            }
        }

        /// <summary>
        /// Terminate connection.
        /// </summary>
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
 