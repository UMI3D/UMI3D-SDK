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

namespace umi3d.common
{
    /// <summary>
    /// DTO describing an asset file.
    /// </summary>
    [System.Serializable]
    public class FileDto : UMI3DDto
    {
        /// <summary>
        /// Relative path of the file in the data folder.
        /// </summary>
        public string url;

        /// <summary>
        /// relative sub-path if the file is packaged in an Asset Bundle.
        /// </summary>
        /// For Unity, see <see cref="UnityEngine.AssetBundle"/>.
        public string pathIfInBundle;

        /// <summary>
        /// Unique name of the library if the assets belongs to an asset library.
        /// </summary>
        public string libraryKey;

        /// <summary>
        /// Format used to write the file.
        /// </summary>
        public string format;

        /// <summary>
        /// Extension of the file.
        /// </summary>
        public string extension;

        /// <summary>
        /// Metrics measuring this asset.
        /// </summary>
        public AssetMetricDto metrics;

        /// <summary>
        /// Authorization header for HTTP authentification scheme.
        /// </summary>
        public string authorization;

        public FileDto() : base() { }
    }
}