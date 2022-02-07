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
using System.Collections;
using System.Collections.Generic;
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

        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

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

        /// <see cref="IResourcesLoader.IsSuitableFor"/>
        public bool IsSuitableFor(string extension)
        {
            return supportedFileExtentions.Contains(extension);
        }

        /// <see cref="IResourcesLoader.IsToBeIgnored"/>
        public bool IsToBeIgnored(string extension)
        {
            return ignoredFileExtentions.Contains(extension);
        }

        /// <see cref="IResourcesLoader.UrlToObject"/>
        public virtual void UrlToObject(string url, string extension, string authorization, Action<object> callback, Action<Umi3dException> failCallback, string pathIfObjectInBundle = "")
        {
            // add bundle in the cache
#if UNITY_ANDROID
            UnityWebRequest www = url.Contains("http") ? UnityWebRequestAssetBundle.GetAssetBundle(url) : UnityWebRequestAssetBundle.GetAssetBundle("file://" + url);
#else
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
#endif
            SetCertificate(www, authorization);
            UMI3DResourcesManager.DownloadObject(www,
                () =>
                {
                    try
                    {
                        if (www.downloadHandler is DownloadHandlerAssetBundle downloadHandlerAssetBundle)
                        {
                            AssetBundle bundle = downloadHandlerAssetBundle?.assetBundle;
                            if (bundle != null)
                                callback.Invoke(bundle);

#if UNITY_2020
                            if (downloadHandlerAssetBundle?.error != null)
                                throw new Umi3dException($"An error has occurred during the decoding of the asset bundle’s assets.\n{downloadHandlerAssetBundle?.error}");
#endif
                            else
                                throw new Umi3dException("The asset bundle was empty. An error might have occurred during the decoding of the asset bundle’s assets.");
                        }
                        else
                            throw new Umi3dException("The downloadHandler provided is not a DownloadHandlerAssetBundle");
                    }
                    catch (Exception e)
                    {
                        failCallback.Invoke(new Umi3dException(e));
                    }
                },
                s => { failCallback?.Invoke(s); }
            );
        }

        /// <see cref="IResourcesLoader.ObjectFromCache"/>
        public virtual void ObjectFromCache(object o, Action<object> callback, string pathIfObjectInBundle)
        {
            UMI3DEnvironmentLoader.StartCoroutine(_ObjectFromCache(o, callback, pathIfObjectInBundle));
        }

        IEnumerator _ObjectFromCache(object o, Action<object> callback, string pathIfObjectInBundle)
        {
            /*     
                Usefull to find pathIfObjectInBundle in a bundle
                UMI3DLogger.Log("asset count : "+((AssetBundle)o).GetAllAssetNames().Length,scope);
                UMI3DLogger.Log("scene count : "+((AssetBundle)o).GetAllScenePaths().Length,scope);
                UMI3DLogger.Log(((AssetBundle)o).GetAllAssetNames()[0],scope);
            */
            if (pathIfObjectInBundle != null && pathIfObjectInBundle != "" && o is AssetBundle bundle)
            {
                if (Array.Exists((bundle).GetAllAssetNames(), element => { return element == pathIfObjectInBundle; }))
                {
#if UNITY_2020_1_OR_NEWER
                    var load = bundle.LoadAssetAsync(pathIfObjectInBundle);
                    yield return load;
                    UnityEngine.Object objectInBundle = load.asset;
#else
                    UnityEngine.Object objectInBundle = bundle.LoadAsset(pathIfObjectInBundle);
#endif
                    if (objectInBundle is GameObject)
                    {
                        AbstractMeshDtoLoader.HideModelRecursively((GameObject)objectInBundle);
                    }
                    callback.Invoke(objectInBundle);
                }
                else
                {
                    if (Array.Exists((bundle).GetAllScenePaths(), element => { return element == pathIfObjectInBundle; }))
                    {
                        var scene = SceneManager.LoadSceneAsync((string)o, LoadSceneMode.Additive);
                        yield return scene;
                        callback.Invoke(null);
                    }
                    else
                    {
                        UMI3DLogger.LogWarning($"Path {pathIfObjectInBundle} not found in Assets nor Scene", scope);
                        callback.Invoke(o);
                    }
                }
            }
            else
                callback.Invoke(o);
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
                www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, authorization);
            }
        }

        /// <see cref="IResourcesLoader.DeleteObject"/>
        public void DeleteObject(object objectLoaded, string reason)
        {
            if (objectLoaded != null) ((AssetBundle)objectLoaded).Unload(true);
        }

    }
}