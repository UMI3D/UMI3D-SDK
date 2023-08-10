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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="GlTFNodeDto"/>.
    /// </summary>
    public class GlTFNodeLoader : AbstractLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GlTFNodeLoader()
        {
        }

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return false;
        }

        /// <inheritdoc/>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update Umi3dProperty.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns>state if the property was handled</returns>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (UMI3DEnvironmentLoader.Parameters.khr_lights_punctualLoader.SetLightPorperty(value.entity, value.property))
                return true;
            var node = value.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = node.dto as GlTFNodeDto;
            if (dto == null) return false;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.Position:
                    node.transform.localPosition = (dto.position = (Vector3Dto)value.property.value).Struct();
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Rotation:
                    node.transform.localRotation = (dto.rotation = (Vector4Dto)value.property.value).Quaternion();
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Scale:
                    node.transform.localScale = (dto.scale = (Vector3Dto)value.property.value).Struct();
                    node.SendOnPoseUpdated();
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Update Umi3dProperty.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns>state if the property was handled</returns>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (UMI3DEnvironmentLoader.Parameters.khr_lights_punctualLoader.SetLightPorperty(value.entity, value.operationId, value.propertyKey, value.container))
                return true;
            var node = value.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = node.dto as GlTFNodeDto;
            if (dto == null) return false;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.Position:
                    node.transform.localPosition = (dto.position =  UMI3DSerializer.Read<Vector3Dto>(value.container)).Struct();
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Rotation:
                    node.transform.localRotation = (dto.rotation = UMI3DSerializer.Read<Vector4Dto>(value.container)).Quaternion();
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Scale:
                     node.transform.localScale = (dto.scale = UMI3DSerializer.Read<Vector3Dto>(value.container)).Struct();
                    node.SendOnPoseUpdated();
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            if (UMI3DEnvironmentLoader.Parameters.khr_lights_punctualLoader.ReadLightPorperty(ref data.result, data.propertyKey, data.container))
                return true;
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.Position:
                    data.result = UMI3DSerializer.Read<Vector3Dto>(data.container);
                    break;
                case UMI3DPropertyKeys.Rotation:
                    data.result = UMI3DSerializer.Read<Vector4Dto>(data.container);
                    break;
                case UMI3DPropertyKeys.Scale:
                    data.result = UMI3DSerializer.Read<Vector3Dto>(data.container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Load a single GlTFNode.
        /// </summary>
        /// <param name="node">node to load.</param>
        /// <param name="finished">Callback called when the node is loaded.</param>
        /// <returns></returns>
        public async Task LoadNode(GlTFNodeDto node)
        {
            await LoadNodes(new List<GlTFNodeDto>() { node }, new Progress(0,"Load Node"));
        }

        /// <summary>
        /// Load a collection of GlTFNodes.
        /// </summary>
        /// <param name="nodes">nodes to load.</param>
        /// <param name="finished">Callback called when all nodes are loaded.</param>
        /// <param name="LoadedNodesCount">Action called each time a node is loaded with the count of all loaded node in parameter.</param>
        /// <returns></returns>
        public async Task LoadNodes(IEnumerable<GlTFNodeDto> nodes, Progress progress)
        {
#if UNITY_EDITOR
            var tasks = nodes
                .Select(n => CreateNode(n))
                .Select(async node =>
                {
                    var dto = node.dto as GlTFNodeDto;
                    progress.AddTotal();
                    try
                    {
                        await UMI3DEnvironmentLoader.AbstractParameters.ReadUMI3DExtension(new ReadUMI3DExtensionData(dto.extensions.umi3d, node.gameObject));

                        ReadLightingExtensions(dto, node.gameObject);
                        // Important: all nodes in the scene must be registred before to handle hierarchy. 
                        // Done using CreateNode( GlTFNodeDto dto) on the whole nodes collections
                        node.transform.localPosition = dto.position.Struct();
                        node.transform.localRotation = dto.rotation.Quaternion();
                        node.transform.localScale = dto.scale.Struct();

                        node.SendOnPoseUpdated();
                        node.NotifyLoaded();

                        progress.AddComplete();
                    }
                    catch (Exception e)
                    {
                        UMI3DLogger.LogException(e, scope);
                        UMI3DLogger.LogError($"Failed to read Umi3d extension [{dto.name}]", scope);
                        if (!await progress.AddFailed(e))
                            throw;
                    }
                });

            foreach (var task in tasks)
            {
                await task;
            }
#else
            await Task.WhenAll(
                nodes
                .Select(n => CreateNode(n))
                .Select(async node =>
                    {
                        var dto = node.dto as GlTFNodeDto;
                        progress.AddTotal();
                        try
                        {
                            await UMI3DEnvironmentLoader.Parameters.ReadUMI3DExtension(new ReadUMI3DExtensionData(dto.extensions.umi3d, node.gameObject));

                            ReadLightingExtensions(dto, node.gameObject);
                            // Important: all nodes in the scene must be registred before to handle hierarchy. 
                            // Done using CreateNode( GlTFNodeDto dto) on the whole nodes collections
                            node.transform.localPosition = dto.position.Struct();
                            node.transform.localRotation = dto.rotation.Quaternion();
                            node.transform.localScale = dto.scale.Struct();

                            node.SendOnPoseUpdated();
                            node.NotifyLoaded();

                            progress.AddComplete();
                        }
                        catch (Exception e)
                        {
                            UMI3DLogger.LogException(e, scope);
                            UMI3DLogger.LogError($"Failed to read Umi3d extension [{dto.name}]", scope);
                            if (!await progress.AddFailed(e))
                                throw;
                        }
                    }));
#endif
        }

        /// <summary>
        /// Create and register a node
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private UMI3DNodeInstance CreateNode(GlTFNodeDto dto)
        {
            var go = new GameObject(dto.name);
            return UMI3DEnvironmentLoader.RegisterNodeInstance(dto.extensions.umi3d.id, dto, go);
        }

        /// <summary>
        /// Read KHR Light Extensions
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="node"></param>
        public void ReadLightingExtensions(GlTFNodeDto dto, GameObject node)
        {
            if (dto.extensions.KHR_lights_punctual != null)
            {
                UMI3DEnvironmentLoader.Parameters.khr_lights_punctualLoader.CreateLight(dto.extensions.KHR_lights_punctual, node.gameObject);
                //TODO: future loaders for e.g. shadows parameters
            }
        }
    }
}