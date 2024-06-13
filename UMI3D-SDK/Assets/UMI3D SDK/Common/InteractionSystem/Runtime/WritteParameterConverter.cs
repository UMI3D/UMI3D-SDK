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
using umi3d.common.interaction.form;

namespace umi3d.common.collaboration
{
    public class WritteParameterConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(DivDto).IsAssignableFrom(objectType) || typeof(VariantStyleDto).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        string GetTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            string typeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            string typeArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            return $"{typeName}<{typeArgs}>";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is VariantStyleDto sdto)
            {
                sdto.type = GetTypeName(sdto.GetType());
            }
            else if (value is DivDto dto)
            {
                dto.type = GetTypeName(dto.GetType());
            }

            if (value is GroupDto group)
            {
                var tmp = group.children;
                group.children = null;
                JObject gjo = JObject.FromObject(value);
                if (tmp is not null)
                    gjo["children"] = JToken.FromObject(tmp, serializer); //JsonConvert.SerializeObject(tmp, serializer.Converters.ToArray());
                gjo["styles"] = JToken.FromObject(group.styles, serializer);
                group.children = tmp;
                gjo.WriteTo(writer);
                return;
            }

            JObject jo = JObject.FromObject(value);

            if(value is DivDto divDto)
            {
                jo["styles"] = JToken.FromObject(divDto.styles, serializer);
                //jo["styles"] = JsonConvert.SerializeObject(divDto.styles, serializer.Converters.ToArray());
            }

            //jo["Type"] = "Hello";
            jo.WriteTo(writer);
        }
    }
}