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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// Represents all <see cref="AssetBundle"/> assets and scenes to be able to unload the bundle.
        /// </summary>
        private class BundleCacheData
        {
            /// <summary>
            /// All bundle assets by bundle path.
            /// </summary>
            public Dictionary<string, Object> assets = new();

            /// <summary>
            /// All bundles scenes by scene path.
            /// </summary>
            public Dictionary<string, (GameObject root, Scene scene)> scenes = new();
        }

        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public List<string> supportedFileExtensions;
        public List<string> ignoredFileExtensions;

        /// <summary>
        /// Bundles can be loaded only one by one.
        /// </summary>
        private volatile bool isLoadingABundle = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BundleDtoLoader()
        {
            supportedFileExtensions = new List<string>() { ".bundle" };
            ignoredFileExtensions = new List<string>();
        }

        /// <inheritdoc/>
        public bool IsSuitableFor(string extension)
        {
            return supportedFileExtensions.Contains(extension);
        }

        /// <inheritdoc/>
        public bool IsToBeIgnored(string extension)
        {
            return ignoredFileExtensions.Contains(extension);
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
                while (isLoadingABundle)
                {
                    await Task.Delay(500);
                }

                isLoadingABundle = true;

                AssetBundle bundle = downloadHandlerAssetBundle?.assetBundle;

                if (bundle != null)
                {
                    www.Dispose();

                    BundleCacheData data = new BundleCacheData();

                    try
                    {
                        Object obj = null;

                        foreach (string assetPath in bundle.GetAllAssetNames())
                        {
                            obj = bundle.LoadAsset(assetPath);
                            data.assets[assetPath] = obj;
                        }

                        foreach (string scenePath in bundle.GetAllScenePaths())
                        {
                            data.scenes[scenePath] = await LoadScene(scenePath);
                            obj = data.scenes[scenePath].root;
                        }

                        if (obj is GameObject go)
                        {
                            AbstractMeshDtoLoader.HideModelRecursively(go);
#if UNITY_EDITOR && !UNITY_STANDALONE_WIN
                            UnityEngine.Debug.Log("<color=green>TODO: </color>" + $"Fix shader on asset bundle go {go.name}");
                            ShaderFix.FixShadersForEditor(go);
#endif
                        }

                        var op = bundle.UnloadAsync(false);

                        while (op.isDone)
                            await UMI3DAsyncManager.Yield();
                    }
                    catch (System.Exception ex)
                    {
                        UMI3DLogger.LogException(ex, scope);
                    }

                    isLoadingABundle = false;

                    return data;
                }
#if UNITY_2020_1_OR_NEWER
                else if (downloadHandlerAssetBundle?.error != null)
                {
                    string error = downloadHandlerAssetBundle?.error;
                    www.Dispose();

                    isLoadingABundle = false;

                    throw new Umi3dBundleException($"An error has occurred during the decoding of the asset bundle’s assets.\n{error}", error.Contains("can't be loaded because another AssetBundle with the same files is already loaded."));
                }
#endif
                else
                {
                    UMI3DResourcesManager.Instance.DebugCache();
                    www.Dispose();

                    isLoadingABundle = false;

                    throw new Umi3dBundleException($"Asset bundle empty: \n\n\"{url}\" \n\nAn error might have occurred during the decoding of the asset bundle’s assets.", true);
                }
            }

            www.Dispose();

            throw new common.Umi3dException("The downloadHandler provided is not a DownloadHandlerAssetBundle");
        }

        /// <see cref="IResourcesLoader.ObjectFromCache"/>
        public virtual async Task<object> ObjectFromCache(object o, string pathIfObjectInBundle)
        {
            if (!string.IsNullOrEmpty(pathIfObjectInBundle) && o is BundleCacheData data)
            {
                bool isAsset = data.assets.ContainsKey(pathIfObjectInBundle);
                bool isScene = false;

                if (!isAsset)
                    isScene = data.scenes.ContainsKey(pathIfObjectInBundle);

                if (!isAsset && !isScene)
                {
                    object result = null;

                    string matchingPath = data.assets.Keys.FirstOrDefault(path => path.Contains(pathIfObjectInBundle));
                    if (matchingPath != null)
                        result = data.assets[matchingPath];

                    if (result != null)
                    {
                        isAsset = true;
                        pathIfObjectInBundle = matchingPath;
                    }
                    else
                    {
                        matchingPath = data.scenes.Keys.FirstOrDefault(path => path.Contains(pathIfObjectInBundle));

                        if (matchingPath != null)
                        {
                            isScene = true;
                            pathIfObjectInBundle = matchingPath;
                        }
                    }
                }

                if (isAsset)
                {
                    Object asset = data.assets[pathIfObjectInBundle];

                    if (asset is Material mat)
                    {
                        return (new Material(mat));
                    }
                    else
                    {
                        return (asset);
                    }
                }
                else if (isScene)
                {
                    return data.scenes[pathIfObjectInBundle];
                }
                else
                {
                    UMI3DLogger.LogWarning($"Path {pathIfObjectInBundle} not found in bundle assets or scenes.\n Available assets were {data.assets.Keys.ToString<string>()}\nAvailable scenes were {data.scenes.Keys.ToString<string>()}", scope);
                    return (o);
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
            List<int> alreadyLoaded = new();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);

                if (sc.path == scenePath)
                {
                    alreadyLoaded.Add(i);
                }
            }

            UnityEngine.AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            while (!asyncLoading.isDone)
                await UMI3DAsyncManager.Yield();

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);

                if (sc.path == scenePath && !alreadyLoaded.Contains(i))
                {
                    scene = sc;
                    break;
                }
            }

            GameObject sceneObj = new GameObject(scenePath);

            if (UMI3DResourcesManager.Exists)
                sceneObj.transform.SetParent(UMI3DResourcesManager.Instance.transform, true);

            await UMI3DAsyncManager.Yield();

            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                obj.transform.SetParent(sceneObj.transform);

                foreach (Camera cam in obj.GetComponentsInChildren<Camera>())
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
        public async void DeleteObject(object objectLoaded, string reason)
        {
            try
            {
                if (objectLoaded is BundleCacheData bundleCacheData)
                {
                    foreach (Object obj in bundleCacheData.assets.Values)
                    {
                        Object.DestroyImmediate(obj, true);
                    }

                    // Scenes assets are unloaded elsewhere
                    foreach ((GameObject root, Scene scene) in bundleCacheData.scenes.Values)
                    {
                        if (scene.isLoaded)
                        {
                            UnityEngine.AsyncOperation op = SceneManager.UnloadSceneAsync(scene);

                            while (!op.isDone)
                                await UMI3DAsyncManager.Yield();
                        }

                        if (root)
                            Object.Destroy(root);
                    }

                    bundleCacheData.assets.Clear();
                    bundleCacheData.scenes.Clear();
                }

                Resources.UnloadUnusedAssets();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}