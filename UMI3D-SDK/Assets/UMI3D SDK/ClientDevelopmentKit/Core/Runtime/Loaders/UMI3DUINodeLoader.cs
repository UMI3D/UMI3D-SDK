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
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DNodeDto"/> related to UI.
    /// </summary>
    public class UMI3DUINodeLoader : UMI3DNodeLoader
    {
        private readonly UMI3DUITextNodeLoader textNodeLoader = new UMI3DUITextNodeLoader();
        private readonly UMI3DUICanvasNodeLoader canvasNodeLoader = new UMI3DUICanvasNodeLoader();
        private readonly UMI3DUIImageNodeLoader imageNodeLoader = new UMI3DUIImageNodeLoader();

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UIRectDto && base.CanReadUMI3DExtension(data);
        }

        /// <summary>
        /// Load an UMI3D UI.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the UMI3D UI will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            await base.ReadUMI3DExtension(data);

            switch (data.dto)
            {
                case UICanvasDto c:
                    canvasNodeLoader.ReadUMI3DExtension(c, data.node);
                    break;
                case UIImageDto i:
                    await imageNodeLoader.ReadUMI3DExtension(i, data.node);
                    break;
                case UITextDto t:
                    textNodeLoader.ReadUMI3DExtension(t, data.node);
                    break;
            }
            RectTransform transform = data.node.GetOrAddComponent<RectTransform>();
            var rect = data.dto as UIRectDto;
            transform.anchoredPosition = rect.anchoredPosition.Struct();
            transform.anchoredPosition3D = rect.anchoredPosition3D.Struct();
            transform.anchorMax = rect.anchorMax.Struct();
            transform.anchorMin = rect.anchorMin.Struct();
            transform.offsetMax = rect.offsetMax.Struct();
            transform.offsetMin = rect.offsetMin.Struct();
            transform.pivot = rect.pivot.Struct();
            transform.sizeDelta = rect.sizeDelta.Struct();
            if (rect.rectMask) data.node.GetOrAddComponent<RectMask2D>();
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            var node = data.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UIRectDto;
            if (dto == null) return false;
            switch (data.property.property)
            {
                case UMI3DPropertyKeys.AnchoredPosition:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition = (dto.anchoredPosition = (Vector2Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.AnchoredPosition3D:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition3D = (dto.anchoredPosition3D = (Vector3Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMax = (dto.anchorMax = (Vector2Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMin = (dto.anchorMin = (Vector2Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMax = (dto.offsetMax = (Vector2Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMin = (dto.offsetMin = (Vector2Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.Pivot:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.pivot = (dto.pivot = (Vector2Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.SizeDelta:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.sizeDelta = (dto.sizeDelta = (Vector2Dto)data.property.value).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.RectMask:
                    dto.rectMask = (bool)data.property.value;
                    if (dto.rectMask)
                    {
                        node.gameObject.GetOrAddComponent<RectMask2D>();
                    }
                    else
                    {
                        RectMask2D mask = node.gameObject.GetComponent<RectMask2D>();
                        if (mask != null)
                            GameObject.Destroy(mask);
                    }
                    break;
                default:
                    switch (dto)
                    {
                        case UICanvasDto c:
                            return canvasNodeLoader.SetUMI3DPorperty(c, node, data.property);
                        case UIImageDto i:
                            return imageNodeLoader.SetUMI3DPorperty(i, node, data.property);
                        case UITextDto t:
                            return textNodeLoader.SetUMI3DPorperty(t, node, data.property);
                        default:
                            return false;
                    }
            }
            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            var node = data.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UIRectDto;
            if (dto == null) return false;
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.AnchoredPosition:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition = (dto.anchoredPosition = UMI3DSerializer.Read<Vector2Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.AnchoredPosition3D:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchoredPosition3D = (dto.anchoredPosition3D = UMI3DSerializer.Read<Vector3Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMax = (dto.anchorMax = UMI3DSerializer.Read<Vector2Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.AnchorMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.anchorMin = (dto.anchorMin = UMI3DSerializer.Read<Vector2Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMax:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMax = (dto.offsetMax = UMI3DSerializer.Read<Vector2Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.OffsetMin:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.offsetMin = (dto.offsetMin = UMI3DSerializer.Read<Vector2Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.Pivot:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.pivot = (dto.pivot = UMI3DSerializer.Read<Vector2Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.SizeDelta:
                    {
                        RectTransform transform = node.gameObject.GetOrAddComponent<RectTransform>();
                        transform.sizeDelta = (dto.sizeDelta = UMI3DSerializer.Read<Vector2Dto>(data.container)).Struct();
                    }
                    break;
                case UMI3DPropertyKeys.RectMask:
                    dto.rectMask = UMI3DSerializer.Read<bool>(data.container);
                    if (dto.rectMask)
                    {
                        node.gameObject.GetOrAddComponent<RectMask2D>();
                    }
                    else
                    {
                        RectMask2D mask = node.gameObject.GetComponent<RectMask2D>();
                        if (mask != null)
                            GameObject.Destroy(mask);
                    }
                    break;
                default:
                    switch (dto)
                    {
                        case UICanvasDto c:
                            return canvasNodeLoader.SetUMI3DPorperty(c, node, data.operationId, data.propertyKey, data.container);
                        case UIImageDto i:
                            return imageNodeLoader.SetUMI3DPorperty(i, node, data.operationId, data.propertyKey, data.container);
                        case UITextDto t:
                            return textNodeLoader.SetUMI3DPorperty(t, node, data.operationId, data.propertyKey, data.container);
                        default:
                            return false;
                    }
            }
            return true;
        }
    }
}