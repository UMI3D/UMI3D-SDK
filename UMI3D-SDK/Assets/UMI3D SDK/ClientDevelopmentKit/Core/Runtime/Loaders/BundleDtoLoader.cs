﻿/*
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
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace umi3d.cdk
{
    /// <summary>
    /// Resource Loader for a bundle
    /// </summary>
    public class BundleDtoLoader : IResourcesLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public List<string> supportedFileExtentions;
        public List<string> ignoredFileExtentions;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BundleDtoLoader()
        {
            supportedFileExtentions = new List<string>() { ".bundle" };
            ignoredFileExtentions = new List<string>();
        }

        /// <inheritdoc/>
        public bool IsSuitableFor(string extension)
        {
            return supportedFileExtentions.Contains(extension);
        }

        /// <inheritdoc/>
        public bool IsToBeIgnored(string extension)
        {
            return ignoredFileExtentions.Contains(extension);
        }

        /// <inheritdoc/>
        public virtual async Task<object> UrlToObject(string url, string extension, string authorization, string pathIfObjectInBundle = "")
        {
            // add bundle in the cache
#if UNITY_ANDROID
            UnityWebRequest www = url.Contains("http") ? UnityWebRequestAssetBundle.GetAssetBundle(url) : UnityWebRequestAssetBundle.GetAssetBundle("file://" + url);
#else
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
#endif
            SetCertificate(www, authorization);
            await UMI3DResourcesManager.DownloadObject(www);

            if (www.downloadHandler is DownloadHandlerAssetBundle downloadHandlerAssetBundle)
            {
                AssetBundle bundle = downloadHandlerAssetBundle?.assetBundle;

                www.Dispose();
                if (bundle != null) return (bundle);
#if UNITY_2020_OR_NEWER
                    else if (downloadHandlerAssetBundle?.error != null)
                        throw new Umi3dException($"An error has occurred during the decoding of the asset bundle’s assets.\n{downloadHandlerAssetBundle?.error}");
#endif
                else
                    throw new Umi3dException($"Asset bundle empty: \n\n\"{url}\" \n\nAn error might have occurred during the decoding of the asset bundle’s assets.");
            }
            www.Dispose();
            throw new Umi3dException("The downloadHandler provided is not a DownloadHandlerAssetBundle");
        }

        /// <see cref="IResourcesLoader.ObjectFromCache"/>
        public virtual async Task<object> ObjectFromCache(object o, string pathIfObjectInBundle)
        {
            if (pathIfObjectInBundle != null && pathIfObjectInBundle != "" && o is AssetBundle bundle)
            {
                if (Array.Exists(bundle.GetAllAssetNames(), element => { return element == pathIfObjectInBundle; }))
                {
#if UNITY_2020_1_OR_NEWER
                    var load = bundle.LoadAssetAsync(pathIfObjectInBundle);
                    while (!load.isDone)
                        await UMI3DAsyncManager.Yield();

                    UnityEngine.Object objectInBundle = load.asset;
#else
                    UnityEngine.Object objectInBundle = bundle.LoadAsset(pathIfObjectInBundle);
#endif
                    if (objectInBundle is Material)
                    {
                        return (new Material(objectInBundle as Material));
                    }
                    else
                    {
                        if (objectInBundle is GameObject)
                        {
                            AbstractMeshDtoLoader.HideModelRecursively((GameObject)objectInBundle);
                        }
                        return (objectInBundle);
                    }
                }
                else
                {
                    if (Array.Exists(bundle.GetAllScenePaths(), element => { return element == pathIfObjectInBundle; }))
                    {
                        var scene = await LoadScene(pathIfObjectInBundle);
                        AbstractMeshDtoLoader.HideModelRecursively(scene.Item1);
                        return scene;
                    }
                    else
                    {
                        UMI3DLogger.LogWarning($"Path {pathIfObjectInBundle} not found in Assets nor Scene", scope);
                        return (o);
                    }
                }
            }

            return (o);
        }

        /// <summary>
        /// Loads scene from bundle.
        /// </summary>
        /// <param name="scenePath"></param>
        /// <returns>(Empty object which contains every object of loaded scene; loaded scene, empty)</returns>
        private async Task<(GameObject, Scene)> LoadScene(string scenePath)
        {
            var asyncLoading = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            while (!asyncLoading.isDone)
                await UMI3DAsyncManager.Yield();

            var scene = SceneManager.GetSceneByPath(scenePath);

            GameObject sceneObj = new GameObject(scenePath);

            await UMI3DAsyncManager.Yield();

            foreach (var obj in scene.GetRootGameObjects())
            {
                obj.transform.SetParent(sceneObj.transform);

                foreach (var cam in obj.GetComponentsInChildren<Camera>())
                {
                    cam.gameObject.SetActive(false);
                    UMI3DLogger.LogWarning($"{cam.transform.name} has a camera, so it is disabled", scope);
                }
            }

            await UMI3DAsyncManager.Yield();

            LightProbes.TetrahedralizeAsync();

            return (sceneObj, scene);
        }

        /// <summary>
        /// set Certificate for webRequest.
        /// </summary>
        /// <param name="www">web request.</param>
        /// <param name="fileAuthorization">Authorization</param>
        public virtual void SetCertificate(UnityWebRequest www, string fileAuthorization)
        {
            if (fileAuthorization != null && fileAuthorization != "")
            {
                string authorization = fileAuthorization;
                if (!UMI3DClientServer.Instance.AuthorizationInHeader && www.url.StartsWith("http"))
                {
                    www.url = UMI3DResourcesManager.Instance.SetAuthorisationWithParameter(www.url, fileAuthorization);
                }
                else
                {
                    www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, authorization);
                }
            }
        }

        /// <inheritdoc/>
        public void DeleteObject(object objectLoaded, string reason)
        {
            if (objectLoaded != null) ((AssetBundle)objectLoaded).Unload(false);
        }
    }
}