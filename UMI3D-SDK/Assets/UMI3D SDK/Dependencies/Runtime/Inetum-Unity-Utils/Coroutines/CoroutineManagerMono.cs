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
using System.Collections.Generic;

using UnityEngine;

namespace inetum.unityUtils
{
    /// <summary>
    /// Singleton monobehaviour that runs coroutines. Prefer to use <see cref="CoroutineManagerMono"/>
    /// </summary>
    internal class CoroutineManagerMono : SingleBehaviour<CoroutineManagerMono>, ICoroutineService, ILateRoutineService
    {
        private List<IEnumerator> routines = new();

        /// <summary>
        /// Call this method to attach a coroutine to the handler and start it.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public Coroutine AttachCoroutine(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }

        /// <summary>
        /// Call this method to remove a coroutine from the handler and stop it.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public void DettachCoroutine(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        public IEnumerator AttachLateRoutine(IEnumerator routine)
        {
            routines.Add(routine);
            return routine;
        }

        public void DettachLateRoutine(IEnumerator routine)
        {
            routines.Remove(routine);
        }

        private void LateUpdate()
        {
            if (routines.Count == 0)
                return;

            foreach (var routine in routines)
                routine.MoveNext();
        }
    }
}