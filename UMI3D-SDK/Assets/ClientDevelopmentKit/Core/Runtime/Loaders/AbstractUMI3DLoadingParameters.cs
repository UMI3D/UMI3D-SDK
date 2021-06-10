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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract loading parameters and workflow 
    /// </summary>
    public abstract class AbstractUMI3DLoadingParameters : ScriptableObject
    {
        /// <summary>
        /// Loader for KHR Light.
        /// </summary>
        public virtual KHR_lights_punctualLoader khr_lights_punctualLoader { get; } = new KHR_lights_punctualLoader();

        /// <summary>
        /// Return the best ResourcesLoader for an extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        abstract public IResourcesLoader SelectLoader(string extension);

        /// <summary>
        /// Return the best MaterialLoader for a GltfMaterialDto.
        /// </summary>
        /// <param name="gltfMatDto"></param>
        /// <returns></returns>
        abstract public AbstractUMI3DMaterialLoader SelectMaterialLoader(GlTFMaterialDto gltfMatDto);

        /// <summary>
        /// Choose the best library variant for this client.
        /// </summary>
        /// <param name="assetLibrary"></param>
        /// <returns></returns>
        abstract public UMI3DLocalAssetDirectory ChooseVariant(AssetLibraryDto assetLibrary);

        /// <summary>
        /// Choose the best file variant for this client.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        abstract public FileDto ChooseVariante(List<FileDto> files);

        /// <summary>
        /// Setup an Umi3d object acording to a UMI3DDto.
        /// </summary>
        /// <param name="dto">Dto to read.</param>
        /// <param name="node">Gameobject on which to setup the object.</param>
        /// <param name="finished">Finished callback.</param>
        /// <param name="failed">Error callback.</param>
        abstract public void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed);

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the updated value</param>
        /// <returns></returns>
        abstract public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property);

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the updated value</param>
        /// <returns></returns>
        abstract public bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container);

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the updated value</param>
        /// <returns></returns>
        abstract public bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container);

        /// <summary>
        /// Handle Operation not handle by default.
        /// </summary>
        /// <param name="operation">Operation to handle.</param>
        /// <param name="performed">Callback to call when the operation is performed (or won't be performed)</param>
        abstract public void UnknownOperationHandler(AbstractOperationDto operation, Action performed);

        /// <summary>
        /// Handle Operation not handle by default.
        /// </summary>
        /// <param name="operation">Operation to handle.</param>
        /// <param name="performed">Callback to call when the operation is performed (or won't be performed)</param>
        abstract public void UnknownOperationHandler(uint operationId, ByteContainer container, Action performed);

        /// <summary>
        /// Load a ResourceDto as a Skybox.
        /// </summary>
        /// <param name="skybox"></param>
        abstract public void loadSkybox(ResourceDto skybox);

    }

}