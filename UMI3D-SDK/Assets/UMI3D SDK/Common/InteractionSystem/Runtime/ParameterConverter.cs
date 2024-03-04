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
using umi3d.common.interaction.form;
using UnityEngine.UIElements;

namespace umi3d.common.collaboration
{
    public class ParameterConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

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
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new StyleConverter() }
            };

            JsonSerializer local = JsonSerializer.CreateDefault(settings);
            try
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                var jo = JObject.Load(reader);
                DivDto dto = null;

                if (jo.TryGetValue("type", out JToken tokenB))
                {
                    (string type, IEnumerable<string> args) = GetType(tokenB.ToObject<string>());
                    switch (type)
                    {
                        case "GroupDto":
                        case "GroupScrollViewDto":
                            var tmp = jo.ToObject<BaseInputDto>(local);
                            var _dto = type == "GroupScrollViewDto" ? new GroupScrollViewDto() : new GroupDto()
                            {
                                id = tmp.id,
                                type = tmp.type,
                                tooltip = tmp.tooltip,
                                styles = tmp.styles,
                                label = tmp.label,

                                previousId = tmp.previousId,
                                nextId = tmp.nextId,

                                isInteractable = tmp.isInteractable,
                                submitOnValidate = tmp.submitOnValidate,
                            };

                            if (jo.TryGetValue("canRemember", out JToken cr))
                                _dto.canRemember = cr.ToObject<bool>(local);

                            if (jo.TryGetValue("selectFirstInput", out JToken sfi))
                                _dto.selectFirstInput = sfi.ToObject<bool>(local);

                            if (jo.TryGetValue("children", out JToken children))
                            {
                                if (children is JArray)
                                {
                                    _dto.children = children.ToObject<DivDto[]>(serializer).ToList();
                                }
                                else
                                {
                                    var s = children.ToObject<string>();
                                    var j = JArray.Parse(s);
                                    _dto.children = j.ToObject<DivDto[]>(serializer).ToList();
                                }
                            }
                            else
                                _dto.children = null;

                            if (_dto is GroupScrollViewDto sg && jo.TryGetValue("Mode", out JToken mode))
                                sg.Mode = mode.ToObject<ScrollViewMode>();

                            return _dto;
                        case "InputDto":
                            if (args is null || args.Count() == 0)
                                break;
                            var t = GetInputDtoType(type, args);
                            return jo.ToObject(t, local);
                        case "EnumDto":
                            if (args is null || args.Count() <= 1)
                                break;
                            var t2 = GetEnumDtoType(type, args);
                            return jo.ToObject(t2, local);
                        case "ButtonDto":
                            return jo.ToObject(typeof(ButtonDto), local);
                        case "LabelDto":
                            return jo.ToObject(typeof(LabelDto), local);
                        case "ImageDto":
                            return jo.ToObject(typeof(ImageDto), local);
                        case "RangeDto":
                            if (args is null || args.Count() == 0)
                                break;
                            var t3 = GetRangeDtoType(type, args);
                            return jo.ToObject(t3, local);
                    }

                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            return local.Deserialize(reader, objectType);
        }

        Type GetInputDtoType(string type, IEnumerable<string> args)
        {
            Type t = Type.GetType(args.FirstOrDefault());
            if (t is not null)
            {
                Type gen = typeof(InputDto<>);
                return gen.MakeGenericType(t);
            }

            switch (args.FirstOrDefault())
            {
                case "String":
                    return typeof(InputDto<string>);
                case "Int32":
                    return typeof(InputDto<int>);
                case "Boolean":
                    return typeof(InputDto<bool>);
                case "ColorDto":
                    return typeof(InputDto<ColorDto>);
                case "Vector2Dto":
                    return typeof(InputDto<Vector2Dto>);
                case "Vector3Dto":
                    return typeof(InputDto<Vector3Dto>);
                case "Vector4Dto":
                    return typeof(InputDto<Vector4Dto>);
                default:
                    UnityEngine.Debug.Log("Missing case " + args.FirstOrDefault());
                    break;
            }
            return null;
        }

        Type GetRangeDtoType(string type, IEnumerable<string> args)
        {
            Type t = Type.GetType(args.FirstOrDefault());
            if (t is not null)
            {
                Type gen = typeof(RangeDto<>);
                return gen.MakeGenericType(t);
            }

            switch (args.FirstOrDefault())
            {
                case "String":
                    return typeof(RangeDto<string>);
                case "Int32":
                    return typeof(RangeDto<int>);
                case "Boolean":
                    return typeof(RangeDto<bool>);
                case "ColorDto":
                    return typeof(RangeDto<ColorDto>);
                case "Vector2Dto":
                    return typeof(RangeDto<Vector2Dto>);
                case "Vector3Dto":
                    return typeof(RangeDto<Vector3Dto>);
                case "Vector4Dto":
                    return typeof(RangeDto<Vector4Dto>);
                default:
                    UnityEngine.Debug.Log("Missing case " + args.FirstOrDefault());
                    throw new NotImplementedException();
            }
        }

        Type GetEnumDtoType(string type, IEnumerable<string> args)
        {
            (string type2, IEnumerable<string> args2) = GetType(args.FirstOrDefault());


            (string type3, IEnumerable<string> args3) = GetType(args.Skip(1).FirstOrDefault());
            var g = GetSubEnumDtoType(type3, args3);

            var t = GetSubEnumDtoType(type2, g);

            var gen = typeof(EnumDto<,>);
            UnityEngine.Debug.Assert(t != null);
            UnityEngine.Debug.Assert(g != null);

            return gen.MakeGenericType(t, g);
        }

        Type GetSubEnumDtoType(string type, Type other)
        {
            var gen = typeof(EnumValue<>);
            return gen.MakeGenericType(other);
        }


        Type GetSubEnumDtoType(string type, IEnumerable<string> args)
        {
            Type result = null;
            switch (type)
            {
                case "GroupDto":
                    result = typeof(GroupDto);
                    break;
                case "GroupScrollViewDto":
                    result = typeof(GroupScrollViewDto);
                    break;
                case "InputDto":
                    result = typeof(InputDto<>);
                    break;
                case "EnumDto":
                    result = typeof(EnumDto<,>);
                    break;
                case "ButtonDto":
                    result = typeof(ButtonDto);
                    break;
                case "LabelDto":
                    result = typeof(LabelDto);
                    break;
                case "ImageDto":
                    result = typeof(ImageDto);
                    break;
                case "RangeDto":
                    result = typeof(RangeDto<>);
                    break;
                default:
                    UnityEngine.Debug.Log($"Missing case for -{type}-");
                    throw new NotImplementedException();
            }

            if (result == null)
                return null;

            if (result.IsGenericType && (args?.Count() ?? 0) > 0)
            {
                var sub = args.Select(nt => GetType(nt)).Select(nt => GetSubEnumDtoType(nt.type, nt.args)).ToList();
                if (sub.Count == 1)
                    return result.MakeGenericType(sub[0]);
                if (sub.Count == 2)
                    return result.MakeGenericType(sub[0], sub[1]);
                UnityEngine.Debug.Log($"Missing case for {sub.Count}");
                throw new NotImplementedException();
            }

            return result;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class StyleConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(VariantStyleDto).IsAssignableFrom(objectType);
        }

        string GetType(string fullType)
        {
            if (fullType is null)
                return (null);
            fullType = fullType.Trim();
            return fullType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JsonSerializer local = JsonSerializer.CreateDefault();
            try
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                var jo = JObject.Load(reader);

                if (jo.TryGetValue("type", out JToken tokenB))
                {
                    string type = GetType(tokenB.ToObject<string>());
                    UnityEngine.Debug.Log($"{jo} {tokenB} {type}");
                    switch (type)
                    {
                        case "FlexStyleDto":
                            return jo.ToObject(typeof(FlexStyleDto));
                        case "PositionStyleDto":
                            return jo.ToObject(typeof(PositionStyleDto));
                        case "SizeStyleDto":
                            return jo.ToObject(typeof(SizeStyleDto));
                        default:
                            UnityEngine.Debug.Log($"Missing case for -{type}-");
                            throw new NotImplementedException();
                    }

                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            return local.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}