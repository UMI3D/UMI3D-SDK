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
using umi3d.debug;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace inetum.unityUtils
{
    public static class SAUtils
    {
        static UMI3DLogger logger = new(mainTag: nameof(SAUtils));

        public static AsyncOperationHandle<T>[] LoadAllAssetAsync<T>(params SerializedAddressableT<T>[] stack)
            where T: Object
        {
            AsyncOperationHandle<T>[] results = new AsyncOperationHandle<T>[stack.Length];
            for (int i = 0; i < stack.Length; i++)
            {
                if (stack[i].IsValid)
                {
                    results[i] = stack[i].LoadAssetAsync();
                }
                else
                {
                    logger.Error(nameof(LoadAllAssetAsync), $"[SA of type {typeof(T).Name}] Elt {i} is not valid");
                }
            }

            return results;
        }

        public static void ReleaseAll<T>(params SerializedAddressableT<T>[] stack)
            where T : Object
        {
            if (stack == null || stack.Length == 0)
            {
                return;
            }

            foreach (var sa in stack)
            {
                sa.Release();
            }
        }
    }
}
