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
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for UMI3D UI Image.
    /// </summary>
    public class UMI3DUIImageNodeLoader
    {
        /// <summary>
        /// Load an UMI3D UI Image.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the UMI3D UI Image will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public void ReadUMI3DExtension(UIImageDto dto, GameObject node)
        {
            Image image = node.GetOrAddComponent<Image>();
            image.color = dto.color;
            image.type = dto.type.Convert();


            if (dto.sprite == null || dto.sprite.variants == null || dto.sprite.variants.Count < 1)
            {
                image.sprite = null;
                dto.sprite = null;
                return;
            }

            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.sprite.variants);

            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            if (loader != null)
                UMI3DResourcesManager.LoadFile(
                    dto.id,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (o) =>
                    {
                        var tex = (Texture2D)o;
                        if (tex != null)
                            image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        else
                            Debug.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}");
                    },
                    Debug.LogWarning,
                    loader.DeleteObject
                    );
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public bool SetUMI3DPorperty(UIImageDto dto, UMI3DNodeInstance node, SetEntityPropertyDto property)
        {
            switch (property.property)
            {
                //Image
                case UMI3DPropertyKeys.ImageColor:
                    {
                        Image image = node.gameObject.GetOrAddComponent<Image>();
                        image.color = dto.color = (SerializableColor)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.ImageType:
                    {
                        Image image = node.gameObject.GetOrAddComponent<Image>();
                        image.type = (dto.type = (ImageType)(Int64)property.value).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.Image:
                    {
                        Image image = node.gameObject.GetOrAddComponent<Image>();
                        dto.sprite = property.value as ResourceDto;
                        FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.sprite?.variants);
                        if (fileToLoad == null)
                        {
                            image.sprite = null;
                            dto.sprite.variants = null;
                            break;
                        }

                        string url = fileToLoad.url;
                        string ext = fileToLoad.extension;
                        string authorization = fileToLoad.authorization;
                        IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                        if (loader != null)
                            UMI3DResourcesManager.LoadFile(
                                dto.id,
                                fileToLoad,
                                loader.UrlToObject,
                                loader.ObjectFromCache,
                                (o) =>
                                {
                                    var tex = (Texture2D)o;
                                    if (tex != null)
                                        image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                    else
                                        Debug.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}");
                                },
                                Debug.LogWarning,
                                loader.DeleteObject
                                );
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        public bool SetUMI3DPorperty(UIImageDto dto, UMI3DNodeInstance node, uint operationId, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                //Image
                case UMI3DPropertyKeys.ImageColor:
                    {
                        Image image = node.gameObject.GetOrAddComponent<Image>();
                        image.color = dto.color = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    }
                    break;
                case UMI3DPropertyKeys.ImageType:
                    {
                        Image image = node.gameObject.GetOrAddComponent<Image>();
                        image.type = (dto.type = (ImageType)UMI3DNetworkingHelper.Read<int>(container)).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.Image:
                    {
                        Image image = node.gameObject.GetOrAddComponent<Image>();
                        dto.sprite = UMI3DNetworkingHelper.Read<ResourceDto>(container);
                        FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.sprite?.variants);
                        if (fileToLoad == null)
                        {
                            image.sprite = null;
                            dto.sprite.variants = null;
                            break;
                        }

                        string url = fileToLoad.url;
                        string ext = fileToLoad.extension;
                        string authorization = fileToLoad.authorization;
                        IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                        if (loader != null)
                            UMI3DResourcesManager.LoadFile(
                                dto.id,
                                fileToLoad,
                                loader.UrlToObject,
                                loader.ObjectFromCache,
                                (o) =>
                                {
                                    var tex = (Texture2D)o;
                                    if (tex != null)
                                        image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                    else
                                        Debug.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}");
                                },
                                Debug.LogWarning,
                                loader.DeleteObject
                                );
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}