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
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace inetum.unityUtils
{
    [Serializable]
    public struct SerializedAddressableT<T>
        where T: UnityEngine.Object
    {
        [Tooltip("Choose how you want to load the asset")]
        public AddressableLoadingSourceEnum loadingSource;

        [ShowWhenEnum(nameof(loadingSource), new[] { (int)AddressableLoadingSourceEnum.Reference })]
        public AssetReferenceT<T> reference;
        [ShowWhenEnum(nameof(loadingSource), new[] { (int)AddressableLoadingSourceEnum.Address })]
        public string address;

        /// <summary>
        /// The <see cref="AsyncOperationHandle"/> set when the load method is called.
        /// </summary>
        [HideInInspector]
        public AsyncOperationHandle<T> operationHandler;

        /// <summary>
        /// Whether or not this <see cref="SerializedAddressableT{T}"/> has enough informations to load its asset.
        /// </summary>
        public bool IsValid
        {
            get
            {
                switch (loadingSource)
                {
                    case AddressableLoadingSourceEnum.Reference:
                        return reference.RuntimeKeyIsValid();
                    case AddressableLoadingSourceEnum.Address:
                        return !string.IsNullOrEmpty(address);
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Whether or not the handler has been set.
        /// 
        /// <para>
        /// A handler that has not been set cannot be used.
        /// </para>
        /// </summary>
        public bool IsHandlerValid
        {
            get
            {
                return operationHandler.IsValid();
            }
        }

        /// <summary>
        /// Load the asset in an asynchronous way.
        /// </summary>
        /// <exception cref="SerializedAddressableException"></exception>
        public AsyncOperationHandle<T> LoadAssetAsync()
        {
            switch (loadingSource)
            {
                case AddressableLoadingSourceEnum.Reference:
                    if (reference.RuntimeKeyIsValid())
                    {
                        operationHandler = reference.LoadAssetAsync();
                    }
                    else
                    {
                        throw new SerializedAddressableException($"Reference for type [{typeof(T).Name}] has an invalid RuntimeKey");
                    }
                    break;
                case AddressableLoadingSourceEnum.Address:
                    operationHandler = Addressables.LoadAssetAsync<T>(address);
                    break;
                default:
                    break;
            }

            return operationHandler;
        }

        public void Release()
        {
            if (operationHandler.IsValid())
            {
                Addressables.Release(operationHandler);
            }
        }
    }
}
