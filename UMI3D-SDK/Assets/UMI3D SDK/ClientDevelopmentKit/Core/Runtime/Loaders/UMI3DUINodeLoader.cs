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

using inetum.unityUtils;
using System;
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.cdk
{
    public class UMI3DUINodeLoader : UMI3DNodeLoader
    {
        UMI3DUITextNodeLoader textNodeLoader = new UMI3DUITextNodeLoader();
        UMI3DUICanvasNodeLoader canvasNodeLoader = new UMI3DUICanvasNodeLoader();
        UMI3DUIImageNodeLoader imageNodeLoader = new UMI3DUIImageNodeLoader();

        /// <summary>
        /// Load an UMI3D UI.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the UMI3D UI will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {
            base.ReadUMI3DExtension(dto, node, () =>
             {

                 switch (dto)
                 {
                     case UICanvasDto c:
                         canvasNodeLoader.ReadUMI3DExtension(c, node);
                         break;
                     case UIImageDto i:
                         imageNodeLoader.ReadUMI3DExtension(i, node);
                         break;
                     case UITextDto t:
                         textNodeLoader.ReadUMI3DExtension(t, node);
                         break;
                 }
                 RectTransform transform = node.GetOrAddComponent<RectTransform>();
                 UIRectDto rect = dto as UIRectDto;
                 transform.anchoredPosition = rect.anchoredPosition;
                 transform.anchoredPosition3D = rect.anchoredPosition3D;
                 transform.anchorMax = rect.anchorMax;
                 transform.anchorMin = rect.anchorMin;
                 transform.offsetMax = rect.offsetMax;
                 transform.offsetMin = rect.offsetMin;
                 transform.pivot = rect.pivot;
                 transform.sizeDelta = rect.sizeDelta;
                 if (rect.rectMask) node.GetOrAddComponent<RectMask2D>();
                 finished.Invoke();
             }, failed);
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;
            UIRectDto dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UIRectDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.AnchoredPosition:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition = dto.anchoredPosition = (SerializableVector2)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.AnchoredPosition3D:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition3D = dto.anchoredPosition3D = (SerializableVector3)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMax = dto.anchorMax = (SerializableVector2)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMin = dto.anchorMin = (SerializableVector2)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMax = dto.offsetMax = (SerializableVector2)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMin = dto.offsetMin = (SerializableVector2)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.Pivot:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.pivot = dto.pivot = (SerializableVector2)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.SizeDelta:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.sizeDelta = dto.sizeDelta = (SerializableVector2)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.RectMask:
                    dto.rectMask = (bool)property.value;
                    if (dto.rectMask)
                        node.gameObject.GetOrAddComponent<RectMask2D>();
                    else
                    {
                        var mask = node.gameObject.GetComponent<RectMask2D>();
                        if (mask != null)
                            GameObject.Destroy(mask);
                    }
                    break;
                default:
                    switch (dto)
                    {
                        case UICanvasDto c:
                            return canvasNodeLoader.SetUMI3DPorperty(c, node, property);
                        case UIImageDto i:
                            return imageNodeLoader.SetUMI3DPorperty(i, node, property);
                        case UITextDto t:
                            return textNodeLoader.SetUMI3DPorperty(t, node, property);
                        default:
                            return false;
                    }
            }
            return true;
        }

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;
            UIRectDto dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UIRectDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.AnchoredPosition:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition = dto.anchoredPosition = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    }
                    break;
                case UMI3DPropertyKeys.AnchoredPosition3D:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition3D = dto.anchoredPosition3D = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMax = dto.anchorMax = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMin = dto.anchorMin = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMax = dto.offsetMax = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMin = dto.offsetMin = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    }
                    break;
                case UMI3DPropertyKeys.Pivot:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.pivot = dto.pivot = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    }
                    break;
                case UMI3DPropertyKeys.SizeDelta:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.sizeDelta = dto.sizeDelta = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    }
                    break;
                case UMI3DPropertyKeys.RectMask:
                    dto.rectMask = UMI3DNetworkingHelper.Read<bool>(container);
                    if (dto.rectMask)
                        node.gameObject.GetOrAddComponent<RectMask2D>();
                    else
                    {
                        var mask = node.gameObject.GetComponent<RectMask2D>();
                        if (mask != null)
                            GameObject.Destroy(mask);
                    }
                    break;
                default:
                    switch (dto)
                    {
                        case UICanvasDto c:
                            return canvasNodeLoader.SetUMI3DPorperty(c, node, operationId, propertyKey, container);
                        case UIImageDto i:
                            return imageNodeLoader.SetUMI3DPorperty(i, node, operationId, propertyKey, container);
                        case UITextDto t:
                            return textNodeLoader.SetUMI3DPorperty(t, node, operationId, propertyKey, container);
                        default:
                            return false;
                    }
            }
            return true;
        }
    }
}