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
    public enum LoadingSourceEnum
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

    [Serializable]
    public struct SerializedAddressable<T>
        where T: UnityEngine.Object
    {
        [Tooltip("Choose how you want to load the asset")]
        public LoadingSourceEnum loadingSource;

        [ShowWhenEnum(nameof(loadingSource), new[] { (int)LoadingSourceEnum.Reference })]
        public AssetReferenceT<T> reference;
        [ShowWhenEnum(nameof(loadingSource), new[] { (int)LoadingSourceEnum.Address })]
        public string address;

        /// <summary>
        /// The <see cref="AsyncOperationHandle"/> set when the load method is called.
        /// </summary>
        [HideInInspector]
        public AsyncOperationHandle<T> operationHandler;

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
                case LoadingSourceEnum.Reference:
                    if (reference.RuntimeKeyIsValid())
                    {
                        operationHandler = reference.LoadAssetAsync();
                    }
                    else
                    {
                        throw new SerializedAddressableException($"Reference for type [{typeof(T).Name}] has an invalid RuntimeKey");
                    }
                    break;
                case LoadingSourceEnum.Address:
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

        public class SerializedAddressableException: Exception
        {
            public SerializedAddressableException(string message) : base(message)
            { }

            public SerializedAddressableException(string message, Exception innerException) : base(message, innerException)
            { }
        }
    }
}
