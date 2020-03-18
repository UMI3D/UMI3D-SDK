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
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    public class BundleExtensionLoader : AbstractExtensionLoader
    {
        public override bool IsSuitable(string extension)
        {
            return extension == ".bundle";
        }

        public override void Load(string extension, Resource resource, GameObject pivot, Transform parent, Action<GameObject> onSuccess, Action<string> onError)
        {
            pivot.transform.localRotation = Quaternion.identity;
            StartCoroutine(LoadAssetBundle(resource, parent, pivot, onSuccess, onError, name));
        }

        static IEnumerator LoadAssetBundle(Resource resource, Transform parent, GameObject pivot, Action<GameObject> onSuccess, Action<string> onError, string name = "model")
        {
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(resource.Url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                onError?.Invoke(www.error);
                Destroy(pivot);
                Debug.Log(www.error);
                yield break;
            }
            else
            {
                if (HDResourceCache.Instance.modelCache.ContainsKey(resource.Url) && HDResourceCache.Instance.modelCache[resource.Url] != null)
                {
                    Destroy(pivot);
                }
                else
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                    //bundle.
                    //var names = bundle.GetAllAssetNames();
                    UnityEngine.Object[] objs = bundle.LoadAllAssets();
                    foreach (UnityEngine.Object obj in objs)
                    {
                        //Debug.Log("OBJ: " + obj.name);
                        if (obj is GameObject)
                        {
                            GameObject go = Instantiate<GameObject>(obj as GameObject, pivot.transform);
                            //go.transform.localPosition = Vector3.zero;
                            //go.transform.localRotation = Quaternion.identity;
                            //go.transform.localScale = Vector3.one;
                            /*Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                            foreach (var rend in renderers)
                            {
                                Material[] materials = rend.sharedMaterials;
                                string[] shaders = new string[materials.Length];

                                for (int i = 0; i < materials.Length; i++)
                                {
                                    shaders[i] = materials[i].shader.name;
                                }

                                for (int i = 0; i < materials.Length; i++)
                                {
                                    Debug.Log("Shader: " + shaders[i]);
                                    //materials[i].shader = Shader.Find(shaders[i]);
                                    //materials[i].shader = Shader.Find("Standard");
                                }
                            }*/
                        }
                    }

                    bundle.Unload(false);
                    HDResourceCache.Instance.modelCache[resource.Url] = pivot;
                    pivot.SetActive(false);
                }
                www.Dispose();
                HDResourceCache.Instance.InstanciateFromCache(resource.Url, parent, onSuccess, name);

                if (HDResourceCache.Instance.modelCallbackCache.ContainsKey(resource.Url))
                {
                    foreach (HDResourceCache.ModelCache model in HDResourceCache.Instance.modelCallbackCache[resource.Url])
                    {
                        HDResourceCache.Instance.InstanciateFromCache(resource.Url, model.transform, model.callback, model.name);
                    }
                    HDResourceCache.Instance.modelCallbackCache.Remove(resource.Url);
                }

            }

        }
    }
}