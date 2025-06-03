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

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// Resource Loader for Image.
    /// </summary>
    public class ImageDtoLoader : IResourcesLoader
    {
        public List<string> supportedFileExtensions;
        public List<string> ignoredFileExtensions;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ImageDtoLoader()
        {
            this.supportedFileExtensions = new List<string>() { ".jpg", ".bmp", ".dib", ".rle", ".exr", ".gif", ".hdr", ".iff", ".jpeg", ".pict", ".pct", ".png", ".psd", ".tga", ".tif", ".tiff", ".TGA", ".PNG", ".JPG", ".JPEG" };
            this.ignoredFileExtensions = new List<string>();
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
#if UNITY_ANDROID
            using UnityWebRequest www = url.Contains("http") ? UnityWebRequestTexture.GetTexture(url) : UnityWebRequestTexture.GetTexture("file://" + url);
#else
            using UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
#endif
            LoaderUtils.SetWebRequestCertificate(www, authorization);

            await UMI3DResourcesManager.DownloadObject(www);

            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            return texture;
        }

        /// <see cref="IResourcesLoader.ObjectFromCache"/>
        public virtual Task<object> ObjectFromCache(object o, string pathIfObjectInBundle)
        {
            return Task.FromResult((o));
        }

        /// <inheritdoc/>
        public void DeleteObject(object objectLoaded, string reason)
        {
            GameObject.Destroy(objectLoaded as UnityEngine.Object);
        }
    }
}