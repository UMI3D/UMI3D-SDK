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

using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public interface IUMI3DLoadingParameters : IUMI3DAbstractLoadingParameters
    {
        UMI3DAbstractAnchorLoader AnchorLoader { get; }
        List<AbstractUMI3DMaterialLoader> MaterialLoaders { get; }
        UMI3DNodeLoader nodeLoader { get; }
        List<IResourcesLoader> ResourcesLoaders { get; }
        Material skyboxEquirectangularMaterial { get; }
        Material skyboxMaterial { get; }
        KHR_lights_punctualLoader khr_lights_punctualLoader { get; }

        /// <summary>
        /// True if the browser uses purely Virtual Reality and not any form of Mixed Reality
        /// </summary>
        bool HasImmersiveDevice { get; }

        /// <summary>
        /// True if the browser uses an immersive display.
        /// </summary>
        bool HasHeadMountedDisplay { get; }
    }

    public interface IUMI3DAbstractLoadingParameters
    {

        UMI3DLocalAssetDirectoryDto ChooseVariant(AssetLibraryDto assetLibrary);
        FileDto ChooseVariant(List<FileDto> files);
        void LoadSkybox(ResourceDto skybox, SkyboxType type, float skyboxRotation, float skyboxExposure);
        Task ReadUMI3DExtension(ReadUMI3DExtensionData data);
        Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data);
        IResourcesLoader SelectLoader(string extension);
        AbstractUMI3DMaterialLoader SelectMaterialLoader(GlTFMaterialDto gltfMatDto);
        bool SetSkyboxProperties(SkyboxType type, float skyboxRotation, float skyboxExposure);
        Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data);
        Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data);
        Material GetDefaultMaterial();
        Task UnknownOperationHandler(DtoContainer operation);
        Task UnknownOperationHandler(uint operationId, ByteContainer container);
    }
}