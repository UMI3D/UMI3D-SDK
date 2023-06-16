/*
Copyright 2019 - 2023 Inetum
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
    /// Describes a Webview
    /// </summary>
    [System.Serializable]
    public class UMI3DWebViewDto : UMI3DNodeDto, IEntity
    {
        /// <summary>
        /// Can users interact with the webview .
        /// </summary>
        public bool canInteract { get; set; }

        /// Webview size.
        public SerializableVector2 size { get; set; }

        /// Webview texture dimensions.
        public SerializableVector2 textureSize { get; set; }

        /// <summary>
        /// Url to load on clients.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// Synchronises all webview from a master view.
        /// </summary>
        public bool syncView { get; set; }
    }
}