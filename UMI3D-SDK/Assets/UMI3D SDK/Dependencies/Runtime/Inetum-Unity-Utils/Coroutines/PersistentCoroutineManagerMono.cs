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
    /// Singleton monobehaviour that runs coroutines. Prefer to use <see cref="PersistentCoroutineManagerMono"/>
    /// </summary>
    internal class PersistentCoroutineManagerMono : PersistentSingleBehaviour<PersistentCoroutineManagerMono>, ICoroutineService, ILateRoutineService
    {
        private List<IEnumerator> routines = new();
        private Queue<IEnumerator> routinesToRemove = new();
        private Queue<IEnumerator> routinesToAdd = new();

        /// <summary>
        /// Call this method to attach a coroutine to the handler and start it.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public Coroutine AttachCoroutine(IEnumerator coroutine, bool isPersistent = false)
        {
            return StartCoroutine(coroutine);
        }

        /// <summary>
        /// Call this method to remove a coroutine from the handler and stop it.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public void DetachCoroutine(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        public IEnumerator AttachLateRoutine(IEnumerator routine, bool isPersistent = false)
        {
            routinesToAdd.Enqueue(routine);
            return routine;
        }

        public void DetachLateRoutine(IEnumerator routine)
        {
            routinesToRemove.Enqueue(routine);
        }

        private void LateUpdate()
        {
            if (routinesToAdd.Count > 0)
            {
                foreach (var routineToAdd in routinesToAdd)
                {
                    routines.Add(routineToAdd);
                }
                routinesToAdd.Clear();
            }

            if (routinesToRemove.Count > 0)
            {
                foreach (var routineToRemove in routinesToRemove)
                {
                    routines.Remove(routineToRemove);
                }
                routinesToRemove.Clear();
            }

            foreach (var routine in routines)
                routine.MoveNext();
        }
    }
}