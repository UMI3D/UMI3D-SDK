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
using UnityEngine;

namespace umi3d.common
{
    [System.Serializable]
    public class UMI3DLocalAssetDirectory : IBytable
    {
        public string name = "new variant";
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

        bool IBytable.IsCountable()
        {
            return true;
        }

        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            return
                UMI3DNetworkingHelper.Write(name)
                + UMI3DNetworkingHelper.Write(path)
                + UMI3DNetworkingHelper.Write(metrics.resolution)
                + UMI3DNetworkingHelper.Write(metrics.size)
                + UMI3DNetworkingHelper.WriteCollection(formats);
        }
        //public List<string> dependencies = new List<string>();
    }
}