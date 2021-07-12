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

using AsImpL;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Mesh loader for .obj
    /// </summary>
    public class ObjMeshDtoLoader : AbstractMeshDtoLoader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjMeshDtoLoader()
        {
            supportedFileExtentions = new List<string>() { ".obj" };
            ignoredFileExtentions = new List<string>() { ".mtl" };
        }

        ///<inheritdoc/>
        public override void UrlToObject(string url, string extension, string authorization, Action<object> callback, Action<string> failCallback, string pathIfObjectInBundle = "")
        {
            GameObject createdObj = new GameObject();

            ObjectImporter objImporter = createdObj.AddComponent<ObjectImporter>();
            ImportOptions importOptions = CreateImportOption(authorization);
            MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(
                UMI3DEnvironmentLoader.Instance.GetBaseMaterialBeforeAction(
                    (m) =>
                    {
                        objImporter.ImportModelAsync(System.IO.Path.GetFileNameWithoutExtension(url), url, createdObj.transform /*UMI3DResourcesManager.Instance.gameObject.transform*/, importOptions, m);



                        objImporter.ImportingComplete += () =>
                        {
                            try
                            {
                                HideModelRecursively(createdObj);

                                Transform newModel = objImporter.transform.GetChild(0);
                                newModel.SetParent(UMI3DResourcesManager.Instance.transform);
                                callback.Invoke(newModel.gameObject);
                            }
                            catch
                            {
                                failCallback("Importing failed with : " + url);
                            }
                            GameObject.Destroy(objImporter.gameObject, 1);
                        };

                    }));


        }

        /// <summary>
        /// Create Import Option.
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns></returns>
        private ImportOptions CreateImportOption(string authorization)
        {
            return new ImportOptions()
            {
                localPosition = UMI3DResourcesManager.Instance.transform.position,
                localEulerAngles = UMI3DResourcesManager.Instance.transform.eulerAngles + rotOffset,
                localScale = UMI3DResourcesManager.Instance.transform.lossyScale,
                authorization = authorization,
                authorizationName = common.UMI3DNetworkingKeys.Authorization,
                zUp = false,
                hideWhileLoading = true,

            };
        }

        private Vector3 rotOffset = new Vector3(0, 180, 0);
    }
}

