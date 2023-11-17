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

        internal class AOHCount
        {
            public int aohValidCount;
            public int aohUndoneCount;

            public AOHCount(int stackLength)
            {
                aohValidCount = stackLength;
                aohUndoneCount = stackLength;
            }
        }

        internal class AOHEvent
        {
            public Action<AsyncOperationHandle<T>[]> _allCompleted = default;
            public bool allCompletedHasBeenRaised = false;
            public Action<AsyncOperationHandle<T>[]> _atLeastOneCompleted = default;
            public bool atLeastOneCompletedHasBeenRaised = false;
        }

        /// <summary>
        /// Event raised when all <see cref="AsyncOperationHandle{TObject}"/> are done.
        /// </summary>
        public event Action<AsyncOperationHandle<T>[]> allCompleted
        {
            add
            {
                if (AllAOHAreCompleted)
                {
                    value?.Invoke(stack);
                }
                else
                {
                    aohEvent._allCompleted += value;
                }
            }
            remove
            {
                aohEvent._allCompleted -= value;
            }
        }
        
        /// <summary>
        /// Event raised when at least one <see cref="AsyncOperationHandle{TObject}"/> are done.
        /// </summary>
        public event Action<AsyncOperationHandle<T>[]> atLeastOneCompleted
        {
            add
            {
                if (AtLeastOneAOHIsCompleted)
                {
                    value?.Invoke(stack);
                }
                else
                {
                    aohEvent._atLeastOneCompleted += value;
                }
            }
            remove
            {
                aohEvent._atLeastOneCompleted -= value;
            }
        }

        AsyncOperationHandle<T>[] stack;
        AOHEvent aohEvent;
        AOHCount count;

        public bool AllAOHAreCompleted
        {
            get
            {
                return count.aohUndoneCount == 0;
            }
        }

        public bool AtLeastOneAOHIsCompleted
        {
            get
            {
                return count.aohUndoneCount <= count.aohValidCount - 1;
            }
        }

        public AOHStack(AsyncOperationHandle<T>[] stack)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            this.stack = stack;
            this.count = new(stack.Length);
            this.aohEvent = new();

            var aohStack = this;
            foreach (var aoh in stack)
            {
                if (!aoh.IsValid())
                {
                    count.aohValidCount--;
                }

                if (aoh.IsValid() && !aoh.IsDone)
                {
                    aoh.Completed += _ =>
                    {
                        aohStack.count.aohUndoneCount--;
                        if (aohStack.AtLeastOneAOHIsCompleted)
                        {
                            aohStack.RaiseAtLeastOneCompleted();
                        }
                        if (aohStack.AllAOHAreCompleted)
                        {
                            aohStack.RaiseAllCompleted();
                        }
                    };
                }
                else
                {
                    count.aohUndoneCount--;
                }
            }

            if (AtLeastOneAOHIsCompleted)
            {
                RaiseAtLeastOneCompleted();
            }
            if (AllAOHAreCompleted)
            {
                RaiseAllCompleted();
            }
        }

        void RaiseAllCompleted()
        {
            if (aohEvent.allCompletedHasBeenRaised)
            {
                return;
            }

            aohEvent.allCompletedHasBeenRaised = true;
            aohEvent._allCompleted?.Invoke(stack);
        }

        void RaiseAtLeastOneCompleted()
        {
            if (aohEvent.atLeastOneCompletedHasBeenRaised)
            {
                return;
            }

            aohEvent.atLeastOneCompletedHasBeenRaised = true;
            aohEvent._atLeastOneCompleted?.Invoke(stack);
        }
    }
}
