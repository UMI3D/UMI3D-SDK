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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.interaction.form;
using UnityEngine;

public class ReadDivConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    JsonSerializer styleSerializer;

    public override bool CanConvert(Type objectType)
    {
        return typeof(DivDto).IsAssignableFrom(objectType);
    }

    (string type, IEnumerable<string> args) GetType(string fullType)
    {
        if (fullType is null)
            return (null, null);
        fullType = fullType.Trim();

        int splitIndex = fullType.IndexOf('<');
        if (splitIndex < 0 || fullType.Last() != '>')
            return (fullType, null);

        string part1 = fullType.Substring(0, splitIndex);
        var part2 = fullType.Substring(splitIndex + 1, fullType.Length - splitIndex - 2).Split(',').Select(t => t.Trim()).ToList(); // -2 to remove the last '>'
        return (part1, part2);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JsonSerializerSettings readStyleSerializer = new JsonSerializerSettings() {
            Converters = new List<JsonConverter>() { new ReadStyleConverter() }
        };
        styleSerializer = JsonSerializer.CreateDefault(readStyleSerializer);


        try
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jsonObject = JObject.Load(reader);

            if (jsonObject.TryGetValue("type", out JToken jsonToken))
            {
                string type = jsonToken.ToObject<string>();
                if (type == nameof(ConnectionFormDto))
                    return ReadConnectionFormJson(serializer, jsonObject);

                if (type == nameof(ImageDto))
                    return ReadImageJson(serializer, jsonObject);
                if (type == nameof(LabelDto))
                    return ReadLabelJson(serializer, jsonObject);

                if (type == nameof(PageDto))
                    return ReadPageJson(serializer, jsonObject);
                if (type == nameof(GroupDto))
                    return ReadGroupJson(serializer, jsonObject);

                if (type == nameof(ButtonDto))
                    return ReadButtonJson(serializer, jsonObject);
                /*if (type == nameof(EnumDto))
                    return ReadEnumJson(serializer, jsonObject);*/

                if (type == "InputDto<String>")
                    return ReadInputJson<string>(serializer, jsonObject);
                if (type == "InputDto<Int32>")
                    return ReadInputJson<int>(serializer, jsonObject);
                if (type == "InputDto<Single>")
                    return ReadInputJson<float>(serializer, jsonObject);
                if (type == "InputDto<Boolean>")
                    return ReadInputJson<bool>(serializer, jsonObject);
                if (type == nameof(InputDto<ColorDto>))
                    return ReadInputJson<ColorDto>(serializer, jsonObject);
                if (type == nameof(InputDto<Vector2Dto>))
                    return ReadInputJson<Vector2Dto>(serializer, jsonObject);
                if (type == nameof(InputDto<Vector3Dto>))
                    return ReadInputJson<Vector3Dto>(serializer, jsonObject);
                if (type == nameof(InputDto<Vector4Dto>))
                    return ReadInputJson<Vector4Dto>(serializer, jsonObject);

                if (type == "RangeDto<String>")
                    return ReadRangeDtoJson<string>(serializer, jsonObject);
                if (type == "RangeDto<Int32>")
                    return ReadRangeDtoJson<int>(serializer, jsonObject);
                if (type == "RangeDto<Single>")
                    return ReadRangeDtoJson<float>(serializer, jsonObject);
                if (type == "RangeDto<Boolean>")
                    return ReadRangeDtoJson<bool>(serializer, jsonObject);
                if (type == nameof(RangeDto<ColorDto>))
                    return ReadRangeDtoJson<ColorDto>(serializer, jsonObject);
                if (type == nameof(RangeDto<Vector2Dto>))
                    return ReadRangeDtoJson<Vector2Dto>(serializer, jsonObject);
                if (type == nameof(RangeDto<Vector3Dto>))
                    return ReadRangeDtoJson<Vector3Dto>(serializer, jsonObject);
                if (type == nameof(RangeDto<Vector4Dto>))
                    return ReadRangeDtoJson<Vector4Dto>(serializer, jsonObject);
                Debug.Log(type);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
        return default;
    }

    private object ReadConnectionFormJson(JsonSerializer serializer, JObject jsonObject)
    {
        var connectionFormDto = new ConnectionFormDto();
        if (jsonObject.TryGetValue("globalToken", out var globalToken))
            connectionFormDto.globalToken = globalToken.ToObject<string>();
        if (jsonObject.TryGetValue("metadata", out var metadata))
            connectionFormDto.metadata = metadata.ToObject<byte[]>();

        if (jsonObject.TryGetValue("name", out var name))
            connectionFormDto.name = name.ToObject<string>();
        if (jsonObject.TryGetValue("description", out var description))
            connectionFormDto.description = description.ToObject<string>();

        ReadDivJson(connectionFormDto, jsonObject, serializer);
        return connectionFormDto;
    }
    private object ReadImageJson(JsonSerializer serializer, JObject jsonObject)
    {
        var imageDto = new ImageDto();

        if (jsonObject.TryGetValue("resource", out var ressource))
            imageDto.resource = ressource.ToObject<ResourceDto>();

        ReadDivJson(imageDto, jsonObject, serializer);
        return imageDto;
    }
    private object ReadLabelJson(JsonSerializer serializer, JObject jsonObject)
    {
        var labelDto = new LabelDto();

        if (jsonObject.TryGetValue("text", out var text))
            labelDto.text = text.ToObject<string>();

        ReadDivJson(labelDto, jsonObject, serializer);
        return labelDto;
    }
    private object ReadPageJson(JsonSerializer serializer, JObject jsonObject)
    {
        var pageDto = new PageDto();

        if (jsonObject.TryGetValue("name", out var name))
            pageDto.name = name.ToObject<string>();

        ReadDivJson(pageDto, jsonObject, serializer);
        return pageDto;
    }
    private object ReadButtonJson(JsonSerializer serializer, JObject jsonObject)
    {
        var buttonDto = new ButtonDto();

        if (jsonObject.TryGetValue("Text", out var Text))
            buttonDto.Text = Text.ToObject<string>();
        if (jsonObject.TryGetValue("buttonType", out var buttonType))
            buttonDto.buttonType = buttonType.ToObject<ButtonType>();
        if (jsonObject.TryGetValue("resource", out var ressource))
            buttonDto.resource = ressource.ToObject<ResourceDto>();

        ReadDivJson(buttonDto, jsonObject, serializer);
        ReadBaseInputJson(buttonDto, jsonObject, serializer);
        return buttonDto;
    }
    private object ReadGroupJson(JsonSerializer serializer, JObject jsonObject)
    {
        var groupDto = new GroupDto();

        if (jsonObject.TryGetValue("canRemember", out var canRemember))
            groupDto.canRemember = canRemember.ToObject<bool>();
        if (jsonObject.TryGetValue("selectFirstInput", out var selectFirstInput))
            groupDto.selectFirstInput = selectFirstInput.ToObject<bool>();

        ReadDivJson(groupDto, jsonObject, serializer);
        ReadBaseInputJson(groupDto, jsonObject, serializer);
        return groupDto;
    }
    private object ReadInputJson<T>(JsonSerializer serializer, JObject jsonObject)
    {
        var inputDto = new InputDto<T>();

        if (jsonObject.TryGetValue("Name", out var Name))
            inputDto.Name = Name.ToObject<string>();
        if (jsonObject.TryGetValue("Value", out var Value))
            inputDto.Value = Value.ToObject<T>();
        if (jsonObject.TryGetValue("TextType", out var TextType))
            inputDto.TextType = Value.ToObject<TextType>();
        if (jsonObject.TryGetValue("PlaceHolder", out var PlaceHolder))
            inputDto.PlaceHolder = PlaceHolder.ToObject<T>();

        ReadDivJson(inputDto, jsonObject, serializer);
        ReadBaseInputJson(inputDto, jsonObject, serializer);
        return inputDto;
    }
    private object ReadRangeDtoJson<T>(JsonSerializer serializer, JObject jsonObject)
    {
        var rangeDto = new RangeDto<T>();

        if (jsonObject.TryGetValue("Min", out var Min))
            rangeDto.Min = Min.ToObject<T>();
        if (jsonObject.TryGetValue("Max", out var Max))
            rangeDto.Max = Max.ToObject<T>();
        if (jsonObject.TryGetValue("Value", out var Value))
            rangeDto.Value = Value.ToObject<T>();

        ReadDivJson(rangeDto, jsonObject, serializer);
        ReadBaseInputJson(rangeDto, jsonObject, serializer);
        return rangeDto;
    }

    private void ReadDivJson(DivDto divDto, JObject jsonObject, JsonSerializer serializer)
    {
        if (jsonObject.TryGetValue("guid", out var guid))
            divDto.guid = guid.ToObject<string>();
        if (jsonObject.TryGetValue("tooltip", out var tooltip))
            divDto.tooltip = tooltip.ToObject<string>();

        if (jsonObject.TryGetValue("styles", out var styles))
            divDto.styles = styles?.ToObject<StyleDto[]>(styleSerializer)?.ToList();
        if (jsonObject.TryGetValue("FirstChildren", out var FirstChildren))
            divDto.FirstChildren = FirstChildren?.ToObject<DivDto[]>(serializer)?.ToList();
    }
    private void ReadBaseInputJson(BaseInputDto baseInputDto, JObject jsonObject, JsonSerializer serializer)
    {
        if (jsonObject.TryGetValue("label", out var label))
            baseInputDto.label = label.ToObject<string>();
        if (jsonObject.TryGetValue("isInteractable", out var isInteractable))
            baseInputDto.isInteractable = isInteractable.ToObject<bool>();
        if (jsonObject.TryGetValue("submitOnValidate", out var submitOnValidate))
            baseInputDto.submitOnValidate = submitOnValidate.ToObject<bool>();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
