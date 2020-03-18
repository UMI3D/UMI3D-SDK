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
using UnityEngine;
using UnityGLTF;

namespace umi3d.cdk
{
    public class GLTFExtensionLoader : AbstractExtensionLoader
    {

        public override bool IsSuitable(string extension)
        {
            return extension == ".gltf" || extension == ".glb";
        }

        public override void Load(string extension, Resource resource, GameObject pivot, Transform parent, Action<GameObject> onSuccess, Action<string> onError)
        {
            var gltf_component = pivot.AddComponent<GLTFComponent>();
            //TODO IF RESOURCE FILE NOT LOCAL
            gltf_component.GLTFUri = resource.Url;
            gltf_component.MaximumLod = 300;
            gltf_component.Collider = GLTFSceneImporter.ColliderType.Mesh;
            gltf_component.Load();
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
        }
    }
}