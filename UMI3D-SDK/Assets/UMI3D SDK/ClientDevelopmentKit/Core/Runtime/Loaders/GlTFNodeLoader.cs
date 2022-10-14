﻿/*
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
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader For GlTFNode.
    /// </summary>
    public class GlTFNodeLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GlTFNodeLoader()
        {
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
            await Task.WhenAll(
                nodes
                .Select(n => CreateNode(n))
                .Select(async node =>
                    {
                        var dto = node.dto as GlTFNodeDto;
                        progress.AddTotal();
                        try
                        {
                            await UMI3DEnvironmentLoader.Parameters.ReadUMI3DExtension(dto.extensions.umi3d, node.gameObject);

                            ReadLightingExtensions(dto, node.gameObject);
                            // Important: all nodes in the scene must be registred before to handle hierarchy. 
                            // Done using CreateNode( GlTFNodeDto dto) on the whole nodes collections
                            node.transform.localPosition = dto.position;
                            node.transform.localRotation = dto.rotation;
                            node.transform.localScale = dto.scale;

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

        /// <summary>
        /// Update Umi3dProperty.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns>state if the property was handled</returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (UMI3DEnvironmentLoader.Parameters.khr_lights_punctualLoader.SetLightPorperty(entity, property))
                return true;
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = node.dto as GlTFNodeDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.Position:
                    node.transform.localPosition = dto.position = (SerializableVector3)property.value;
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Rotation:
                    node.transform.localRotation = dto.rotation = (SerializableVector4)property.value;
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Scale:
                    node.transform.localScale = dto.scale = (SerializableVector3)property.value;
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
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (UMI3DEnvironmentLoader.Parameters.khr_lights_punctualLoader.SetLightPorperty(entity, operationId, propertyKey, container))
                return true;
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = node.dto as GlTFNodeDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Position:
                    dto.position = node.transform.localPosition = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Rotation:
                    node.transform.localRotation = dto.rotation = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                    node.SendOnPoseUpdated();
                    break;
                case UMI3DPropertyKeys.Scale:
                    dto.scale = node.transform.localScale = UMI3DNetworkingHelper.Read<Vector3>(container);
                    node.SendOnPoseUpdated();
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (UMI3DEnvironmentLoader.Parameters.khr_lights_punctualLoader.ReadLightPorperty(ref value, propertyKey, container))
                return true;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Position:
                    value = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                    break;
                case UMI3DPropertyKeys.Rotation:
                    value = UMI3DNetworkingHelper.Read<SerializableVector4>(container); ;
                    break;
                case UMI3DPropertyKeys.Scale:
                    value = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}