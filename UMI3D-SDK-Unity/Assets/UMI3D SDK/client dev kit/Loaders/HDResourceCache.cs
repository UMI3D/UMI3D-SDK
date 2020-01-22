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
using AsImpL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityGLTF;

namespace umi3d.cdk
{
    /// <summary>
    /// Handle asynchronous ressource download.
    /// </summary>
    public class HDResourceCache : PersistentSingleton<HDResourceCache>
    {

        struct ModelCache
        {
            public Transform transform;
            public Action<GameObject> callback;
            public string name;

            public ModelCache(Transform t, Action<GameObject> a, string s)
            {
                transform = t;
                callback = a;
                name = s;
            }
        }

        Transform loadingSpot;

        Dictionary<string, string> httpCache = new Dictionary<string, string>();
        Dictionary<string, List<Action<string>>> httpCallbackCache = new Dictionary<string, List<Action<string>>>();
        Dictionary<string, List<Action<UnityWebRequest>>> httpFailCallbackCache = new Dictionary<string, List<Action<UnityWebRequest>>>();

        Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        Dictionary<string, List<Action<Texture2D>>> textureCallbackCache = new Dictionary<string, List<Action<Texture2D>>>();
        Dictionary<string, List<Action<UnityWebRequest>>> textureFailCallbackCache = new Dictionary<string, List<Action<UnityWebRequest>>>();

        Dictionary<string, GameObject> modelCache = new Dictionary<string, GameObject>();
        Dictionary<string, List<ModelCache>> modelCallbackCache = new Dictionary<string, List<ModelCache>>();

        Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
        Dictionary<string, List<Action<AudioClip>>> audioCallbackCache = new Dictionary<string, List<Action<AudioClip>>>();
        Dictionary<string, List<Action<UnityWebRequest>>> audioFailCallbackCache = new Dictionary<string, List<Action<UnityWebRequest>>>();

        protected override void Awake()
        {
            base.Awake();
            Instance.loadingSpot = Instance.transform;
            Instance.loadingSpot.localScale = Vector3.zero;
            Instance.loadingSpot.position = new Vector3(-100000, -100000, -100000);
            Instance.loadingSpot.gameObject.layer = 9; // invisible layer
        }

