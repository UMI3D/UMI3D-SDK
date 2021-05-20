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
using umi3d.common;

namespace umi3d.edk
{

    [System.Serializable]
    public class UMI3DResourceFile : IByte
    {
        public bool isLocalFile = false;
        public string domain = "";
        public string path = "";
        [ConstStringEnum(typeof(UMI3DAssetFormat))]
        public string format;
        public string extension;
        public bool isInBundle = false;
        public string pathIfInBundle = "";
        public AssetMetricDto metrics;

        public bool isInLibrary = false;
        public AssetLibrary libraryKey = null;

        public FileDto ToDto()
        {
            var dto = new FileDto();
            dto.url = GetUrl();
            dto.format = format;
            dto.extension = extension;
            dto.metrics = metrics;
            dto.pathIfInBundle = isInBundle ? pathIfInBundle : null;
            dto.libraryKey = isInLibrary ? libraryKey?.id : null;
            return dto;
        }

        public (int, Func<byte[], int, int>) ToByte()
        {
            int size =
                UMI3DNetworkingHelper.GetSize(GetUrl())
                + UMI3DNetworkingHelper.GetSize(format)
                + UMI3DNetworkingHelper.GetSize(extension)
                + UMI3DNetworkingHelper.GetSize(metrics.resolution)
                + UMI3DNetworkingHelper.GetSize(metrics.size)
                + UMI3DNetworkingHelper.GetSize(isInBundle ? pathIfInBundle : null)
                + UMI3DNetworkingHelper.GetSize(isInLibrary ? libraryKey?.id : null);
            Func<byte[], int, int> func = (b, i) =>
            {
                i += UMI3DNetworkingHelper.Write(GetUrl(), b, i);
                i += UMI3DNetworkingHelper.Write(format, b, i);
                i += UMI3DNetworkingHelper.Write(extension, b, i);
                i += UMI3DNetworkingHelper.Write(metrics.resolution, b, i);
                i += UMI3DNetworkingHelper.Write(metrics.size, b, i);
                i += UMI3DNetworkingHelper.Write(isInBundle ? pathIfInBundle : null, b, i);
                i += UMI3DNetworkingHelper.Write(isInLibrary ? libraryKey?.id : null, b, i);
                return size;
            };
            return (size, func);
        }

        public string GetUrl()
        {
            path = path.Replace(@"\", "/");
            if (path != null && path != "" && !(path.StartsWith("/") /*|| Path.StartsWith(@"\")*/))
            {
                path = "/" + path;
            }
            if (isLocalFile)
                return System.Uri.EscapeUriString(Path.Combine(UMI3DServer.GetHttpUrl(), UMI3DNetworkingKeys.files, path));
            else
                return System.Uri.EscapeUriString(Path.Combine(domain, path));
        }

        (int, Func<byte[], int, int>) IByte.ToByteArray(params object[] parameters)
        {
            return ToByte();
        }
    }
}