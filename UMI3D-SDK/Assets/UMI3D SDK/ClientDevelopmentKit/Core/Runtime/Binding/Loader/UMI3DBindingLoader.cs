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

using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DBindingLoader : AbstractLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public override UMI3DVersion.VersionCompatibility version => new("2.6", "*");

        #region DependencyInjection

        private readonly IBindingBrowserService bindingManagementService;
        private readonly UMI3DEnvironmentLoader environmentLoaderService;

        public UMI3DBindingLoader()
        {
            bindingManagementService = BindingManager.Instance;
            environmentLoaderService = UMI3DEnvironmentLoader.Instance;
        }

        public UMI3DBindingLoader(IBindingBrowserService emoteManager)
        {
            bindingManagementService = emoteManager;
            environmentLoaderService = UMI3DEnvironmentLoader.Instance;
        }

        #endregion DependencyInjection

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is BindingDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as BindingDto;
            
            await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.boundNodeId, null);

            AbstractBinding binding = LoadData(dto.boundNodeId, dto.data);

            bindingManagementService.AddBinding(dto.boundNodeId, binding);

            void onDelete() { bindingManagementService.RemoveBinding(dto.boundNodeId); }
            environmentLoaderService.RegisterEntity(dto.id, dto, null, onDelete).NotifyLoaded();
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity.dto is not BindingDto) //nothing to set on a binding, it shoud be deleted and added again
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity.dto is not BindingDto) //nothing to set on a binding, it shoud be deleted and added again
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public virtual AbstractBinding LoadData(ulong boundNodeId, AbstractBindingDataDto dto)
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

                        UMI3DNodeInstance parentNode = environmentLoaderService.GetNodeInstance(nodeBindingDataDto.nodeId);
                        if (parentNode is null)
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind node {boundNodeId} on parent node {nodeBindingDataDto.nodeId}. Parent node does not exist.", DEBUG_SCOPE);
                            return null;
                        }

                        return new NodeBinding(nodeBindingDataDto, node.transform, parentNode);
                    }
                case MultiBindingDataDto multiBindingDataDto:
                    {
                        UMI3DNodeInstance node = environmentLoaderService.GetNodeInstance(boundNodeId);
                        (AbstractSimpleBinding binding, bool partialFit)[] orderedBindingData = multiBindingDataDto.Bindings
                                                                                            .OrderByDescending(x => x.priority)
                                                                                            .Select(x =>
                                                                                            {
                                                                                                if (x is not NodeBindingDataDto nodeBindingDataDto)
                                                                                                    return new(null, false);
                                                                                                UMI3DNodeInstance parentNode = environmentLoaderService.GetNodeInstance(nodeBindingDataDto.nodeId);
                                                                                                return (binding: new NodeBinding(x, node.transform, parentNode) as AbstractSimpleBinding, x.partialFit);
                                                                                            })
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
    }
}