        /// <summary>
        /// Clear all caches.
        /// </summary>
        public static void ClearCache()
        {
            Instance.httpCache.Clear();
            Instance.httpCallbackCache.Clear();
            Instance.httpFailCallbackCache.Clear();
            Instance.textureCache.Clear();
            Instance.textureCallbackCache.Clear();
            Instance.textureFailCallbackCache.Clear();
            Instance.audioCache.Clear();
            Instance.audioFailCallbackCache.Clear();
            Instance.audioCallbackCache.Clear();
            Instance.modelCache.Clear();
            Instance.modelCallbackCache.Clear();
            for (int i = 0; i < Instance.transform.childCount; i++)
            {
                Destroy(Instance.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Format the given path into a valid url.
        /// </summary>
        /// <param name="resourcePath">Path to format</param>
        /// <returns>Valid url</returns>
        string getUrl(string resourcePath)
        {
            string path = resourcePath;
            if (path.StartsWith("http:/") && !path.StartsWith("http://"))
            {
                path = path.Insert(6, "/");
            }
            else if (path.StartsWith("https:/") && !path.StartsWith("https://"))
            {
                path = path.Insert(7, "/");
            }
            return (path.Contains("//") ? path : "file:///" + path);
        }

        void setCertificate(UnityWebRequest www, Resource resource)
        {
            if (resource.Login != "" && resource.Password != "")
            {
                www.certificateHandler = new AcceptAllCertificates();

                string authorization = resource.Login + ":" + resource.Password;
                authorization = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(authorization));
                authorization = "Basic " + authorization;

                www.SetRequestHeader("AUTHORIZATION", authorization);
            }
        }

        IEnumerator _Download(Resource resource, Action<string> callback, Action<UnityWebRequest> failCallback)
        {
            if (!httpCache.ContainsKey(resource.Url))
            {
                httpCache.Add(resource.Url, null);
                string url = getUrl(resource.Url);

                UnityWebRequest www = UnityWebRequest.Get(url);
                setCertificate(www, resource);

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    httpCache.Remove(resource.Url);

                    if (httpFailCallbackCache.ContainsKey(resource.Url))
                    {
                        foreach (Action<UnityWebRequest> cb in httpFailCallbackCache[resource.Url])
                        {
                            cb.Invoke(www);
                        }
                        httpFailCallbackCache.Remove(resource.Url);
                    }

                    if (failCallback != null)
                    {
                        failCallback.Invoke(www);

                    }
                    else
                    {
                        Debug.LogWarning(www.error);
                        Debug.LogWarning("Failed to load " + www.url);
                    }
                    yield break;
                }
                httpCache[resource.Url] = www.downloadHandler.text;

                callback.Invoke(httpCache[resource.Url]);

                if (httpCallbackCache.ContainsKey(resource.Url))
                {
                    foreach (Action<string> cb in httpCallbackCache[resource.Url])
                    {
                        cb.Invoke(httpCache[resource.Url]);
                    }
                    httpCallbackCache.Remove(resource.Url);
                }

            }
            else if (httpCache[resource.Url] == null)
            {
                if (!httpCallbackCache.ContainsKey(resource.Url))
                    httpCallbackCache.Add(resource.Url, new List<Action<string>>());

                httpCallbackCache[resource.Url].Add(callback);

                if (failCallback != null)
                {
                    if (!httpFailCallbackCache.ContainsKey(resource.Url))
                        httpFailCallbackCache.Add(resource.Url, new List<Action<UnityWebRequest>>());

                    httpFailCallbackCache[resource.Url].Add(failCallback);
                }
            }
            else
                callback.Invoke(httpCache[resource.Url]);
        }

        IEnumerator _Download(Resource resource, Action<Texture2D> callback, Action<UnityWebRequest> failCallback)
        {
            if (!textureCache.ContainsKey(resource.Url))
            {
                textureCache.Add(resource.Url, null);
                string url = getUrl(resource.Url);
                
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

                setCertificate(www, resource);

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    textureCache.Remove(resource.Url);
                    if (textureFailCallbackCache.ContainsKey(resource.Url))
                    {
                        foreach (Action<UnityWebRequest> cb in textureFailCallbackCache[resource.Url])
                        {
                            cb.Invoke(www);
                        }
                        textureFailCallbackCache.Remove(resource.Url);
                    }

                    if (failCallback != null)
                    {
                        failCallback.Invoke(www);

                    }
                    else
                    {
                        Debug.LogWarning(www.error);
                        Debug.LogWarning("Failed to load " + www.url);
                    }
                    yield break;
                }
                textureCache[resource.Url] = ((DownloadHandlerTexture)www.downloadHandler).texture;
                //textureCache[resource.Url].alphaIsTransparency = true; 
                callback.Invoke(textureCache[resource.Url]);
                if (textureCallbackCache.ContainsKey(resource.Url))
                {
                    foreach (Action<Texture2D> cb in textureCallbackCache[resource.Url])
                    {
                        cb.Invoke(textureCache[resource.Url]);
                    }
                    textureCallbackCache.Remove(resource.Url);
                }
            }
            else if (textureCache[resource.Url] == null)
            {
                if (!textureCallbackCache.ContainsKey(resource.Url))
                    textureCallbackCache.Add(resource.Url, new List<Action<Texture2D>>());

                textureCallbackCache[resource.Url].Add(callback);

                if (failCallback != null)
                {
                    if (!textureFailCallbackCache.ContainsKey(resource.Url))
                        textureFailCallbackCache.Add(resource.Url, new List<Action<UnityWebRequest>>());

                    textureFailCallbackCache[resource.Url].Add(failCallback);
                }
            }
            else
                callback.Invoke(textureCache[resource.Url]);
        }

