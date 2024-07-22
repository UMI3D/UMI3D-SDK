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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using umi3d.common;
using umi3d.common.interaction.form;
using umi3d.common.interaction.form.ugui;

public class ReadStyleConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return typeof(StyleDto).IsAssignableFrom(objectType) || typeof(VariantStyleDto).IsAssignableFrom(objectType)
            || typeof(UGUIStyleItemDto).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jsonObject = JObject.Load(reader);

            if (jsonObject.TryGetValue("variants", out JToken variants))
            {
                var styleDto = new StyleDto();
                styleDto.variants = variants?.ToObject<VariantStyleDto[]>(serializer)?.ToList();
                return styleDto;
            }
            else if (jsonObject.TryGetValue("type", out var typeToken))
            {
                var type = typeToken.ToObject<string>();
                if (type == nameof(UGUIStyleVariantDto))
                    return ReadUGUIStyleVariantJson(jsonObject, serializer);
                if (type == nameof(AnchorStyleDto))
                    return ReadAnchorJson(jsonObject);
                if (type == nameof(ColorStyleDto))
                    return ReadColorJson(jsonObject);
                if (type == nameof(PositionStyleDto))
                    return ReadPositionJson(jsonObject);
                if (type == nameof(SizeStyleDto))
                    return ReadSizeJson(jsonObject);
                if (type == nameof(TextStyleDto))
                    return ReadTextJson(jsonObject, serializer);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
        return default;
    }

    private object ReadTextJson(JObject jsonObject, JsonSerializer serializer)
    {
        var textStyleDto = new TextStyleDto();

        if (jsonObject.TryGetValue("fontSize", out var fontSize))
            textStyleDto.fontSize = fontSize.ToObject<float>();
        if (jsonObject.TryGetValue("color", out var color))
            textStyleDto.color = color.ToObject<ColorStyleDto>(serializer);
        if (jsonObject.TryGetValue("fontStyles", out var fontStyles))
            textStyleDto.fontStyles = fontStyles?.ToObject<E_FontStyle[]>()?.ToList();
        if (jsonObject.TryGetValue("fontAlignments", out var fontAlignments))
            textStyleDto.fontAlignments = fontAlignments?.ToObject<E_FontAlignment[]>()?.ToList();

        return textStyleDto;
    }

    private object ReadSizeJson(JObject jsonObject)
    {
        var sizeStyleDto = new SizeStyleDto();

        if (jsonObject.TryGetValue("width", out var width))
            sizeStyleDto.width = width.ToObject<float>();
        if (jsonObject.TryGetValue("height", out var height))
            sizeStyleDto.height = height.ToObject<float>();

        return sizeStyleDto;
    }

    private object ReadPositionJson(JObject jsonObject)
    {
        var positionStyleDto = new PositionStyleDto();

        if (jsonObject.TryGetValue("posX", out var posX))
            positionStyleDto.posX = posX.ToObject<float>();
        if (jsonObject.TryGetValue("posY", out var posY))
            positionStyleDto.posY = posY.ToObject<float>();
        if (jsonObject.TryGetValue("posZ", out var posZ))
            positionStyleDto.posZ = posZ.ToObject<float>();

        return positionStyleDto;
    }

    private object ReadColorJson(JObject jsonObject)
    {
        var colorStyleDto = new ColorStyleDto();
        if (jsonObject.TryGetValue("color", out var color))
            colorStyleDto.color = color.ToObject<ColorDto>();

        return colorStyleDto;
    }

    private static object ReadAnchorJson(JObject jsonObject)
    {
        var anchorStyleDto = new AnchorStyleDto();

        if (jsonObject.TryGetValue("minX", out var minX))
            anchorStyleDto.minX = minX.ToObject<float>();
        if (jsonObject.TryGetValue("minY", out var minY))
            anchorStyleDto.minY = minY.ToObject<float>();
        if (jsonObject.TryGetValue("maxX", out var maxX))
            anchorStyleDto.maxX = maxX.ToObject<float>();
        if (jsonObject.TryGetValue("maxY", out var maxY))
            anchorStyleDto.maxY = maxY.ToObject<float>();
        if (jsonObject.TryGetValue("pivotX", out var pivotX))
            anchorStyleDto.pivotX = pivotX.ToObject<float>();
        if (jsonObject.TryGetValue("pivotY", out var pivotY))
            anchorStyleDto.pivotY = pivotY.ToObject<float>();

        return anchorStyleDto;
    }

    private static object ReadUGUIStyleVariantJson(JObject jsonObject, JsonSerializer serializer)
    {
        var uguiStyleVariantDto = new UGUIStyleVariantDto();
        if (jsonObject.TryGetValue("StyleVariantItems", out var StyleVariantItems))
            uguiStyleVariantDto.StyleVariantItems = StyleVariantItems?.ToObject<UGUIStyleItemDto[]>(serializer)?.ToList();
        if (jsonObject.TryGetValue("deviceType", out var deviceType))
            uguiStyleVariantDto.deviceType = deviceType.ToObject<umi3d.common.interaction.form.DeviceType>();
        return uguiStyleVariantDto;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}