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
using UnityEngine;

namespace umi3d.cdk
{
    public class OBJExtensionLoader : AbstractExtensionLoader
    {

        public override bool IsSuitable(string extension)
        {
            return true;
        }

        public override void Load(string extension, Resource resource, GameObject pivot, Transform parent, Action<GameObject> onSuccess, Action<string> onError)
        {
            var importer = pivot.AddComponent<ObjectImporter>();
            var options = new AsImpL.ImportOptions();
            options.localPosition = Vector3.zero;
            options.localEulerAngles = new Vector3(0f, 0f, 0f);
            options.localScale = Vector3.one;
            options.localPosition = Vector3.zero;
            options.buildColliders = false;
            importer.ImportingComplete += () =>
            {
                if (HDResourceCache.Instance.modelCache.ContainsKey(resource.Url) && HDResourceCache.Instance.modelCache[resource.Url] != null)
                {
                    Destroy(pivot);
                }
                else
                {
                    HDResourceCache.Instance.modelCache[resource.Url] = pivot;
                    pivot.SetActive(false);
                }
                HDResourceCache.Instance.InstanciateFromCache(resource.Url, parent, onSuccess, name);

                if (HDResourceCache.Instance.modelCallbackCache.ContainsKey(resource.Url))
                {
                    foreach (HDResourceCache.ModelCache model in HDResourceCache.Instance.modelCallbackCache[resource.Url])
                    {
                        HDResourceCache.Instance.InstanciateFromCache(resource.Url, model.transform, model.callback, model.name);
                    }
                    HDResourceCache.Instance.modelCallbackCache.Remove(resource.Url);
                }
            };
            importer.ImportError += path => { onError.Invoke("error obj extension on path "+path); };
            importer.ImportModelAsync(name, resource, pivot.transform, options);
        }
    }
}