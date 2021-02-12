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

using System;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// ResourcesLoader for mesh object.
    /// </summary>
    public abstract class AbstractMeshDtoLoader : IResourcesLoader
    {

        public List<string> supportedFileExtentions;
        public List<string> ignoredFileExtentions;

        /// <summary>
        /// Set web request Certificate.
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

        /// <see cref="IResourcesLoader.UrlToObject"/>
        public abstract void UrlToObject(string url, string extension, string authorization, Action<object> callback, Action<string> failCallback, string pathIfObjectInBundle = "");

        /// <summary>
        /// Return the object itself because its not in a bundle.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="callback"></param>
        /// <param name="pathIfObjectInBundle"></param>
        /// <see cref="IResourcesLoader.ObjectFromCache"/>
        public virtual void ObjectFromCache(object o, Action<object> callback, string pathIfObjectInBundle)
        {
            callback.Invoke(o);
        }

        /// <summary>
        /// Spread recursively transform layer to all is childrens.
        /// </summary>
        /// <param name="transform">Transform</param>
        public static void ApplyParentLayerInChildren(Transform transform)
        {
            int layer = transform.gameObject.layer;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = layer;
                ApplyParentLayerInChildren(child);
            }
        }

        /// <summary>
        /// Hide Model Recursively
        /// </summary>
        /// <param name="go"></param>
        public static void HideModelRecursively(GameObject go)
        {
            int invisibleLayer = LayerMask.NameToLayer("Invisible");
            if (invisibleLayer == -1)
            {
                Debug.LogWarning("Invisible Layer Not found \n Models in cache are not hidden \n You should add 'Invisible' layer in client project");
            }
            else
            {
                go.layer = invisibleLayer;
                ApplyParentLayerInChildren(go.transform);
            }
        }

        /// <summary>
        /// Show model recursivly
        /// </summary>
        /// <param name="go"></param>
        public static void ShowModelRecursively(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Default");
            ApplyParentLayerInChildren(go.transform);
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


        /// <see cref="IResourcesLoader.DeleteObject"/>
        public void DeleteObject(object objectLoaded, string reason)
        {
            GameObject.Destroy(objectLoaded as UnityEngine.Object);
        }

        public virtual Vector3 GetRotationOffset()
        {
            return Vector3.one;
        }


    }
}
