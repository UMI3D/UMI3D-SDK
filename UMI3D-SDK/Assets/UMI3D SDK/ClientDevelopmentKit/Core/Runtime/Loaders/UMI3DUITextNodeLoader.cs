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
    /// <summary>
    /// LOader for an UMI3D UI Text
    /// </summary>
    public class UMI3DUITextNodeLoader
    {
        /// <summary>
        /// Load an UMI3D UI Text.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the UMI3D UI Text will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public void ReadUMI3DExtension(UITextDto dto, GameObject node)
        {
            Text text = node.GetOrAddComponent<Text>();
            text.alignment = dto.alignment.Convert();
            text.alignByGeometry = dto.alignByGeometry;
            text.color = dto.color;
            text.fontSize = dto.fontSize;
            text.fontStyle = dto.fontStyle.Convert();
            text.font = Resources.GetBuiltinResource<Font>(dto.font + ".ttf") as Font;
            text.horizontalOverflow = dto.horizontalOverflow.Convert();
            text.verticalOverflow = dto.verticalOverflow.Convert();
            text.lineSpacing = dto.lineSpacing;
            text.resizeTextForBestFit = dto.resizeTextForBestFit;
            text.resizeTextMaxSize = dto.resizeTextMaxSize;
            text.resizeTextMinSize = dto.resizeTextMinSize;
            text.supportRichText = dto.supportRichText;
            text.text = dto.text;
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public bool SetUMI3DPorperty(UITextDto dto, UMI3DNodeInstance node, SetEntityPropertyDto property)
        {
            switch (property.property)
            {
                case UMI3DPropertyKeys.Alignement:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.alignment = (dto.alignment = (TextAnchorType)(Int64)property.value).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.AlignByGeometry:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.alignByGeometry = dto.alignByGeometry = (bool)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.TextColor:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.color = dto.color = (SerializableColor)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.TextFont:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.font = Resources.GetBuiltinResource<Font>((string)property.value) as Font;
                        dto.font = (string)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.FontSize:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.fontSize = dto.fontSize = (int)(Int64)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.FontStyle:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.fontStyle = (dto.fontStyle = (FontStyleType)(Int64)property.value).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.HorizontalOverflow:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.horizontalOverflow = (dto.horizontalOverflow = (HorizontalWrapType)(Int64)property.value).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.VerticalOverflow:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.verticalOverflow = (dto.verticalOverflow = (VerticalWrapType)(Int64)property.value).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.LineSpacing:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.lineSpacing = dto.lineSpacing = (float)(Double)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.ResizeTextForBestFit:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.resizeTextForBestFit = dto.resizeTextForBestFit = (bool)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.ResizeTextMaxSize:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.resizeTextMaxSize = dto.resizeTextMaxSize = (int)(Int64)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.ResizeTextMinSize:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.resizeTextMinSize = dto.resizeTextMinSize = (int)(Int64)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.SupportRichText:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.supportRichText = dto.supportRichText = (bool)property.value;
                    }
                    break;
                case UMI3DPropertyKeys.Text:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.text = dto.text = (string)property.value;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        public bool SetUMI3DPorperty(UITextDto dto, UMI3DNodeInstance node, uint operationId, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Alignement:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.alignment = (dto.alignment = (TextAnchorType)UMI3DNetworkingHelper.Read<int>(container)).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.AlignByGeometry:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.alignByGeometry = dto.alignByGeometry = UMI3DNetworkingHelper.Read<bool>(container);
                    }
                    break;
                case UMI3DPropertyKeys.TextColor:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.color = dto.color = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    }
                    break;
                case UMI3DPropertyKeys.TextFont:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        var fontName = UMI3DNetworkingHelper.Read<string>(container);
                        text.font = Resources.GetBuiltinResource<Font>(fontName) as Font;
                        dto.font = fontName;
                    }
                    break;
                case UMI3DPropertyKeys.FontSize:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.fontSize = dto.fontSize = UMI3DNetworkingHelper.Read<int>(container);
                    }
                    break;
                case UMI3DPropertyKeys.FontStyle:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.fontStyle = (dto.fontStyle = (FontStyleType)UMI3DNetworkingHelper.Read<int>(container)).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.HorizontalOverflow:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.horizontalOverflow = (dto.horizontalOverflow = (HorizontalWrapType)UMI3DNetworkingHelper.Read<int>(container)).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.VerticalOverflow:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.verticalOverflow = (dto.verticalOverflow = (VerticalWrapType)UMI3DNetworkingHelper.Read<int>(container)).Convert();
                    }
                    break;
                case UMI3DPropertyKeys.LineSpacing:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.lineSpacing = dto.lineSpacing = UMI3DNetworkingHelper.Read<float>(container);
                    }
                    break;
                case UMI3DPropertyKeys.ResizeTextForBestFit:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.resizeTextForBestFit = dto.resizeTextForBestFit = UMI3DNetworkingHelper.Read<bool>(container);
                    }
                    break;
                case UMI3DPropertyKeys.ResizeTextMaxSize:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.resizeTextMaxSize = dto.resizeTextMaxSize = UMI3DNetworkingHelper.Read<int>(container);
                    }
                    break;
                case UMI3DPropertyKeys.ResizeTextMinSize:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.resizeTextMinSize = dto.resizeTextMinSize = UMI3DNetworkingHelper.Read<int>(container);
                    }
                    break;
                case UMI3DPropertyKeys.SupportRichText:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.supportRichText = dto.supportRichText = UMI3DNetworkingHelper.Read<bool>(container);
                    }
                    break;
                case UMI3DPropertyKeys.Text:
                    {
                        Text text = node.gameObject.GetOrAddComponent<Text>();
                        text.text = dto.text = UMI3DNetworkingHelper.Read<string>(container);
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}