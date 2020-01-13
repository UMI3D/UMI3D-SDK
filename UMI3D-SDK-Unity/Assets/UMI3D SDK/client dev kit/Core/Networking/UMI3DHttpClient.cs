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
using System.Collections;
using UnityEngine;
using umi3d.common;
using System;
using UnityEngine.Networking;
using System.Text;


namespace umi3d.cdk
{
    public class UMI3DHttpClient : PersistentSingleton<UMI3DHttpClient>
    {
        public static int loadingObjectsCount = 0;

        #region loading
        /// <summary>
        /// request a media dto.
        /// </summary>
        /// <param name="url">the url to which the request should be send</param>
        /// <param name="callback">an action to do after the mediadto has been get back</param>
        public static void GetMedia(string url,Action<MediaDto> callback)
        {
            Instance.StartCoroutine(Instance._GetMedia(url, callback));
        }

        IEnumerator _GetMedia(string url, Action<MediaDto> finished)
        {
            url = "http://"+url+"/media";
            UnityWebRequest www = UnityWebRequest.Get(url);

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                Debug.LogError("Failed to load " + www.url);
                yield break;
            }
            var res = www.downloadHandler.text;
            MediaDto media = DtoUtility.Deserialize(res) as MediaDto;
            finished.Invoke(media);
        }

        /// <summary>
        /// Request dtos of an object children's.
        /// </summary>
        /// <param name="pid">the id of the parent object</param>
        /// <param name="finished">the callback to call after the children are loaded</param>
        public static void LoadSubObjects(string pid, Action finished)
        {
            loadingObjectsCount++;
            Instance.StartCoroutine(Instance._LoadSubObjects(pid, finished));
        }

        IEnumerator _LoadSubObjects(string pid, Action finished)
        {
            var url = UMI3DBrowser.Media.Url + "/load?user=" + UMI3DBrowser.UserId + "&pid=" + pid;
            UnityWebRequest www = UnityWebRequest.Get(url);

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                Debug.LogError("Failed to load " + www.url);
                yield break;
            }
            var res = www.downloadHandler.text;
            UMI3DBrowser.Scene.Load(DtoUtility.Deserialize(res) as LoadDto);
            finished.Invoke();
        }
        #endregion

        #region updates
        public static void StopUpdatesPooling()
        {
            Instance.StopCoroutine("_PoolUpdates");
        }

        public static void PoolUpdates()
        {
            Instance.StopCoroutine("_PoolUpdates");
            Instance.StartCoroutine(Instance._PoolUpdates());
        }

        IEnumerator _PoolUpdates()
        {
            while (true)
            {
                if (UMI3DBrowser.UserId != null)
                {
                    string query = UMI3DBrowser.Media.Url + "/update?user=" + UMI3DBrowser.UserId;
                    UnityWebRequest www = UnityWebRequest.Get(query);

                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.LogError(www.error);
                        Debug.LogError("Failed to load " + www.url);
                    }
                    else
                    {
                        var res = www.downloadHandler.text;

                        object data = DtoUtility.Deserialize(res);
                        if (data is UpdateDto)
                        {
                            var updates = data as UpdateDto;

                            if (UMI3DBrowser.interactionMapper)
                            {
                                if (updates.RemovedInteractions.Count > 0)
                                    UMI3DBrowser.interactionMapper.DeleteInteractions(updates.RemovedInteractions);
                                if (updates.RemovedTools.Count > 0)
                                    UMI3DBrowser.interactionMapper.DeleteTools(updates.RemovedTools);
                                if (updates.RemovedToolboxes.Count > 0)
                                    UMI3DBrowser.interactionMapper.DeleteToolboxes(updates.RemovedToolboxes);

                                if (updates.AddedToolboxes.Count > 0)
                                    UMI3DBrowser.interactionMapper.CreateToolboxes(updates.AddedToolboxes);
                                if (updates.AddedTools.Count > 0)
                                    UMI3DBrowser.interactionMapper.CreateTools(updates.AddedTools);
                                if (updates.AddedInteractions.Count > 0)
                                    UMI3DBrowser.interactionMapper.CreateInteractions(updates.AddedInteractions);
                            }

                            if (UMI3DBrowser.Scene)
                            {
                                foreach (var del in updates.RemovedObjects)
                                    UMI3DBrowser.Scene.Remove(del);
                                UMI3DBrowser.Scene.Load(updates.LoadedObjects);
                            }
                        }
                        else if (data is ToolProjectionRequestDto)
                        {
                            string toolId = (data as ToolProjectionRequestDto).toolToProjectId;
                            if (AbstractInteractionMapper.Instance.ToolExists(toolId))
                            {
                                AbstractInteractionMapper.Instance.SelectTool(toolId, new RequestedByEnvironment());
                            }
                            else
                            {
                                Debug.LogWarning("No tool to project !");
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(0.1f);
                while (loadingObjectsCount > 0)
                    yield return new WaitForEndOfFrame();
            }
        }
        #endregion

        #region interaction

        public static void Interact(HoveredDto dto)
        {
            Instance.StartCoroutine(Instance._RequestInteraction(dto));
        }



        public static void Interact(string id, object evt = null)
        {
            var request = new InteractionRequestDto
            {
                Id = id,
                Arguments = evt
            };
            Instance.StartCoroutine(Instance._RequestInteraction(request));
        }
         
        IEnumerator _RequestInteraction(UMI3DDto request)
        {
            if (UMI3DBrowser.Media != null)
            {
                var url = UMI3DBrowser.Media.Url + "/interact?user=" + UMI3DBrowser.UserId;
                UnityWebRequest www = CreatePostRequest(url, DtoUtility.Serialize(request));
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(www.error);
                    Debug.LogError("Failed to post /interact to" + www.url);
                    yield break;
                }
            }
            yield return null;
        }
        #endregion

        #region connection
        public static void Login(ConnectionRequestDto request)
        {
            Instance.StartCoroutine(Instance._Login(request));
        }

        IEnumerator _Login(ConnectionRequestDto request)
        {
            var url = UMI3DBrowser.Media.Url + "/login";
            UnityWebRequest www = CreatePostRequest(url, DtoUtility.Serialize(request), true);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                Debug.LogError("Failed to post /login to" + www.url);
                yield break;
            }
            var res = www.downloadHandler.text;
            UMI3DBrowser.OpenEnvironment(DtoUtility.Deserialize(res) as EnterDto);
        }

        public static void Logout()
        {
            Instance.StartCoroutine(Instance._Logout());
        }

        IEnumerator _Logout()
        {
            if (UMI3DBrowser.Media != null && UMI3DBrowser.UserId != null)
            {
                var url = UMI3DBrowser.Media.Url + "/logout?user=" + UMI3DBrowser.UserId;
                UnityWebRequest www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(www.error);
                    Debug.LogError("Failed to get /logout to" + www.url);
                    yield break;
                }
            }
            else
                yield return null;
        }

        #endregion

        #region httpUtils
        UnityWebRequest CreatePostRequest(string url, string json, bool withResult = false)
        {
            UnityWebRequest requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
            requestU.uploadHandler = uH;
            if (withResult)
                requestU.downloadHandler = new DownloadHandlerBuffer();
            requestU.SetRequestHeader("Content-Type", "application/json");
            return requestU;
        }
        #endregion


    }
}