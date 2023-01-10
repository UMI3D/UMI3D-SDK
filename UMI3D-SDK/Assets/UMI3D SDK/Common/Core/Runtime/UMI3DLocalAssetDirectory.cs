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
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Serialized description of an asset directory, a local folder where variants of an assets are stored.
    /// </summary>
    [System.Serializable]
    public class UMI3DLocalAssetDirectory : IBytable
    {
        /// <summary>
        /// Name of the directory.
        /// </summary>
        public string name = "new variant";

        /// <summary>
        /// Local path of the directory.
        /// </summary>
        public string path;
        [SerializeField]
        public AssetMetricDto metrics = new AssetMetricDto();
        [ConstEnum(typeof(UMI3DAssetFormat), typeof(string))]
        public List<string> formats = new List<string>();

        public UMI3DLocalAssetDirectory()
        {
        }

        public UMI3DLocalAssetDirectory(UMI3DLocalAssetDirectory other)
        {
            this.name = other.name;
            this.path = other.path;
            this.metrics = other.metrics;
            this.formats = other.formats;
        }

        /// <inheritdoc/>
        bool IBytable.IsCountable()
        {
            return true;
        }

        /// <inheritdoc/>
        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            return
                UMI3DSerializer.Write(name)
                + UMI3DSerializer.Write(path)
                + UMI3DSerializer.Write(metrics.resolution)
                + UMI3DSerializer.Write(metrics.size)
                + UMI3DSerializer.WriteCollection(formats);
        }
        //public List<string> dependencies = new List<string>();
    }
}