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
using GLTFast.Materials;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public override async Task<object> UrlToObject(string url, string extension, string authorization, string pathIfObjectInBundle = "")
        {
#if UNITY_ANDROID
            if (!url.Contains("http")) url = "file://" + url;
#endif

            bool finished = false;
            object result = null;
            Exception e = null;
            Action<object> callback = (o) => { result = o; finished = true; };
            Action<Exception> failCallback = (o) => { e = o; finished = true; };

            var createdObj = new GameObject();
            GltfAsset gltfComp = createdObj.AddComponent<GltfAsset>();

            IDeferAgent deferAgent = new MaxTimePerFrameDeferAgent(maxTimePerFrame); // new UninterruptedDeferAgent();
            IMaterialGenerator materialGenerator = new GltfastCustomMaterialGenerator();

            if (authorization != null && authorization != "")
            {
                var authorizationHeader = new HttpHeader
                {
                    key = common.UMI3DNetworkingKeys.Authorization,
                    value = authorization
                };

                var headers = new HttpHeader[] { authorizationHeader };
                var customHeaderDownloadProvider = new CustomHeaderDownloadProvider(headers);

                MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(WaitBaseMaterial(async () =>
                {
                    bool success = await gltfComp.Load(url, customHeaderDownloadProvider, deferAgent, materialGenerator);

                    if (success)
                    {
                        //gltfComp.importer.InstantiateMainScene(createdObj.transform);
                        try
                        {
                            HideModelRecursively(createdObj);

                            Transform newModel = gltfComp.transform;//.GetChild(0);
                            newModel.name = newModel.GetChild(0).name;
                            newModel.SetParent(UMI3DResourcesManager.Instance.transform);
                            newModel.localPosition = Vector3.zero;
                            newModel.localEulerAngles += GetRotationOffset();
                            newModel.gameObject.SetActive(true);
                            callback.Invoke(newModel.gameObject);
                        }


                        catch (Exception error)
                        {
                            failCallback(new Umi3dNetworkingException(0, error.Message, url, "Importing failed for "));
                        }
                    }
                    else
                    {
                        failCallback(new Umi3dException($"Importing failed for { url } \nLoad failed"));
                    }
                    //GameObject.Destroy(gltfComp.gameObject, 1);
                }));
            }
            else
            {
                MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(WaitBaseMaterial(async () =>
                {
                    bool success = await gltfComp.Load(url, null, deferAgent, materialGenerator);

                    if (success)
                    {
                        //gltfComp.importer.InstantiateMainScene(createdObj.transform);
                        try
                        {
                            HideModelRecursively(createdObj);

                            Transform newModel = gltfComp.transform;//.GetChild(0);
                            newModel.name = newModel.GetChild(0).name;
                            newModel.SetParent(UMI3DResourcesManager.Instance.transform);
                            newModel.localPosition = Vector3.zero;
                            newModel.localEulerAngles += GetRotationOffset();
                            newModel.gameObject.SetActive(true);
                            callback.Invoke(newModel.gameObject);
                        }
                        catch (Exception error)
                        {
                            failCallback(new Umi3dNetworkingException(0, error.Message, url, "Importing failed for "));
                        }
                    }
                    else
                    {
                        failCallback(new Umi3dException($"Importing failed for { url } \nLoad failed"));
                    }
                    //GameObject.Destroy(gltfComp.gameObject, 1);
                }));
            }

            while (!finished)
                await UMI3DAsyncManager.Yield();
            if (e != null)
                throw e;

            return result;

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

    }
}
