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
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="PreloadedSceneDto"/>.
    /// </summary>
    public class PreloadedSceneLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is PreloadedSceneDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var scenedto = value.dto as PreloadedSceneDto;
            ResourceDto resourceScene = scenedto.scene;

            if (resourceScene != null)
            {
                FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(resourceScene.variants);  // Peut etre ameliore

                string url = fileToLoad.url;
                string ext = fileToLoad.extension;
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                if (loader != null)
                {
                    var o = await UMI3DResourcesManager.LoadFile(UMI3DGlobalID.EnvironementId, fileToLoad, loader);
                    UMI3DLogger.Log("this scene is going  to be loaded : " + fileToLoad.pathIfInBundle, scope);
                }
            }
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var entity = value.entity;
            var property = value.property;

            if (entity == null) return false;
            UMI3DEnvironmentDto dto = ((entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d;
            if (dto == null) return false;
            if (property.property == UMI3DPropertyKeys.PreloadedScenes)
            {
                switch (property)
                {
                    case SetEntityListAddPropertyDto add:
                    case SetEntityListRemovePropertyDto rem:
                    case SetEntityListPropertyDto set:
                        UMI3DLogger.Log($"Case not handled {property}", scope);
                        break;
                    default:
                        var newList = (List<PreloadedSceneDto>)property.value;
                        List<PreloadedSceneDto> oldList = dto.preloadedScenes;
                        var scenesToUnload = new List<PreloadedSceneDto>();
                        var scenesToLoad = new List<PreloadedSceneDto>();

                        foreach (PreloadedSceneDto newScene in newList)
                        {
                            if (!oldList.Contains(newScene))
                            {
                                scenesToLoad.Add(newScene);
                            }
                        }
                        foreach (PreloadedSceneDto oldScene in oldList)
                        {
                            if (!newList.Contains(oldScene))
                            {
                                scenesToUnload.Add(oldScene);
                            }
                        }

                        foreach (PreloadedSceneDto scene in scenesToLoad)
                            await ReadUMI3DExtension(new ReadUMI3DExtensionData(scene, null));

                        foreach (PreloadedSceneDto scene in scenesToUnload)
                            Unload(scene, null);

                        break;
                }
            }

            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity == null) return false;
            UMI3DEnvironmentDto dto = ((value.entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d;
            if (dto == null) return false;
            if (value.propertyKey == UMI3DPropertyKeys.PreloadedScenes)
            {
                switch (value.operationId)
                {
                    case UMI3DOperationKeys.SetEntityListAddProperty:
                    case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    case UMI3DOperationKeys.SetEntityListProperty:
                        UMI3DLogger.Log($"Case not handled {value.operationId}", scope);
                        break;
                    default:
                        List<PreloadedSceneDto> newList = UMI3DSerializer.ReadList<PreloadedSceneDto>(value.container);
                        List<PreloadedSceneDto> oldList = dto.preloadedScenes;
                        var scenesToUnload = new List<PreloadedSceneDto>();
                        var scenesToLoad = new List<PreloadedSceneDto>();

                        foreach (PreloadedSceneDto newScene in newList)
                        {
                            if (!oldList.Contains(newScene))
                            {
                                scenesToLoad.Add(newScene);
                            }
                        }
                        foreach (PreloadedSceneDto oldScene in oldList)
                        {
                            if (!newList.Contains(oldScene))
                            {
                                scenesToUnload.Add(oldScene);
                            }
                        }

                        foreach (PreloadedSceneDto scene in scenesToLoad)
                            await ReadUMI3DExtension(new ReadUMI3DExtensionData(scene, null));

                        foreach (PreloadedSceneDto scene in scenesToUnload)
                            Unload(scene, null);

                        break;
                }
            }

            return true;
        }

        private static void Unload(PreloadedSceneDto scenesdto, GameObject node)
        {
            SceneManager.UnloadSceneAsync(UMI3DEnvironmentLoader.Parameters.ChooseVariant(scenesdto.scene.variants).pathIfInBundle);
        }
    }
}

