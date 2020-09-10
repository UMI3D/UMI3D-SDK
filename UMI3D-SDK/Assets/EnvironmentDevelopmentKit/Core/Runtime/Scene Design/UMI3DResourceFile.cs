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

using umi3d.common;

namespace umi3d.edk
{

    [System.Serializable]
    public class UMI3DResourceFile
    {
        public bool isLocalFile = false;
        public string domain = "";
        public string path = "";
        public string format;
        public string extension;
        public bool isInBundle = false;
        public string pathIfInBundle = "";
        public AssetMetricDto metrics;

        public bool isInLibrary = false;
        public string libraryKey = "";

        public FileDto ToDto()
        {
            var dto = new FileDto();
            dto.url = GetUrl();
            dto.format = format;
            dto.extension = extension;
            dto.metrics = metrics;
            dto.pathIfInBundle = isInBundle ? pathIfInBundle : null;
            dto.libraryKey = isInLibrary ? libraryKey : null;
            return dto;
        }
        public string GetUrl()
        {
            path = path.Replace(@"\", "/");
            if (path != null && path != "" && !(path.StartsWith("/") /*|| Path.StartsWith(@"\")*/))
            {
                path = "/" + path;
            }
            if (isLocalFile)
                return umi3d.Path.Combine(UMI3DServer.GetHttpUrl(),UMI3DNetworkingKeys.files, path);
            else
                return umi3d.Path.Combine(domain,path);
        }
        
    }
}