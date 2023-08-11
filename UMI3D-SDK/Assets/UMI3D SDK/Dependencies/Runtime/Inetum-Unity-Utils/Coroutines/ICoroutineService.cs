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
    /// Service that provide access to a monobehaviour running unity coroutines
    /// </summary>
    public interface ICoroutineService
    {
        // <summary>
        /// Call this method to attach a coroutine to the handler and start it.
        /// </summary>
        public Coroutine AttachCoroutine(IEnumerator coroutine, bool isPersistent = false);

        /// <summary>
        /// Call this method to remove a coroutine from the handler and stop it.
        /// </summary>
        public void DetachCoroutine(Coroutine coroutine);
    }

    /// <summary>
    /// Routine service for coroutine to call during late update.
    /// </summary>
    public interface ILateRoutineService
    {
        // <summary>
        /// Call this method to attach a late update routine to the handler and start it.
        /// </summary>
        public IEnumerator AttachLateRoutine(IEnumerator routine, bool isPersistent = false);

        /// <summary>
        /// Call this method to remove a late update routine from the handler and stop it.
        /// </summary>
        public void DetachLateRoutine(IEnumerator routine);
    }
}