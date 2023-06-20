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

using inetum.unityUtils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Core binding manager. Handles binding lifecycles and computations.
    /// </summary>
    public class BindingManager : Singleton<BindingManager>, IBindingBrowserService
    {
        protected const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        #region dependency injection

        private readonly ILateRoutineService routineService;
        private readonly UMI3DEnvironmentLoader environmentService;

        public BindingManager() : base()
        {
            routineService = CoroutineManager.Instance;
            environmentService = UMI3DEnvironmentLoader.Instance;
        }

        public BindingManager(ILateRoutineService coroutineService, UMI3DEnvironmentLoader environmentService)
        {
            this.routineService = coroutineService;
            this.environmentService = environmentService;
        }

        #endregion dependency injection

        /// <inheritdoc/>
        public virtual bool AreBindingsActivated { get; private set; } = true;

        /// <inheritdoc/>
        public virtual Dictionary<ulong, AbstractBinding> Bindings { get; private set; } = new();

        protected AbstractBinding[] bindingExecutionQueue = new AbstractBinding[0];

        /// <summary>
        /// Execute every binding.
        /// </summary>
        private IEnumerator bindingRoutine;

        /// <inheritdoc/>
        public virtual void DisableBindings()
        {
            AreBindingsActivated = false;
        }

        /// <inheritdoc/>
        public virtual void EnableBindings()
        {
            AreBindingsActivated = true;
        }

        /// <inheritdoc/>
        public virtual void UpdateBindingsActivation(bool shouldEnable)
        {
            if (shouldEnable == AreBindingsActivated)
                return;

            if (shouldEnable)
                EnableBindings();
            else
                DisableBindings();
        }

        /// <inheritdoc/>
        public virtual void AddBinding(ulong boundNodeId, AbstractBinding binding)
        {
            if (binding is null)
                return;

            if (binding is not null)
            {
                Bindings[boundNodeId] = binding;
                ReorderQueue();
            }

            if (Bindings.Count > 0 && AreBindingsActivated && bindingRoutine is null)
                bindingRoutine = routineService.AttachLateRoutine(BindingApplicationRoutine());
        }

        /// <summary>
        /// Coroutine that executes each binding in descending priority order.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator BindingApplicationRoutine()
        {
            while (AreBindingsActivated)
            {
                if (Bindings.Count != bindingExecutionQueue.Length)
                    ReorderQueue();

                foreach (var binding in bindingExecutionQueue)
                {
                    binding.Apply(out bool success);
                    if (!success)
                        break;
                }
                yield return null;
            }
        }

        private void ReorderQueue()
        {
            if (Bindings.Values.Count > 0)
                bindingExecutionQueue = Bindings.Values.OrderByDescending(x => x.Priority).ToArray();
            else
                bindingExecutionQueue = new AbstractBinding[0];
        }

        /// <inheritdoc/>
        public virtual void RemoveBinding(ulong boundNodeId)
        {
            if (Bindings.ContainsKey(boundNodeId))
            {
                Bindings.Remove(boundNodeId);
                ReorderQueue();

                if (Bindings.Count == 0 && bindingRoutine is not null)
                {
                    routineService.DettachLateRoutine(bindingRoutine);
                    bindingRoutine = null;
                }
            }
            else
            {
                UMI3DLogger.LogWarning($"Cannot remove bindings on node {boundNodeId}. Node has no binding.", DEBUG_SCOPE);
            }
        }
    }
}