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

using GLTFast;
using GLTFast.Loading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// MeshDto loader for gltf
    /// </summary>
    public class GlTFMeshDtoLoader : AbstractMeshDtoLoader
    {

        public float maxTimePerFrame = 0.02f;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GlTFMeshDtoLoader()
        {
            supportedFileExtentions = new List<string>() { ".gltf", ".glb" };
            ignoredFileExtentions = new List<string>() { ".bin" };
        }


        ///<inheritdoc/>
        public override void UrlToObject(string url, string extension, string authorization, Action<object> callback, Action<Umi3dException> failCallback, string pathIfObjectInBundle = "")
        {
#if UNITY_ANDROID
            if (!url.Contains("http")) url = "file://" + url;
#endif

            var createdObj = new GameObject();
            GltfAssetBase gltfComp = createdObj.AddComponent<GltfAssetBase>();

            gltfComp.onLoadComplete += (x, b) =>
            {
                if (b)
                {
                    gltfComp.gLTFastInstance.InstantiateGltf(createdObj.transform);
                    try
                    {
                        HideModelRecursively(createdObj);

                        Transform newModel = gltfComp.transform.GetChild(0);
                        newModel.SetParent(UMI3DResourcesManager.Instance.transform);
                        newModel.localPosition = Vector3.zero;
                        newModel.localEulerAngles += GetRotationOffset();
                        newModel.gameObject.SetActive(true);
                        callback.Invoke(newModel.gameObject);

                    }
                    catch (Exception e)
                    {
                        failCallback(new Umi3dException(e, "Importing failed for " + url));
                    }
                }
                else
                {
                    failCallback(new Umi3dException($"Importing failed for {url} \nLoad failed"));
                }
                GameObject.Destroy(gltfComp.gameObject, 1);
            };
            //gltfComp.GLTFUri = url;
            //gltfComp.UseStream = false;
            //gltfComp.Multithreaded = true;
            //gltfComp.RetryCount = 2;
            //gltfComp.LoadAndRetry();

            IDeferAgent deferAgent = new MaxTimePerFrameDeferAgent(maxTimePerFrame); // new UninterruptedDeferAgent();
            IMaterialGenerator materialGenerator = new GltfastCustomMaterialGenerator();

            if (authorization != null && authorization != "")
            {
                var headers = new HttpHeader[] { };

                if (!UMI3DClientServer.Instance.AuthorizationInHeader && url.StartsWith("http"))
                {
                    url = UMI3DResourcesManager.Instance.SetAuthorisationWithParameter(url, authorization);
                }
                else
                {
                    var authorizationHeader = new HttpHeader
                    {
                        Key = common.UMI3DNetworkingKeys.Authorization,
                        Value = authorization
                    };
                    headers = new HttpHeader[] { authorizationHeader };
                }

                var customHeaderDownloadProvider = new CustomHeaderDownloadProvider(headers);
                MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(WaitBaseMaterial(() => gltfComp.Load(url, customHeaderDownloadProvider, deferAgent, materialGenerator)));
            }
            else
            {
                MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(WaitBaseMaterial(() =>
               gltfComp.Load(url, null, deferAgent, materialGenerator)));
            }
        }

        /// <summary>
        /// wait in coroutine the initialization of DefaultMaterial, then invoke the callback
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator WaitBaseMaterial(Action callback)
        {
            yield return new WaitWhile(() => UMI3DEnvironmentLoader.Instance.baseMaterial == null);
            callback.Invoke();
        }

        ///<inheritdoc/>
        public override Vector3 GetRotationOffset()
        {
            return new Vector3(0, 180, 0);
        }
    }
}


