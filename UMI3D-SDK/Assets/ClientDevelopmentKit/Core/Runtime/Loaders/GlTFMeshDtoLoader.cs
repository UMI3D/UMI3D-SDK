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

using GLTFast;
using GLTFast.Loading;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// MeshDto loader for gltf
    /// </summary>
    public class GlTFMeshDtoLoader : AbstractMeshDtoLoader
    {

        public float maxTimePerFrame = 0.02f;
        private static GlTFMeshDtoLoader instance;// = new ObjMeshDtoLoader();

        /// <summary>
        /// Constructor.
        /// </summary>
        public GlTFMeshDtoLoader()
        {
            if (instance == null)
            {
                instance = this;
                instance.supportedFileExtentions = new List<string>() { ".gltf", ".glb" };
                instance.ignoredFileExtentions = new List<string>() { ".bin" };
            }
        }

        /// <summary>
        /// Set Certificate for a webRequest
        /// </summary>
        /// <param name="www">web request</param>
        /// <param name="authorization">Authorization</param>
        public static void SetCertificateAuthorization(UnityWebRequest www, string authorization)
        {
            if (instance == null)
                new GlTFMeshDtoLoader();
            instance.SetCertificate(www, authorization);
        }


        ///<inheritdoc/>
        public override void UrlToObject(string url, string extension, string authorization, Action<object> callback, Action<string> failCallback, string pathIfObjectInBundle = "")
        {
            GameObject createdObj = new GameObject();
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
                    catch
                    {
                        failCallback("Importing failed with : " + url);
                    }
                }
                else
                {
                    failCallback("Importing failed with : " + url);
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
                authorization = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(authorization));

                HttpHeader authorizationHeader = new HttpHeader
                {
                    Key = "AUTHORIZATION",
                    Value = "Basic " + authorization
                };

                HttpHeader[] headers = new HttpHeader[] { authorizationHeader };
                CustomHeaderDownloadProvider customHeaderDownloadProvider = new CustomHeaderDownloadProvider(headers);
                gltfComp.Load(url, customHeaderDownloadProvider, deferAgent, materialGenerator);
            }
            else
            {
                gltfComp.Load(url, null, deferAgent, materialGenerator);
            }
        }

        ///<inheritdoc/>
        public override Vector3 GetRotationOffset()
        {
            return new Vector3(0, 180, 0);
        }

    }
}


