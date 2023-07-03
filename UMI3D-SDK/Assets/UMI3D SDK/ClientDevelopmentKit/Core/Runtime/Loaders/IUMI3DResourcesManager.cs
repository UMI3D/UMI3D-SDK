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
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public interface IUMI3DResourcesManager
    {
        Transform CacheTransform { get; }
        void AddSubModels(string modelUrlInCache, UMI3DResourcesManager.SubmodelDataCollection nodes);
        void ClearCache(List<UMI3DResourcesManager.Library> exceptLibraries = null);
        void GetSubModel(string modelUrlInCache, string subModelName, List<int> indexes, List<string> names, Action<object> callback);
        Transform GetSubModelNow(string modelUrlInCache, string subModelName, List<int> indexes, List<string> names);
        Transform GetSubModelRoot(string modelUrlInCache);
        bool IsSubModelsSetFor(string modelUrlInCach);
        string SetAuthorisationWithParameter(string fileUrl, string authorization);
        List<string> _LibrariesToDownload(List<AssetLibraryDto> assetLibraries);
        Task<object> _LoadFile(ulong id, FileDto file, IResourcesLoader loader);
    }
}