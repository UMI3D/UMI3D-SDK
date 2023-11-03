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
using System.Collections;
using System.Collections.Generic;
using umi3d.debug;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace inetum.unityUtils
{
    public static class AddressablesExtensions
    {
        /// <summary>
        /// Loads async the gameObject and instantiates it.
        /// 
        /// <para>
        /// Doing so will increase the unity count of loading and instantiating. When this count reaches 0 the bundle will be unloaded resulting in undetermined behaviour if a gameObject is still present. So to avoid this issue use releaseInstance in pair with instantiateAsync.
        /// </para>
        /// </summary>
        /// <param name="addressable"></param>
        /// <param name="parent"></param>
        /// <param name="instantiateInWorldSpace"></param>
        /// <returns></returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(this SerializedAddressable<GameObject> addressable, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            AsyncOperationHandle<GameObject> result;
            switch (addressable.loadingSource)
            {
                case LoadingSourceEnum.Reference:
                    result = addressable.reference.InstantiateAsync(parent, instantiateInWorldSpace);
                    break;
                case LoadingSourceEnum.Address:
                    result = Addressables.InstantiateAsync(addressable.address, parent, instantiateInWorldSpace);
                    break;
                default:
                    result = default;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Release a gameObject and decrease the unity loading and instantiating count.
        /// </summary>
        /// <param name="addressable"></param>
        public static void ReleaseInstance(this SerializedAddressable<GameObject> addressable)
        {
            if (addressable.IsHandlerValid)
            {
                Addressables.ReleaseInstance(addressable.operationHandler);
            }
        }

        /// <summary>
        /// Execute the <paramref name="operation"/> now if the AsyncOperationHandle has been completed. Else execute when completed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addressable"></param>
        /// <param name="operation"></param>
        public static void NowOrLater<T>(this SerializedAddressable<T> addressable, Action<T> operation)
            where T : UnityEngine.Object
        {
            addressable.operationHandler.NowOrLater(operation);
        }

        /// <summary>
        /// Execute the <paramref name="operation"/> now if the AsyncOperationHandle has been completed. Else execute when completed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="operation"></param>
        public static void NowOrLater<T>(this AsyncOperationHandle<T> handler, Action<T> operation)
        {
            if (!handler.IsValid())
            {
                UnityEngine.Debug.LogError($"The handler is not valide: may try to access it before initialisation or after termination.");
                return;
            }

            void _operation(AsyncOperationHandle<T> handler)
            {
                if (handler.Status == AsyncOperationStatus.Succeeded)
                {
                    operation(handler.Result);
                }
                else
                {
                    var logger = new UMI3DLogger(mainTag: "AsyncOperationHandleExtensions");
                    logger.Error($"{nameof(NowOrLater)}", $"handler result failed.");
                }
            }

            if (handler.IsDone)
            {
                _operation(handler);
            }
            else
            {
                handler.Completed += _operation;
            }
        }
    }
}
