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
using System.Threading.Tasks;
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
        public abstract IResourcesLoader SelectLoader(string extension);

        /// <summary>
        /// Return the best MaterialLoader for a GltfMaterialDto.
        /// </summary>
        /// <param name="gltfMatDto"></param>
        /// <returns></returns>
        public abstract AbstractUMI3DMaterialLoader SelectMaterialLoader(GlTFMaterialDto gltfMatDto);

        /// <summary>
        /// Choose the best library variant for this client.
        /// </summary>
        /// <param name="assetLibrary"></param>
        /// <returns></returns>
        public abstract UMI3DLocalAssetDirectory ChooseVariant(AssetLibraryDto assetLibrary);

        /// <summary>
        /// Choose the best file variant for this client.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public abstract FileDto ChooseVariant(List<FileDto> files);

        /// <summary>
        /// Setup an UMI3D object based on a <see cref="UMI3DDto"/>.
        /// </summary>
        /// <param name="dto">DTO to read.</param>
        /// <param name="node">Gameobject on which to setup the object.</param>
        /// <param name="finished">Finished callback.</param>
        /// <param name="failed">Error callback.</param>
        public abstract Task ReadUMI3DExtension(ReadUMI3DExtensionData data);

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the updated value.</param>
        /// <returns></returns>
        public abstract Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data);

        /// <summary>
        /// Update a property from a value in a <paramref name="container"/>.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="operationId">UMI3D operation key in <see cref="UMI3DOperationKeys"/>.</param>
        /// <param name="propertyKey">UMI3D property key in <see cref="UMI3DPropertyKeys"/>.</param>
        /// <param name="container">Container of the updated value to read.</param>
        /// <returns></returns>
        public abstract Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data);

        /// <summary>
        /// Read a property in a <paramref name="container"/> and stores its value.
        /// </summary>
        /// <param name="value">Boxing object to store the read value.</param>
        /// <param name="propertyKey">UMI3D key of the property to read.</param>
        /// <param name="container">Container of the value to read.</param>
        /// <returns></returns>
        public abstract Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data);

        /// <summary>
        /// Handle Operation not handled by default.
        /// </summary>
        /// <param name="operation">Operation to handle.</param>
        /// <param name="performed">Callback to call when the operation is performed (or won't be performed)</param>
        public abstract Task UnknownOperationHandler(AbstractOperationDto operation);

        /// <summary>
        /// Handle Operation not handled by default.
        /// </summary>
        /// <param name="operationId">UMI3D operation key in <see cref="UMI3DOperationKeys"/>.</param>
        /// <param name="container">Value container.</param>
        /// <param name="performed">Callback to call when the operation is performed (or won't be performed)</param>
        public abstract Task UnknownOperationHandler(uint operationId, ByteContainer container);

        /// <summary>
        /// Load a <see cref="ResourceDto"/> as a Skybox.
        /// </summary>
        /// <param name="skybox"></param>
        public abstract void LoadSkybox(ResourceDto skybox, SkyboxType type, float skyboxRotatio, float skyboxExposure);

        /// <summary>
        /// Sets skybox properties.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="skyboxRotatio"></param>
        /// <param name="skyboxExposure"></param>
        /// <returns></returns>
        public abstract bool SetSkyboxProperties(SkyboxType type, float skyboxRotatio, float skyboxExposure);
    }
}