        IEnumerator _Download(Resource resource, Action<AudioClip> callback, Action<UnityWebRequest> failCallback)
        {
            if (!audioCache.ContainsKey(resource.Url))
            {
                audioCache.Add(resource.Url, null);
                string url = getUrl(resource.Url);

                UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
                setCertificate(www, resource);

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    audioCache.Remove(resource.Url);
                    if (audioFailCallbackCache.ContainsKey(resource.Url))
                    {
                        foreach (Action<UnityWebRequest> cb in audioFailCallbackCache[resource.Url])
                        {
                            cb.Invoke(www);
                        }
                        audioFailCallbackCache.Remove(resource.Url);
                    }

                    if (failCallback != null)
                    {
                        failCallback.Invoke(www);

                    }
                    else
                    {
                        Debug.LogWarning(www.error);
                        Debug.LogWarning("Failed to load " + www.url);
                    }
                    //totalProgress.fileProgress.Remove( objLoadingProgress );
                    yield break;
                }
                audioCache[resource.Url] = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;
                callback.Invoke(audioCache[resource.Url]);
                if (audioCallbackCache.ContainsKey(resource.Url))
                {
                    foreach (Action<AudioClip> cb in audioCallbackCache[resource.Url])
                    {
                        cb.Invoke(audioCache[resource.Url]);
                    }
                    audioCallbackCache.Remove(resource.Url);
                }
            }
            else if (audioCache[resource.Url] == null)
            {
                if (!audioCallbackCache.ContainsKey(resource.Url))
                    audioCallbackCache.Add(resource.Url, new List<Action<AudioClip>>());

                audioCallbackCache[resource.Url].Add(callback);

                if (failCallback != null)
                {
                    if (!audioFailCallbackCache.ContainsKey(resource.Url))
                        audioFailCallbackCache.Add(resource.Url, new List<Action<UnityWebRequest>>());

                    audioFailCallbackCache[resource.Url].Add(failCallback);
                }
            }
            else
                callback.Invoke(audioCache[resource.Url]);
        }
        
