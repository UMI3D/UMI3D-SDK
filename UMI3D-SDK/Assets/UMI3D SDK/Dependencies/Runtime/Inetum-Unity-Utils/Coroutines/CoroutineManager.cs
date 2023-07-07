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

using System.Collections;

using UnityEngine;

namespace inetum.unityUtils
{
    /// <summary>
    /// Singleton that attach coroutines to the loading handler.
    /// </summary>
    /// Easily mock-able for edit mode unit tests.
    public class CoroutineManager : Singleton<CoroutineManager>, ICoroutineService, ILateRoutineService
    {
        #region Dependency Injection

        private readonly CoroutineManagerMono coroutineManagerMono;

        public CoroutineManager() : base()
        {
            coroutineManagerMono = CoroutineManagerMono.Instance;
        }

        internal CoroutineManager(CoroutineManagerMono coroutineManagerMono) : base()
        {
            this.coroutineManagerMono = coroutineManagerMono;
        }

        #endregion Dependency Injection

        /// <inheritdoc/>
        public virtual Coroutine AttachCoroutine(IEnumerator coroutine)
        {
            return coroutineManagerMono.AttachCoroutine(coroutine);
        }

        /// <inheritdoc/>
        public virtual void DettachCoroutine(Coroutine coroutine)
        {
            coroutineManagerMono.DettachCoroutine(coroutine);
        }

        public virtual IEnumerator AttachLateRoutine(IEnumerator routine)
        {
            return coroutineManagerMono.AttachLateRoutine(routine);
        }

        public virtual void DettachLateRoutine(IEnumerator routine)
        {
            coroutineManagerMono.AttachLateRoutine(routine);
        }
    }
}