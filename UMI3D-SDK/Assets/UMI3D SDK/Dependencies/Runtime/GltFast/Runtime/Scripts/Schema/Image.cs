﻿// Copyright 2020 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

namespace GLTFast.Schema
{
    [System.Serializable]
    public class Image : RootChild
    {
        /// <summary>
        /// The uri of the image.  Relative paths are relative to the .gltf file.
        /// Instead of referencing an external file, the uri can also be a data-uri.
        /// The image format must be jpg, png, bmp, or gif.
        /// </summary>
        public string uri;

        /// <summary>
        /// The image's MIME type.
        /// <minLength>1</minLength>
        /// </summary>
        public string mimeType;

        /// <summary>
        /// The index of the bufferView that contains the image.
        /// Use this instead of the image's uri property.
        /// </summary>
        public int bufferView = -1;

#if KTX_UNITY
        public ImageExtension extensions;
#endif
    }
}
