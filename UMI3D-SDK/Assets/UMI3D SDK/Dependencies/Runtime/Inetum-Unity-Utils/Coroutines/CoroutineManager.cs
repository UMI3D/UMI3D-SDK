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

namespace inetum.unityUtils
{
    /// <summary>
    /// Singleton that attach coroutines to the loading handler.
    /// </summary>
    /// Easily mock-able for edit mode unit tests.
    public class CoroutineManager : Singleton<CoroutineManager>, ICoroutineService, ILateRoutineService
    {
        static UMI3DLogger logger = new (mainTag: nameof(CoroutineManager));

        #region Dependency Injection

        private CoroutineManagerMono coroutineManagerMono;
        private PersistentCoroutineManagerMono persistentCoroutineManagerMono;

        private Dictionary<Coroutine, bool> coroutines = new();
        private Dictionary<IEnumerator, bool> lateRoutines = new();

        public CoroutineManager() : base()
        {
            LazyInitialisationCoroutineManager();
            LazyInitialisationPersistentCoroutineManager();
        }

        internal CoroutineManager(CoroutineManagerMono coroutineManagerMono, PersistentCoroutineManagerMono persistentCoroutineManagerMono) : base()
        {
            logger.Assert(coroutineManagerMono != null, $"coroutineManagerMono null when creating a {nameof(CoroutineManager)}");
            logger.Assert(persistentCoroutineManagerMono != null, $"persistentCoroutineManagerMono null when creating a {nameof(CoroutineManager)}");

            LazyInitialisationCoroutineManager(coroutineManagerMono);
            LazyInitialisationPersistentCoroutineManager(persistentCoroutineManagerMono);
        }

        #endregion Dependency Injection

        /// <summary>
        /// Set <see cref="coroutineManagerMono"/> in a lazy way.
        /// 
        /// <para>
        /// If the field is already initialized then it won't be set again.
        /// </para>
        /// </summary>
        /// <param name="coroutineManagerMono"></param>
        void LazyInitialisationCoroutineManager(CoroutineManagerMono coroutineManagerMono = null)
        {
            if (this.coroutineManagerMono != null)
            {
                return;
            }

            if (coroutineManagerMono != null)
            {
                this.coroutineManagerMono = coroutineManagerMono;
            }
            else
            {
                this.coroutineManagerMono = CoroutineManagerMono.Instance;
            }
        }

        /// <summary>
        /// Set <see cref="persistentCoroutineManagerMono"/> in a lazy way.
        /// 
        /// <para>
        /// If the field is already initialized then it won't be set again.
        /// </para>
        /// </summary>
        /// <param name="persistentCoroutineManagerMono"></param>
        void LazyInitialisationPersistentCoroutineManager(PersistentCoroutineManagerMono persistentCoroutineManagerMono = null)
        {
            if (this.persistentCoroutineManagerMono != null)
            {
                return;
            }

            if (persistentCoroutineManagerMono != null)
            {
                this.persistentCoroutineManagerMono = persistentCoroutineManagerMono;
            }
            else
            {
                this.persistentCoroutineManagerMono = PersistentCoroutineManagerMono.Instance;
            }
        }

        /// <inheritdoc/>
        public virtual Coroutine AttachCoroutine(IEnumerator coroutine, bool isPersistent = false)
        {
            logger.Assert(coroutine != null, "Issue", $"Coroutine null when trying to {nameof(AttachCoroutine)}.");

            LazyInitialisationCoroutineManager();
            LazyInitialisationPersistentCoroutineManager();
            
            ICoroutineService routineService = isPersistent ? persistentCoroutineManagerMono : coroutineManagerMono;
            var resRoutine = routineService.AttachCoroutine(coroutine);
            logger.Assert(resRoutine != null, "Issue", $"resRoutine null when trying to attache. Is persistent coroutine: {isPersistent}");

            coroutines.Add(resRoutine, isPersistent);
            return resRoutine;
        }

        /// <inheritdoc/>
        public virtual void DetachCoroutine(Coroutine coroutine)
        {
            logger.Assert(coroutine != null, "Issue", $"Coroutine null when trying to {nameof(DetachCoroutine)}.");

            LazyInitialisationCoroutineManager();
            LazyInitialisationPersistentCoroutineManager();

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
            logger.Assert(routine != null, "Issue", $"Coroutine null when trying to {nameof(AttachLateRoutine)}.");

            LazyInitialisationCoroutineManager();
            LazyInitialisationPersistentCoroutineManager();

            ILateRoutineService routineService = isPersistent ? persistentCoroutineManagerMono : coroutineManagerMono;
            lateRoutines.Add(routine, isPersistent);
            return routineService.AttachLateRoutine(routine);
        }

        public virtual void DetachLateRoutine(IEnumerator routine)
        {
            logger.Assert(routine != null, "Issue", $"Coroutine null when trying to {nameof(DetachLateRoutine)}.");

            LazyInitialisationCoroutineManager();
            LazyInitialisationPersistentCoroutineManager();

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