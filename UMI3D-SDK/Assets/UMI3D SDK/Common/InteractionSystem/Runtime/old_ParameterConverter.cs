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
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Deserializer for old form parameter
    /// </summary>
    public class Old_ParameterConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AbstractParameterDto);
        }


        public AbstractParameterDto ReadObjectArray(JObject obj, JToken tokenA)
        {
            switch (ReadObjectValue(obj))
            {
                case UnityEngine.Color col:
                    return new EnumParameterDto<UnityEngine.Color>()
                    {
                        possibleValues = tokenA.Values<object>().Select(objA => (UnityEngine.Color)ReadObjectValue(objA as JObject)).ToList(),
                        value = col
                    };
                case Vector4 v4:
                    return new EnumParameterDto<Vector4>()
                    {
                        possibleValues = tokenA.Values<object>().Select(objA => (Vector4)ReadObjectValue(objA as JObject)).ToList(),
                        value = v4
                    };
                case Vector3 v3:
                    return new EnumParameterDto<Vector3>()
                    {
                        possibleValues = tokenA.Values<object>().Select(objA => (Vector3)ReadObjectValue(objA as JObject)).ToList(),
                        value = v3
                    };
                case Vector2 v2:
                    return new EnumParameterDto<Vector2>()
                    {
                        possibleValues = tokenA.Values<object>().Select(objA => (Vector2)ReadObjectValue(objA as JObject)).ToList(),
                        value = v2
                    };
            }
            UnityEngine.Debug.LogError($"Missing case. {obj}");
            return null;
        }

        public AbstractParameterDto ReadObject(JObject obj)
        {
            switch (ReadObjectValue(obj))
            {
                case UnityEngine.Color col:
                    return new ColorParameterDto
                    {
                        value = col.Dto()
                    };
                case Vector4 v4:
                    return new Vector4ParameterDto
                    {
                        value = v4.Dto()
                    };
                case Vector3 v3:
                    return new Vector3ParameterDto
                    {
                        value = v3.Dto()
                    };
                case Vector2 v2:
                    return new Vector2ParameterDto
                    {
                        value = v2.Dto()
                    };
            }
            UnityEngine.Debug.LogError($"Missing case. {obj}");
            return null;
        }

        public object ReadObjectValue(JObject obj)
        {
            if (obj.TryGetValue("R", out JToken tokenR)
                && obj.TryGetValue("G", out JToken tokenG)
                && obj.TryGetValue("B", out JToken tokenB)
                && obj.TryGetValue("A", out JToken tokenA))
            {
                return new UnityEngine.Color(tokenR.ToObject<float>(), tokenG.ToObject<float>(), tokenB.ToObject<float>(), tokenA.ToObject<float>());
            }

            if (obj.TryGetValue("X", out JToken tokenX)
                && obj.TryGetValue("Y", out JToken tokenY))
            {
                if (obj.TryGetValue("Z", out JToken tokenZ))
                {
                    if (obj.TryGetValue("W", out JToken tokenW))
                        return new Vector4(tokenX.ToObject<float>(), tokenY.ToObject<float>(), tokenZ.ToObject<float>(), tokenW.ToObject<float>());
                    return new Vector3(tokenX.ToObject<float>(), tokenY.ToObject<float>(), tokenZ.ToObject<float>());
                }
                return new Vector2(tokenX.ToObject<float>(), tokenY.ToObject<float>());
            }
            UnityEngine.Debug.LogError($"Missing case. {obj}");
            return null;
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            AbstractParameterDto dto = null;
            bool isArray = false;
            isArray = jo.TryGetValue("possibleValues", out JToken tokenA);

            if (jo.TryGetValue("value", out JToken token))
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                        if (isArray)
                            dto = new EnumParameterDto<string>()
                            {
                                possibleValues = tokenA.Values<string>().ToList(),
                                value = token.ToObject<string>()
                            };
                        else
                            dto = new StringParameterDto()
                            {
                                value = token.ToObject<string>()
                            };
                        break;
                    case JTokenType.Boolean:
                        dto = new BooleanParameterDto()
                        {
                            value = token.ToObject<bool>()
                        };
                        break;
                    case JTokenType.Float:
                        if (isArray)
                            dto = new EnumParameterDto<float>()
                            {
                                possibleValues = tokenA.Values<float>().ToList(),
                                value = token.ToObject<float>()
                            };
                        else
                            dto = new FloatParameterDto()
                            {
                                value = token.ToObject<float>()
                            };
                        break;
                    case JTokenType.Integer:
                        if (isArray)
                            dto = new EnumParameterDto<int>()
                            {
                                possibleValues = tokenA.Values<int>().ToList(),
                                value = token.ToObject<int>()
                            };
                        else
                            dto = new IntegerParameterDto()
                            {
                                value = token.ToObject<int>()
                            };
                        break;
                    case JTokenType.Object:
                        var obj = token.ToObject<object>() as JObject;
                        if (isArray)
                            dto = ReadObjectArray(obj, tokenA);
                        else
                            dto = ReadObject(obj);
                        break;
                    default:
                        UnityEngine.Debug.LogError($"TODO Add Case for Color, Range or Vector 2 3 4. {token.Type}");
                        break;
                }
            }
            if (dto == null)
                return null;

            if (jo.TryGetValue("privateParameter", out JToken tokenp))
                dto.privateParameter = tokenp.ToObject<bool>();
            if (jo.TryGetValue("isDisplayer", out JToken tokendisp))
                dto.isDisplayer = tokendisp.ToObject<bool>();
            if (jo.TryGetValue("description", out JToken tokend))
                dto.description = tokend.ToObject<string>();
            if (jo.TryGetValue("id", out JToken tokeni))
                dto.id = (ulong)tokeni.ToObject<int>();
            if (jo.TryGetValue("name", out JToken tokenn))
                dto.name = tokenn.ToObject<string>();
            if (jo.TryGetValue("icon2D", out JToken tokenI2))
                dto.icon2D = tokenI2.ToObject<ResourceDto>();
            if (jo.TryGetValue("icon3D", out JToken tokenI3))
                dto.icon3D = tokenI3.ToObject<ResourceDto>();

            return dto;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}