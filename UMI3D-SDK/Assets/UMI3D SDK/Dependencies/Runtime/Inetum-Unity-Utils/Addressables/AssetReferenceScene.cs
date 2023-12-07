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
using UnityEngine.AddressableAssets;

namespace inetum.unityUtils
{
    /// <summary>
    /// An <see cref="AssetReference"/> that accept only scenes.
    /// </summary>
    [System.Serializable]
    public class AssetReferenceScene : AssetReference
    {
        public override bool ValidateAsset(string path)
        {
            return path.EndsWith(".unity");
        }
    }
}
