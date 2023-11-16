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
using umi3d.debug;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace inetum.unityUtils
{
    public struct AOHStack<T>
    {
        static UMI3DLogger logger = new(mainTag: nameof(AOHStack<T>));

        /// <summary>
        /// Event raised when 
        /// </summary>
        public event Action<AsyncOperationHandle<T>[]> completed
        {
            add
            {
                if (hasBeenCompleted)
                {
                    value?.Invoke(stack);
                }
                else
                {
                    _completed += value;
                }
            }
            remove
            {
                _completed -= value;
            }
        }
        event Action<AsyncOperationHandle<T>[]> _completed;

        AsyncOperationHandle<T>[] stack;
        int aohCount;


        bool hasBeenCompleted
        {
            get
            {
                return aohCount == 0;
            }
        }

        public AOHStack(AsyncOperationHandle<T>[] stack)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            this.stack = stack;
            this.aohCount = stack.Length;
            this._completed = default;

            var aohStack = this;
            foreach (var aoh in stack)
            {
                if (aoh.IsValid() && !aoh.IsDone)
                {
                    aoh.Completed += _ =>
                    {
                        if (aohStack.CheckIfAllAOHAreCompleted())
                        {
                            aohStack.RaiseCompleted();
                        }
                    };
                }
                else
                {
                    aohCount--;
                }
            }

            if (aohCount == 0)
            {
                RaiseCompleted();
            }
        }

        bool CheckIfAllAOHAreCompleted()
        {
            return aohCount == 0;

            UnityEngine.Debug.Log($"message");
            if (hasBeenCompleted)
            {
                return true;
            }

            foreach (var aoh in stack)
            {
                if (aoh.IsValid() && !aoh.IsDone)
                {
                    logger.Default(nameof(CheckIfAllAOHAreCompleted), "false");
                    return false;
                }
            }

            logger.Default(nameof(CheckIfAllAOHAreCompleted), "true");
            return true;
        }

        void RaiseCompleted()
        {
            if (hasBeenCompleted)
            {
                return;
            }

            hasBeenCompleted = true;
            this._completed?.Invoke(stack);
            UnityEngine.Debug.Log($"raise");
        }
    }
}
