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
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Handle binding operations and computations.
    /// </summary>
    public interface IBindingManager
    {
        /// <summary>
        /// Are bindings computed for this client?
        /// </summary>
        public bool AreBindingsActivated { get; }

        /// <summary>
        /// Currently computed bindings per UMI3D node id.
        /// </summary>
        public Dictionary<ulong, AbstractBinding> Bindings { get; }

        /// <summary>
        /// Load and add a new binding operation.
        /// </summary>
        /// <param name="dto"></param>
        public void AddBinding(BindingDto dto);

        /// <summary>
        /// Add a binding that already has been loaded.
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="boundNodeId"></param>
        public void AddBinding(AbstractBinding binding, ulong boundNodeId);

        /// <summary>
        /// Remove a binding to compute.
        /// </summary>
        /// <param name="dto"></param>
        public void RemoveBinding(RemoveBindingDto dto);

        /// <summary>
        /// Load a binding et prepare it for computing.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="boundNodeId"></param>
        /// <returns></returns>
        public AbstractBinding Load(AbstractBindingDataDto dto, ulong boundNodeId);

        /// <summary>
        /// Enable/disable bindings computation for this client.
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateBindingsActivation(UpdateBindingsActivationDto dto);
    }

    /// <summary>
    /// Core binding manager. Handles binding operations and computations.
    /// </summary>
    public class BindingManager : Singleton<BindingManager>, IBindingManager
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

        public virtual bool AreBindingsActivated { get; private set; } = true;

        public virtual Dictionary<ulong, AbstractBinding> Bindings { get; private set; } = new();

        /// <summary>
        /// Execute every binding.
        /// </summary>
        private IEnumerator bindingRoutine;

        public virtual void DisableBindings()
        {
            AreBindingsActivated = false;
        }

        public virtual void EnableBindings()
        {
            AreBindingsActivated = true;
        }

        public virtual void UpdateBindingsActivation(UpdateBindingsActivationDto dto)
        {
            if (dto.areBindingsActivated == AreBindingsActivated)
                return;

            if (dto.areBindingsActivated)
                EnableBindings();
            else
                DisableBindings();
        }

        public virtual void AddBinding(BindingDto dto)
        {
            AbstractBinding binding = Load(dto.data, dto.boundNodeId);
            if (binding is null)
                return;
            AddBinding(binding, dto.boundNodeId);
        }

        public virtual void AddBinding(AbstractBinding binding, ulong boundNodeId)
        {
            if (binding is not null)
                Bindings[boundNodeId] = binding;

            if (Bindings.Count > 0 && AreBindingsActivated && bindingRoutine is null)
                bindingRoutine = routineService.AttachLateRoutine(BindingRoutine());
        }

        public virtual AbstractBinding Load(AbstractBindingDataDto dto, ulong boundNodeId)
        {
            switch (dto)
            {
                case AbstractSimpleBindingDataDto simpleBindingDto:
                    {
                        UMI3DNodeInstance node = environmentService.GetNodeInstance(boundNodeId);
                        if (node is null)
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on node {boundNodeId}. Node does not exist.", DEBUG_SCOPE);
                            return null;
                        }

                        return new NodeBinding(simpleBindingDto, node.transform);
                    }
                case MultiBindingDataDto multiBindingDataDto:
                    {
                        UMI3DNodeInstance node = environmentService.GetNodeInstance(boundNodeId);
                        (AbstractSimpleBinding binding, bool partialFit)[] orderedBindingData = multiBindingDataDto.Bindings
                                                                                            .OrderByDescending(x => x.priority)
                                                                                            .Select(x => (binding: new NodeBinding(x, node.transform) as AbstractSimpleBinding,
                                                                                                                                partialFit: x.partialFit))
                                                                                            .Where(x => x.binding is not null)
                                                                                            .ToArray();

                        if (orderedBindingData.Length == 0)
                        {
                            UMI3DLogger.LogWarning($"Impossible to multi-bind. All bindings are impossible to apply.", DEBUG_SCOPE);
                            return null;
                        }

                        return new MultiBinding(multiBindingDataDto, orderedBindingData, node.transform);
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Coroutine that executes each binding in no particular order.
        /// </summary>
        /// <returns></returns>
        private IEnumerator BindingRoutine()
        {
            while (AreBindingsActivated)
            {
                foreach (var binding in Bindings)
                {
                    binding.Value.Apply(out bool success);
                    if (!success)
                        yield break;
                }
                yield return null;
            }
        }

        public virtual void RemoveBinding(RemoveBindingDto dto)
        {
            if (Bindings.ContainsKey(dto.boundNodeId))
            {
                Bindings.Remove(dto.boundNodeId);
                if (Bindings.Count == 0 && bindingRoutine is not null)
                {
                    routineService.DettachLateRoutine(bindingRoutine);
                    bindingRoutine = null;
                }   
            }
        }
    }
}