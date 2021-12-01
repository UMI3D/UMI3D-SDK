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
using UnityEngine.SceneManagement;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for Preloade Scene
    /// </summary>
    public class PreloadedSceneLoader
    {
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <summary>
        /// Load PreloadedSceneDto.
        /// </summary>
        /// <param name="dto">preloaded scene dto to load.</param>
        /// <param name="node">node on which the scene should be loaded.</param>
        /// <param name="finished">finish callback.</param>
        /// <param name="failed">error callback.</param>
        public static void ReadUMI3DExtension(PreloadedSceneDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {
            CreatePreloadedScene(dto, node);
            finished?.Invoke();
        }

        private static void CreatePreloadedScene(PreloadedSceneDto scenesdto, GameObject node)
        {
            ResourceDto resourceScene = scenesdto.scene;

            if (resourceScene != null)
            {
                FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(resourceScene.variants);  // Peut etre ameliore

                string url = fileToLoad.url;
                string ext = fileToLoad.extension;
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                if (loader != null)
                {
                    UMI3DResourcesManager.LoadFile(
                        UMI3DGlobalID.EnvironementId,
                        fileToLoad,
                        loader.UrlToObject,
                        loader.ObjectFromCache,
                        (o) =>
                        {
                            UMI3DLogger.Log("this scene is going  to be loaded : " + fileToLoad.pathIfInBundle + "   " + o.ToString(),scope);
                            SceneManager.LoadSceneAsync((string)o, LoadSceneMode.Additive);

                        },
                        e => UMI3DLogger.LogWarning(e,scope),
                        loader.DeleteObject
                        );
                }
            }
        }

        private static void Unload(PreloadedSceneDto scenesdto, GameObject node)
        {
            SceneManager.UnloadSceneAsync((UMI3DEnvironmentLoader.Parameters.ChooseVariant(scenesdto.scene.variants).pathIfInBundle));
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity == null) return false;
            UMI3DEnvironmentDto dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d;
            if (dto == null) return false;
            if (property.property == UMI3DPropertyKeys.PreloadedScenes)
            {
                switch (property)
                {
                    case SetEntityListAddPropertyDto add:
                    case SetEntityListRemovePropertyDto rem:
                    case SetEntityListPropertyDto set:
                        UMI3DLogger.Log($"Case not handled {property}",scope);
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
                            CreatePreloadedScene(scene, null);

                        foreach (PreloadedSceneDto scene in scenesToUnload)
                            Unload(scene, null);

                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (entity == null) return false;
            UMI3DEnvironmentDto dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d;
            if (dto == null) return false;
            if (propertyKey == UMI3DPropertyKeys.PreloadedScenes)
            {
                switch (operationId)
                {
                    case UMI3DOperationKeys.SetEntityListAddProperty:
                    case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    case UMI3DOperationKeys.SetEntityListProperty:
                        UMI3DLogger.Log($"Case not handled {operationId}",scope);
                        break;
                    default:
                        List<PreloadedSceneDto> newList = UMI3DNetworkingHelper.ReadList<PreloadedSceneDto>(container);
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
                            CreatePreloadedScene(scene, null);

                        foreach (PreloadedSceneDto scene in scenesToUnload)
                            Unload(scene, null);

                        break;
                }
            }

            return true;
        }

        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            return false;
        }

    }


}