        /// <summary>
        /// Asynchronous ressource download.
        /// </summary>
        /// <param name="resource">Ressource to download</param>
        /// <param name="callback">Callback to execute</param>
        public static void Download(Resource resource, Action<string> callback, Action<UnityWebRequest> failCallback = null)
        {
            var coroutine = Instance._Download(resource, callback, failCallback);
            Instance.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Asynchronous ressource download.
        /// </summary>
        /// <param name="resource">Ressource to download</param>
        /// <param name="callback">Callback to execute</param>
        public static void Download(Resource resource, Action<AudioClip> callback, Action<UnityWebRequest> failCallback = null)
        {
            var coroutine = Instance._Download(resource, callback, failCallback);
            Instance.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Asynchronous ressource download.
        /// </summary>
        /// <param name="resource">Ressource to download</param>
        /// <param name="callback">Callback to execute</param>
        public static void Download(Resource resource, Action<Texture2D> callback, Action<UnityWebRequest> failCallback = null)
        {
            var coroutine = Instance._Download(resource, callback, failCallback);
            Instance.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Asynchronous ressource download.
        /// </summary>
        /// <param name="resource">Ressource to download</param>
        /// <param name="parent">Scene object to instantiate object as child of</param>
        /// <param name="callback">Callback to execute</param>
        /// <param name="name">Model's name in scene</param>
        public static void Download(Resource resource, Transform parent, Action<GameObject> callback, string name = "model")
        {
            resource.Url = resource.Url.Replace('\\', '/');
            if (Instance.modelCache.ContainsKey(resource.Url))
            {
                if (Instance.modelCache[resource.Url] != null)
                    Instance.InstanciateFromCache(resource.Url, parent, callback, name);
                else
                {
                    if (!Instance.modelCallbackCache.ContainsKey(resource.Url))
                        Instance.modelCallbackCache.Add(resource.Url, new List<ModelCache>());

                    Instance.modelCallbackCache[resource.Url].Add(new ModelCache(parent, callback, name));
                }
            }
            else
            {
                Instance.modelCache.Add(resource.Url, null);
                GameObject pivot = new GameObject(name + "_pivot");
                pivot.transform.SetParent(Instance.loadingSpot.transform, false);
                pivot.transform.localPosition = Vector3.zero;
                pivot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                pivot.transform.localScale = Vector3.one;
                string extension = Path.GetExtension(resource.Url);
                switch (extension)
                {
                    case ".gltf":
                        var gltf_component = pivot.AddComponent<GLTFComponent>();
                        //TODO IF RESOURCE FILE NOT LOCAL
                        gltf_component.GLTFUri = resource.Url;
                        gltf_component.MaximumLod = 300;
                        gltf_component.Collider = GLTFSceneImporter.ColliderType.Mesh;
                        gltf_component.Load();
                        if (Instance.modelCache.ContainsKey(resource.Url) && Instance.modelCache[resource.Url] != null)
                        {
                            Destroy(pivot);
                        }
                        else
                        {
                            Instance.modelCache[resource.Url] = pivot;
                            pivot.SetActive(false);
                        }
                        Instance.InstanciateFromCache(resource.Url, parent, callback, name);

                        if (Instance.modelCallbackCache.ContainsKey(resource.Url))
                        {
                            foreach (ModelCache model in Instance.modelCallbackCache[resource.Url])
                            {
                                Instance.InstanciateFromCache(resource.Url, model.transform, model.callback, model.name);
                            }
                            Instance.modelCallbackCache.Remove(resource.Url);
                        }
                        break;
                    case ".glb":
                        var glb_component = pivot.AddComponent<GLTFComponent>();
                        //TODO IF RESOURCE FILE NOT LOCAL
                        glb_component.GLTFUri = resource.Url;
                        glb_component.MaximumLod = 300;
                        glb_component.Collider = GLTFSceneImporter.ColliderType.Mesh;
                        glb_component.Load();
                        if (Instance.modelCache.ContainsKey(resource.Url) && Instance.modelCache[resource.Url] != null)
                        {
                            Destroy(pivot);
                        }
                        else
                        {
                            Instance.modelCache[resource.Url] = pivot;
                            pivot.SetActive(false);
                        }
                        Instance.InstanciateFromCache(resource.Url, parent, callback, name);

                        if (Instance.modelCallbackCache.ContainsKey(resource.Url))
                        {
                            foreach (ModelCache model in Instance.modelCallbackCache[resource.Url])
                            {
                                Instance.InstanciateFromCache(resource.Url, model.transform, model.callback, model.name);
                            }
                            Instance.modelCallbackCache.Remove(resource.Url);
                        }
                        break;
                    default:
                        var importer = pivot.AddComponent<ObjectImporter>();
                        var options = new ImportOptions();
                        options.localPosition = Vector3.zero;
                        options.localEulerAngles = new Vector3(0f, 0f, 0f);
                        options.localScale = Vector3.one;
                        options.localPosition = Vector3.zero;
                        options.buildColliders = false;
                        importer.ImportingComplete += () =>
                        {
                            if (Instance.modelCache.ContainsKey(resource.Url) && Instance.modelCache[resource.Url] != null)
                            {
                                Destroy(pivot);
                            }
                            else
                            {
                                Instance.modelCache[resource.Url] = pivot;
                                pivot.SetActive(false);
                            }
                            Instance.InstanciateFromCache(resource.Url, parent, callback, name);

                            if (Instance.modelCallbackCache.ContainsKey(resource.Url))
                            {
                                foreach (ModelCache model in Instance.modelCallbackCache[resource.Url])
                                {
                                    Instance.InstanciateFromCache(resource.Url, model.transform, model.callback, model.name);
                                }
                                Instance.modelCallbackCache.Remove(resource.Url);
                            }
                        };
                        importer.ImportModelAsync(name, resource, pivot.transform, options);
                        break;
                }
            }
        }


        /// <summary>
        /// Instanciate model from cache, and pass it to a given callback.
        /// </summary>
        /// <param name="url">Model's url</param>
        /// <param name="parent">Scene object to instantiate object as child of</param>
        /// <param name="callback">Callback to execute after instantiation</param>
        /// <param name="name">Model's name in scene</param>
        void InstanciateFromCache(string url, Transform parent, Action<GameObject> callback, string name = "model")
        {
            if (modelCache.ContainsKey(url))
            {
                var model = Instantiate(modelCache[url], parent, false);
                model.SetActive(true);
                model.name = name;
                callback.Invoke(model);
            }
        }
    }
}