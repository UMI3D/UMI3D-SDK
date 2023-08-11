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
        private readonly PersistentCoroutineManagerMono persistentCoroutineManagerMono;

        private Dictionary<Coroutine, bool> coroutines = new();
        private Dictionary<IEnumerator, bool> lateRoutines = new();

        public CoroutineManager() : base()
        {
            coroutineManagerMono = CoroutineManagerMono.Instance;
            persistentCoroutineManagerMono = PersistentCoroutineManagerMono.Instance;
        }

        internal CoroutineManager(CoroutineManagerMono coroutineManagerMono, PersistentCoroutineManagerMono persistentCoroutineManagerMono) : base()
        {
            this.coroutineManagerMono = coroutineManagerMono;
            this.persistentCoroutineManagerMono = persistentCoroutineManagerMono;
        }

        #endregion Dependency Injection

        /// <inheritdoc/>
        public virtual Coroutine AttachCoroutine(IEnumerator coroutine, bool isPersistent = false)
        {
            ICoroutineService routineService = isPersistent ? persistentCoroutineManagerMono : coroutineManagerMono;
            var resRoutine = routineService.AttachCoroutine(coroutine);
            coroutines.Add(resRoutine, isPersistent);
            return resRoutine;
        }

        /// <inheritdoc/>
        public virtual void DetachCoroutine(Coroutine coroutine)
        {
            if (coroutines.TryGetValue(coroutine, out bool isPersistent))
            {
                ICoroutineService routineService = isPersistent ? persistentCoroutineManagerMono : coroutineManagerMono;
                routineService.DetachCoroutine(coroutine);
            }
            else
                throw new Exception("Can't detach, coroutine not found");
        }

        public virtual IEnumerator AttachLateRoutine(IEnumerator routine, bool isPersistent = false)
        {
            ILateRoutineService routineService = isPersistent ? persistentCoroutineManagerMono : coroutineManagerMono;
            lateRoutines.Add(routine, isPersistent);
            return routineService.AttachLateRoutine(routine);
        }

        public virtual void DetachLateRoutine(IEnumerator routine)
        {
            if (lateRoutines.TryGetValue(routine, out bool isPersistent))
            {
                ILateRoutineService routineService = isPersistent ? persistentCoroutineManagerMono : coroutineManagerMono;
                routineService.DetachLateRoutine(routine);
            }
            else
                throw new Exception("Can't detach, routine not found");
        }
    }
}