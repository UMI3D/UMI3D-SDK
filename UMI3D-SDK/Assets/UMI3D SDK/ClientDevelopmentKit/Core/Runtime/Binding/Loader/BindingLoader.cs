﻿/*
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

using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.binding;

namespace umi3d.cdk.binding
{
    /// <summary>
    /// Loader for bindings.
    /// </summary>
    public class BindingLoader : AbstractLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <inheritdoc/>
        public override UMI3DVersion.VersionCompatibility version => new("2.6", "*");

        #region DependencyInjection

        private readonly IBindingBrowserService bindingManagementService;
        private readonly UMI3DEnvironmentLoader environmentLoaderService;

        public BindingLoader()
        {
            bindingManagementService = BindingManager.Instance;
            environmentLoaderService = UMI3DEnvironmentLoader.Instance;
        }

        public BindingLoader(IBindingBrowserService bindingManager, UMI3DEnvironmentLoader environmentLoaderService)
        {
            bindingManagementService = bindingManager;
            this.environmentLoaderService = environmentLoaderService;
        }

        #endregion DependencyInjection

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is BindingDto;
        }

        /// <inheritdoc/>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as BindingDto;

            await environmentLoaderService.WaitUntilEntityLoaded(dto.boundNodeId, null);

            AbstractBinding binding = await LoadData(dto.boundNodeId, dto.data);

            bindingManagementService.AddBinding(dto.boundNodeId, binding);

            void onDelete() { bindingManagementService.RemoveBinding(dto.boundNodeId); }
            environmentLoaderService.RegisterEntity(dto.id, dto, null, onDelete).NotifyLoaded();
        }

        /// <inheritdoc/>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity.dto is not BindingDto) //nothing to set on a binding, it shoud be deleted and added again
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity.dto is not BindingDto) //nothing to set on a binding, it shoud be deleted and added again
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Load the right binding based on the binding data.
        /// </summary>
        /// <param name="boundNodeId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        protected virtual async Task<AbstractBinding> LoadData(ulong boundNodeId, AbstractBindingDataDto dto)
        {
            switch (dto)
            {
                case NodeBindingDataDto nodeBindingDataDto:
                    {
                        UMI3DNodeInstance node = environmentLoaderService.GetNodeInstance(boundNodeId);
                        if (node is null)
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind node {boundNodeId}. Node does not exist.", DEBUG_SCOPE);
                            return null;
                        }

                        var parentNode = await environmentLoaderService.WaitUntilEntityLoaded(nodeBindingDataDto.parentNodeId, null) as UMI3DNodeInstance;
                        if (parentNode is null)
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind node {boundNodeId} on parent node {nodeBindingDataDto.parentNodeId}. Parent node does not exist.", DEBUG_SCOPE);
                            return null;
                        }

                        return new NodeBinding(nodeBindingDataDto, node.transform, parentNode);
                    }
                case MultiBindingDataDto multiBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentLoaderService.GetNodeInstance(boundNodeId);

                        var tasks = multiBindingDataDto.Bindings.Select(x => LoadData(boundNodeId, x));
                        var bindings = await Task.WhenAll(tasks);

                        var simpleBindings = bindings.Select(x => x as AbstractSimpleBinding)
                                                        .Where(x => x is not null)
                                                        .OrderByDescending(x => x.Priority)
                                                        .ToArray();

                        if (bindings.Length == 0)
                        {
                            UMI3DLogger.LogWarning($"Impossible to multi-bind. All bindings are impossible to apply.", DEBUG_SCOPE);
                            return null;
                        }

                        return new MultiBinding(multiBindingDataDto, simpleBindings, boundNode.transform, isOrdered: true);
                    }
                default:
                    return null;
            }
        }
    }
}