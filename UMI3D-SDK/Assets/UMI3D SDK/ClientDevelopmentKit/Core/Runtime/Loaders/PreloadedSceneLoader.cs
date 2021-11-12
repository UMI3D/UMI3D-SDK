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

        static void CreatePreloadedScene(PreloadedSceneDto scenesdto, GameObject node)
        {
            ResourceDto resourceScene = scenesdto.scene;

            if (resourceScene != null)
            {
                FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(resourceScene.variants);  // Peut etre ameliore

                string url = fileToLoad.url;
                string ext = fileToLoad.extension;
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                if (loader != null)
                    UMI3DResourcesManager.LoadFile(
                        UMI3DGlobalID.EnvironementId,
                        fileToLoad,
                        loader.UrlToObject,
                        loader.ObjectFromCache,
                        (o) =>
                        {
                            Debug.Log("this scene is going  to be loaded : " + fileToLoad.pathIfInBundle + "   " + o.ToString());
                            SceneManager.LoadSceneAsync((string)o, LoadSceneMode.Additive);

                        },
                        Debug.LogWarning,
                        loader.DeleteObject
                        );
            }
        }

        static void Unload(PreloadedSceneDto scenesdto, GameObject node)
        {
            SceneManager.UnloadSceneAsync((UMI3DEnvironmentLoader.Parameters.ChooseVariante(scenesdto.scene.variants).pathIfInBundle));
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity == null) return false;
            var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d;
            if (dto == null) return false;
            if (property.property == UMI3DPropertyKeys.PreloadedScenes)
                switch (property)
                {
                    case SetEntityListAddPropertyDto add:
                    case SetEntityListRemovePropertyDto rem:
                    case SetEntityListPropertyDto set:
                        Debug.Log($"Case not handled {property}");
                        break;
                    default:
                        var newList = (List<PreloadedSceneDto>)property.value;
                        var oldList = dto.preloadedScenes;
                        var scenesToUnload = new List<PreloadedSceneDto>();
                        var scenesToLoad = new List<PreloadedSceneDto>();

                        foreach (var newScene in newList)
                        {
                            if (!oldList.Contains(newScene))
                            {
                                scenesToLoad.Add(newScene);
                            }
                        }
                        foreach (var oldScene in oldList)
                        {
                            if (!newList.Contains(oldScene))
                            {
                                scenesToUnload.Add(oldScene);
                            }
                        }

                        foreach (var scene in scenesToLoad)
                            CreatePreloadedScene(scene, null);

                        foreach (var scene in scenesToUnload)
                            Unload(scene, null);

                        break;
                }
            return true;
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to update.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (entity == null) return false;
            var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d;
            if (dto == null) return false;
            if (propertyKey == UMI3DPropertyKeys.PreloadedScenes)
                switch (operationId)
                {
                    case UMI3DOperationKeys.SetEntityListAddProperty:
                    case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    case UMI3DOperationKeys.SetEntityListProperty:
                        Debug.Log($"Case not handled {operationId}");
                        break;
                    default:
                        var newList = UMI3DNetworkingHelper.ReadList<PreloadedSceneDto>(container);
                        var oldList = dto.preloadedScenes;
                        var scenesToUnload = new List<PreloadedSceneDto>();
                        var scenesToLoad = new List<PreloadedSceneDto>();

                        foreach (var newScene in newList)
                        {
                            if (!oldList.Contains(newScene))
                            {
                                scenesToLoad.Add(newScene);
                            }
                        }
                        foreach (var oldScene in oldList)
                        {
                            if (!newList.Contains(oldScene))
                            {
                                scenesToUnload.Add(oldScene);
                            }
                        }

                        foreach (var scene in scenesToLoad)
                            CreatePreloadedScene(scene, null);

                        foreach (var scene in scenesToUnload)
                            Unload(scene, null);

                        break;
                }
            return true;
        }

        static public bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            return false;
        }

    }


}

