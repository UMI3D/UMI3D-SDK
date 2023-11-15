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

namespace inetum.unityUtils
{
    /// <summary>
    /// Enumeration to choose between an assetReference or an address when trying to load an asset via the Addressables package.
    /// </summary>
    public enum AddressableLoadingSourceEnum
    {
        /// <summary>
        /// Use an <see cref="AssetReference"/> to load the asset.
        /// </summary>
        Reference,
        /// <summary>
        /// Use the address of the asset to load the asset.
        /// </summary>
        Address
    }
}
