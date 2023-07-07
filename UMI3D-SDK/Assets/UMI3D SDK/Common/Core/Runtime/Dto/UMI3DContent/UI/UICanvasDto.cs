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
    /// DTO describing a Canvas for UI.
    /// </summary>
    [System.Serializable]
    public class UICanvasDto : UIRectDto
    {
        /// <summary>
        /// Number of pixels par unit in the UI to use for dynamically created elements.
        /// </summary>
        /// Such dynamically created elements typically includes bitmaps, such as UI texts.
        public float dynamicPixelsPerUnit { get; set; }

        /// <summary>
        /// Number of pixels that should correspond to one unit in the UI.
        /// </summary>
        public float referencePixelsPerUnit { get; set; }

        /// <summary>
        /// Order of the canvas in the rendering of the layer.
        /// </summary>
        public int orderInLayer { get; set; }
    }
}