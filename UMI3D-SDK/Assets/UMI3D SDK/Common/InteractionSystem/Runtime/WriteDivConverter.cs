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
using umi3d.common.interaction.form.ugui;

namespace umi3d.common.collaboration
{
    public class WriteDivConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(DivDto).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        string GetTypeName(Type type)
        {
            UnityEngine.Debug.Log("GetTypeName");
            if (!type.IsGenericType)
                return type.Name;

            string typeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            string typeArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            return $"{typeName}<{typeArgs}>";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var divDto = value as DivDto;
            SetDivAndChildsType(divDto);

            JObject jsonObject = JObject.FromObject(value);
            jsonObject.WriteTo(writer);
        }

        private void SetDivAndChildsType(DivDto divDto)
        {
            if (divDto == null)
                return;

            divDto.type = GetTypeName(divDto.GetType());
            if (divDto.styles != null)
                SetStylesTypes(divDto.styles);

            if (divDto.FirstChildren == null)
                return;

            foreach (var child in divDto.FirstChildren)
            {
                SetDivAndChildsType(child);
            }
        }

        private void SetStylesTypes(List<StyleDto> lstStyleDto)
        {
            foreach (var styleDto in lstStyleDto)
            {
                foreach (var variantStyleDto in styleDto.variants)
                {
                    variantStyleDto.type = GetTypeName(variantStyleDto.GetType());

                    if (variantStyleDto is UGUIStyleVariantDto uguiStyleVariantDto)
                        foreach (var uguiStyleItemDto in uguiStyleVariantDto.StyleVariantItems)
                        {
                            uguiStyleItemDto.type = GetTypeName(uguiStyleItemDto.GetType());
                            if (uguiStyleItemDto is TextStyleDto textStyleItemDto && textStyleItemDto.color != null)
                                textStyleItemDto.color.type = GetTypeName(textStyleItemDto.color.GetType());
                        }
                }
            }
        }
    }
}