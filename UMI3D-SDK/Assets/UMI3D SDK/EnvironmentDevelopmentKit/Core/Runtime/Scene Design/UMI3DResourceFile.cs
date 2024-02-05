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
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// File info for a <see cref="UMI3DResource"/>.
    /// </summary>
    [System.Serializable]
    public class UMI3DResourceFile
    {
        public bool isLocalFile = false;
        public string domain = "";
        public string path = "";
        [ConstEnum(typeof(UMI3DAssetFormat), typeof(string))]
        public string format;
        public string extension;
        public bool isInBundle = false;
        public string pathIfInBundle = "";
        public AssetMetric metrics = new AssetMetric();

        public bool isInLibrary = false;
        public AssetLibrary libraryKey = null;

        /// <inheritdoc/>
        public FileDto ToDto()
        {
            var dto = new FileDto
            {
                url = GetUrl(),
                format = format,
                extension = extension,
                metrics = metrics.ToDto(),
                pathIfInBundle = isInBundle ? pathIfInBundle : null,
                libraryKey = isInLibrary ? libraryKey?.idVersion : null
            };
            return dto;
        }

        /// <inheritdoc/>
        public Bytable ToByte()
        {
            return UMI3DSerializer.Write(GetUrl())
                + UMI3DSerializer.Write(format)
                + UMI3DSerializer.Write(extension)
                + UMI3DSerializer.Write(metrics.resolution)
                + UMI3DSerializer.Write(metrics.size)
                + UMI3DSerializer.Write(isInBundle ? pathIfInBundle : null)
                + UMI3DSerializer.Write(isInLibrary ? libraryKey?.idVersion : null);
        }


        public string GetUrl()
        {
            path = path.Replace(@"\", "/");
            if (path != null && path != "" && !path.StartsWith("/") /*|| Path.StartsWith(@"\")*/)
            {
                path = "/" + path;
            }
            if (isLocalFile)
                return System.Uri.EscapeUriString(Path.Combine(UMI3DServer.GetResourcesUrl(), UMI3DNetworkingKeys.files, path));
            else
                return System.Uri.EscapeUriString(Path.Combine(domain, path));
        }
    }
}