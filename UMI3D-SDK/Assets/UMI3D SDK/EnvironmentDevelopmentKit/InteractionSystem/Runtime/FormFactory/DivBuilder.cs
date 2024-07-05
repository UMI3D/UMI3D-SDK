/*
Copyright 2019 - 2024 Inetum

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
using System.Collections.Generic;
using umi3d.common.interaction.form.ugui;

namespace umi3d.common.interaction.form
{
    public class DivBuilder<T> where T : DivDto, new()
    {
        protected T value;
        private UGUIStyleVariantDto style;
        private TextStyleDto textStyle;

        protected void InstantiateValue()
        {
            value = new T();
        }

        public DivBuilder<T> Position(float x, float y, float z)
        {
            CheckStyleCreated();
            style.StyleVariantItems.Add(new PositionStyleDto() { posX = x, posY = y, posZ = z });

            return this;
        }

        public DivBuilder<T> Position(float x, float y)
        {
            CheckStyleCreated();
            style.StyleVariantItems.Add(new PositionStyleDto() { posX = x, posY = y, posZ = 0 });

            return this;
        }

        public DivBuilder<T> Color(float r, float g, float b, float a)
        {
            CheckStyleCreated();
            style.StyleVariantItems.Add(new ColorStyleDto() { color = new ColorDto() { R = r, G = g, B = b, A = a } });

            return this;
        }

        public DivBuilder<T> Size(float width, float height)
        {
            CheckStyleCreated();
            style.StyleVariantItems.Add(new SizeStyleDto() { width = width, height = height });

            return this;
        }

        public DivBuilder<T> TextSize(float size)
        {
            CheckStyleCreated();
            CheckTextStyleCreated();
            textStyle.fontSize = size;

            return this;
        }

        public DivBuilder<T> TextColor(float r, float g, float b, float a)
        {
            CheckStyleCreated();
            CheckTextStyleCreated();
            textStyle.color = new ColorStyleDto() { color = new ColorDto() { R = r, G = g, B = b, A = a } };

            return this;
        }

        public DivBuilder<T> AddTextStyle(E_FontStyle style)
        {
            CheckStyleCreated();
            CheckTextStyleCreated();
            if (textStyle.fontStyles == null)
                textStyle.fontStyles = new List<E_FontStyle>();
            textStyle.fontStyles.Add(style);

            return this;
        }

        public DivBuilder<T> AddTextAlignement(E_FontAlignment alignement)
        {
            CheckStyleCreated();
            CheckTextStyleCreated();
            if (textStyle.fontAlignments == null)
                textStyle.fontAlignments = new List<E_FontAlignment>();
            textStyle.fontAlignments.Add(alignement);

            return this;
        }

        public DivBuilder<PageDto> AddPage(string id, string label)
        {
            var builder = new PageBuilder(label);
            if (value.FirstChildren == null)
                value.FirstChildren = new List<DivDto>();
            builder.value.guid = id;
            value.FirstChildren.Add(builder.value);
            return builder;
        }

        public LabelBuilder AddLabel(string id, string label)
        {
            var builder = new LabelBuilder(label);
            if (value.FirstChildren == null)
                value.FirstChildren = new List<DivDto>();
            builder.value.guid = id;
            value.FirstChildren.Add(builder.value);
            return builder;
        }

        public ImageBuilder AddImage(string id, string url, string format, AssetMetricDto metrics)
        {
            var builder = new ImageBuilder(url, format, metrics);
            if (value.FirstChildren == null)
                value.FirstChildren = new List<DivDto>();
            builder.value.guid = id;
            value.FirstChildren.Add(builder.value);
            return builder;
        }

        public ButtonBuilder AddButton(string id, string label)
        {
            var builder = new ButtonBuilder(label);
            if (value.FirstChildren == null)
                value.FirstChildren = new List<DivDto>();
            builder.value.guid = id;
            value.FirstChildren.Add(builder.value);
            return builder;
        }

        public GroupBuilder AddGroup(string id)
        {
            var builder = new GroupBuilder();
            if (value.FirstChildren == null)
                value.FirstChildren = new List<DivDto>();
            builder.value.guid = id;
            value.FirstChildren.Add(builder.value);
            return builder;
        }

        public InputBuilder<V> AddInput<V>(string id, string label)
        {
            var builder = new InputBuilder<V>(label);
            if (value.FirstChildren == null)
                value.FirstChildren = new List<DivDto>();
            builder.value.guid = id;
            value.FirstChildren.Add(builder.value);
            return builder;
        }

        public RangeBuilder<V> AddRange<V>(string id, V min, V max)
        {
            var builder = new RangeBuilder<V>(min, max);
            if (value.FirstChildren == null)
                value.FirstChildren = new List<DivDto>();
            builder.value.guid = id;
            value.FirstChildren.Add(builder.value);
            return builder;
        }

        private void CheckStyleCreated()
        {
            if (value.styles == null)
            {
                style = new UGUIStyleVariantDto() {
                    StyleVariantItems = new List<UGUIStyleItemDto>()
                };
                value.styles = new List<StyleDto>() {
                    new StyleDto() {
                        variants = new List<VariantStyleDto>() {
                            style
                        }
                    }
                };
            }
        }

        private void CheckTextStyleCreated()
        {
            if (textStyle == null)
            {
                textStyle = new TextStyleDto();
                style.StyleVariantItems.Add(textStyle);
            }
        }
    }
}