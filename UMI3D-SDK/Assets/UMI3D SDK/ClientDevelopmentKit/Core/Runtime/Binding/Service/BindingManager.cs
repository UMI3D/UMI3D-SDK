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

namespace umi3d.cdk.binding
{
    /// <summary>
    /// Core binding manager. Handles binding lifecycles and computations.
    /// </summary>
    public class BindingManager : Singleton<BindingManager>, IBindingBrowserService
    {
        protected const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        #region dependency injection

        private readonly ILateRoutineService routineService;
        private readonly IUMI3DClientServer clientServer;

        public BindingManager() : this(CoroutineManager.Instance, UMI3DClientServer.Instance)
        { }

        public BindingManager(ILateRoutineService coroutineService, IUMI3DClientServer clientServer) : base()
        {
            this.routineService = coroutineService;
            this.clientServer = clientServer;
            Init();
        }

        #endregion dependency injection

        private void Init()
        {
            clientServer.OnLeavingEnvironment.AddListener(Reset);
            clientServer.OnRedirection.AddListener(Reset);
        }

        private void Reset()
        {
            AreBindingsActivated = true;
            foreach ((ulong, ulong) bindingNodeId in bindings.Keys.ToList())
            {
                RemoveBinding(bindingNodeId.Item1,bindingNodeId.Item2);
            }
            bindings.Clear();
            bindingExecutionQueue = new AbstractBinding[0];
        }

        /// <inheritdoc/>
        public virtual bool AreBindingsActivated { get; private set; } = true;

        /// <inheritdoc/>
        public virtual IReadOnlyDictionary<(ulong environmentId, ulong nodeId), AbstractBinding> Bindings => bindings;
        protected readonly Dictionary<(ulong environmentId, ulong nodeId), AbstractBinding> bindings = new();

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
        public virtual void AddBinding(ulong environmentId, ulong boundNodeId, AbstractBinding binding)
        {
            if (binding is null)
                return;

            if (binding is not null)
            {
                bindings[(environmentId, boundNodeId)] = binding;
                ReorderQueue();
            }

            if (bindings.Count > 0 && AreBindingsActivated && bindingRoutine is null)
            {
                bindingRoutine = routineService.AttachLateRoutine(BindingApplicationRoutine());
                clientServer.OnLeavingEnvironment.AddListener(() => { if (bindingRoutine is not null) routineService.DetachLateRoutine(bindingRoutine); });
            }
        }

        /// <summary>
        /// Coroutine that executes each binding in descending priority order.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator BindingApplicationRoutine()
        {
            while (AreBindingsActivated)
            {
                ApplyBindings();
                yield return null;
            }
        }

        private void ApplyBindings()
        {
            if (bindings.Count != bindingExecutionQueue.Length)
                ReorderQueue();

            foreach (AbstractBinding binding in bindingExecutionQueue)
            {
                if (binding.BoundTransform == null)
                {
                    UMI3DLogger.LogWarning($"Bound transform is null. It may have been deleted without removing the binding first.", DebugScope.CDK | DebugScope.Core);
                    ((ulong environmentId, ulong nodeId), AbstractBinding bindingToRemove) = bindings.FirstOrDefault(kp => kp.Value.Equals(binding));
                    if (bindingToRemove != null)
                        RemoveBinding(environmentId, nodeId);
                    continue;
                }

                binding.Apply(out bool success);
                if (!success)
                    break;
            }
        }

        public void ForceBindingsApplicationUpdate()
        {
            if (!AreBindingsActivated)
                return;

            ApplyBindings();
        }

        private void ReorderQueue()
        {
            if (bindings.Count > 0)
                bindingExecutionQueue = bindings.Values.OrderByDescending(x => x.Priority).ToArray();
            else
                bindingExecutionQueue = new AbstractBinding[0];
        }

        /// <inheritdoc/>
        public virtual void RemoveBinding(ulong environmentId, ulong boundNodeId)
        {
            var key = (environmentId, boundNodeId);
            if (bindings.TryGetValue(key, out AbstractBinding bindingToRemove))
            {
                if (bindingToRemove is not null && bindingToRemove.ResetWhenRemoved)
                {
                    bindingToRemove.Reset();
                }

                bindings.Remove(key);
                ReorderQueue();

                if (bindings.Count == 0 && bindingRoutine is not null)
                {
                    routineService.DetachLateRoutine(bindingRoutine);
                    bindingRoutine = null;
                }
            }
            else
            {
                UMI3DLogger.LogWarning($"Cannot remove bindings on node ({environmentId},{boundNodeId}). Node has no binding.", DEBUG_SCOPE);
            }
        }
    }
}