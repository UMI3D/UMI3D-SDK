/*
Copyright 2019 - 2021 Inetum

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
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DAbstractNodeDto"/>.
    /// </summary>
    public class UMI3DAbstractNodeLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        #region Dependency Injection

        protected readonly IEnvironmentManager environmentManager;
        protected readonly ILoadingManager loadingManager;

        public UMI3DAbstractNodeLoader() : base()
        {
            environmentManager = UMI3DEnvironmentLoader.Instance;
            loadingManager = UMI3DEnvironmentLoader.Instance;
        }

        public UMI3DAbstractNodeLoader(IEnvironmentManager environmentManager,
                                       ILoadingManager loadingManager)
        {
            this.environmentManager = environmentManager;
            this.loadingManager = loadingManager;
        }

        #endregion Dependency Injection

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DAbstractNodeDto;
        }

        /// <summary>
        /// Load an abstract node dto.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            var nodeDto = data.dto as UMI3DAbstractNodeDto;
            if (data.node == null)
                throw (new Umi3dException("dto should be an  UMI3DAbstractNodeDto"));

            if (data.dto != null)
            {
                if (nodeDto.pid != 0)
                {
                    loadingManager.WaitUntilEntityLoaded(nodeDto.pid, e =>
                    {
                        if (e is UMI3DNodeInstance instance)
                        {
                            var nodeInstance = environmentManager.TryGetEntityInstance(nodeDto.pid) as UMI3DNodeInstance;
                            if ( nodeInstance != null && nodeInstance.mainInstance != null)
                            {
                                data.node.transform.SetParent(nodeInstance.mainInstance.transform, false);
                            }
                            else
                                data.node.transform.SetParent(instance.transform, false);

                            ModelTracker modelTracker = data.node.GetComponentInParent<ModelTracker>();
                            if(modelTracker != null && modelTracker.areSubObjectTracked)
                            {
                                modelTracker.RebindAnimators();
                            }
                        }

                    });
                }
                else
                {
                    data.node.transform.SetParent(environmentManager.transform, false);
                }

                if (data.node.activeSelf != nodeDto.active)
                    data.node.SetActive(nodeDto.active);

                if (nodeDto.isStatic != data.node.isStatic)
                    data.node.isStatic = nodeDto.isStatic;
            }
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            var node = data.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DAbstractNodeDto;
            if (dto == null) dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d;
            if (dto == null) return false;
            switch (data.property.property)
            {
                case UMI3DPropertyKeys.Static:
                    dto.isStatic = (bool)data.property.value;
                    if (dto.isStatic != node.gameObject.isStatic)
                        node.gameObject.isStatic = dto.isStatic;
                    break;
                case UMI3DPropertyKeys.Active:
                    dto.active = (bool)data.property.value;
                    if (node.gameObject.activeSelf != dto.active)
                        node.gameObject.SetActive(dto.active);
                    break;
                case UMI3DPropertyKeys.ParentId:
                    ulong pid = dto.pid = (ulong)(long)data.property.value;
                    UMI3DNodeInstance parent = environmentManager.GetNodeInstance(pid);
                    node.transform.SetParent(parent != null ? parent.transform : environmentManager.transform);
                    if(parent != null)
                    {
                        ModelTracker modelTracker = node.transform.GetComponentInParent<ModelTracker>();
                        if (modelTracker != null && modelTracker.areSubObjectTracked)
                        {
                            modelTracker.RebindAnimators();
                        }
                    }

                    break;
                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.Static:
                    data.result = UMI3DSerializer.Read<bool>(data.container);
                    break;
                case UMI3DPropertyKeys.Active:
                    data.result = UMI3DSerializer.Read<bool>(data.container);
                    break;
                case UMI3DPropertyKeys.ParentId:
                    data.result = UMI3DSerializer.Read<ulong>(data.container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            var node = data.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DAbstractNodeDto;
            if (dto == null) dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d;
            if (dto == null) return false;
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.Static:
                    dto.isStatic = UMI3DSerializer.Read<bool>(data.container);
                    if (dto.isStatic != node.gameObject.isStatic)
                        node.gameObject.isStatic = dto.isStatic;
                    break;
                case UMI3DPropertyKeys.Active:
                    dto.active = UMI3DSerializer.Read<bool>(data.container);
                    if (node.gameObject.activeSelf != dto.active)
                        node.gameObject.SetActive(dto.active);
                    break;
                case UMI3DPropertyKeys.ParentId:
                    ulong pid = dto.pid = UMI3DSerializer.Read<ulong>(data.container);
                    UMI3DNodeInstance parent = UMI3DEnvironmentLoader.GetNode(pid);
                    node.transform.SetParent(parent != null ? parent.transform : environmentManager.transform);
                    if (parent != null)
                    {
                        ModelTracker modelTracker = node.transform.GetComponentInParent<ModelTracker>();
                        if (modelTracker != null && modelTracker.areSubObjectTracked)
                        {
                            modelTracker.RebindAnimators();
                        }
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}