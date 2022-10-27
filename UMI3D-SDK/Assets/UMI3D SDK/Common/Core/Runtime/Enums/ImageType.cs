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
    /// Image display type in UMI3D.
    /// </summary>
    public enum ImageType
    {
        /// <summary>
        /// Full image display.
        /// </summary>
        Simple = 0,

        /// <summary>
        /// 9-sliced image display.
        /// </summary>
        Sliced = 1,

        /// <summary>
        /// Similar to <see cref="Sliced"/> but the resizable parts of the image are repeated.
        /// </summary>
        Tiled = 2,

        /// <summary>
        /// Section of the image display. Left portion is transparent.
        /// </summary>
        Filled = 3
    }
}