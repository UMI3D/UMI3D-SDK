/*
Copyright 2019 - 2023 Inetum

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
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction.form;
using UnityEngine;

namespace EditMode_Tests.Core.Binding.Common
{

    public class FormSerializer_Test 
    {
        [OneTimeSetUp]
        public virtual void InitSerializer()
        {

        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {

        }

        void TestConnectionForm(ConnectionFormDto form, ConnectionFormDto other)
        {
            TestForm(form, other);

            Assert.AreEqual(form.globalToken, other.globalToken);
            Assert.AreEqual(form.metadata, other.metadata);
        }


        void TestForm(FormDto form, FormDto other)
        {
            TestItem(form, other);

            Assert.AreEqual(form.name, other.name);
            Assert.AreEqual(form.description, other.description);
            Assert.AreEqual(form.pages?.Count ?? -1, other.pages?.Count ?? -1);
            if (form.pages != null)
            {
                form.pages.Zip(other.pages, (a, b) => (a, b)).ForEach(c => TestPage(c.a, c.b));
            }
        }

        void TestPage(PageDto page, PageDto other)
        {
            TestItem(page, other);

            Assert.AreEqual(page.name, other.name);
            Assert.AreEqual(page.group is null, other.group is null);
            if (page.group is not null)
                TestGroup(page.group, other.group);
        }

        void TestItem(ItemDto item, ItemDto other)
        {
            Assert.AreEqual(item.id, other.id);
        }

        void TestBaseInput(BaseInputDto input, BaseInputDto other)
        {
            TestItem(input, other);

            Assert.AreEqual(input.label, other.label);
            Assert.AreEqual(input.previousId, other.previousId);
            Assert.AreEqual(input.nextId, other.nextId);
            Assert.AreEqual(input.isInteractable, other.isInteractable);
            Assert.AreEqual(input.submitOnValidate, other.submitOnValidate);
        }

        void TestGroup(GroupDto group, GroupDto other)
        {
            TestBaseInput(group, other);

            Assert.AreEqual(group.canRemember, other.canRemember);
            Assert.AreEqual(group.selectFirstInput, other.selectFirstInput);
            Assert.AreEqual(group.children?.Count ?? 0, other.children?.Count ?? 0);
            if (group.children is not null && group.children.Count > 0)
                group.children.Zip(other.children, (a, b) => (a, b)).ForEach(c => TestDiv(c.a, c.b));
        }

        void TestDiv(DivDto div, DivDto other)
        {
            TestItem(div, other);

            Assert.AreEqual(div.GetType(), other.GetType());
            Assert.AreEqual(div.tooltip, other.tooltip);
            Assert.AreEqual(div.styles?.Count ?? -1, other.styles?.Count ?? -1);
            if (div.styles is not null)
                div.styles.Zip(other.styles, (a, b) => (a, b)).ForEach(c => TestStyle(c.a, c.b));

            switch (div)
            {
                case InputDto<string> s:
                    TestInput(s, other as InputDto<string>);
                    break;
            }
            //need to test inheritedclass
        }

        void TestInput<T>(InputDto<T> input, InputDto<T> other)
        {
            TestBaseInput(input, other);
            Assert.AreEqual(input.tooltip, other.tooltip);
        }


        void TestStyle(StyleDto style, StyleDto other)
        {
            Assert.AreEqual(style.variants?.Count ?? -1, other.variants?.Count ?? -1);
            if (style.variants is not null)
                style.variants.Zip(other.variants, (a, b) => (a, b)).ForEach(c => TestVariantStyle(c.a, c.b));

            //need to test inheritedclass

        }

        void TestVariantStyle(VariantStyleDto style, VariantStyleDto other)
        {
            Assert.AreEqual(style.GetType().Name, other.GetType().Name);
            if(style is PositionStyleDto positionStyle && other is PositionStyleDto otherPosition)
            {
                Assert.AreEqual(positionStyle.right, otherPosition.right);
            }
            //if (style.Variants is not null)
            //    style.Variants.Zip(other.Variants, (a, b) => (a, b)).ForEach(c => TestDiv(c.a, c.b));

            //            public string Tooltip { get; set; }
            //public List<StyleDto> Styles { get; set; }

            //need to test inheritedclass
        }

        [Test]
        public void WriteRead_Form()
        {
            var form = new ConnectionFormDto().FillConnectionForm();

            TestForm(form);
        }

        [Test]
        public void WriteRead_Page()
        {
            var form = new ConnectionFormDto().FillConnectionForm();
            form.pages.Add(new PageDto().FillPage());

            TestForm(form);
        }

        [Test]
        public void WriteRead_Group()
        {
            var form = new ConnectionFormDto().FillConnectionForm();
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());

            form.pages.ForEach(p => p.group = new GroupDto().FillGroup());

            TestForm(form);
        }

        void TestForm(ConnectionFormDto form)
        {
            var _form = form.ToJson(TypeNameHandling.None, new List<JsonConverter>() { new WritteParameterConverter() });
            var dForm = UMI3DDtoSerializer.FromJson<ConnectionFormDto>(_form, converters: new List<JsonConverter>() { new ParameterConverter() });

            TestConnectionForm(form, dForm);
        }


        [Test]
        public void WriteRead_Div()
        {
            var form = new ConnectionFormDto().FillConnectionForm();
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());

            form.pages.Select(p => p.group = new GroupDto().FillGroup()).ForEach(g =>
            {
                g.children.Add(FormExtension.RandomInput<string>());
            });

            TestForm(form);
        }

        [Test]
        public void WriteRead_Style()
        {
            var form = new ConnectionFormDto().FillConnectionForm();
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());

            form.pages.Select(p => p.group = new GroupDto().FillGroup()).ForEach(g =>
            {
                g.children.Add(FormExtension.RandomInput<string>());
                g.styles.Add(new StyleDto() { variants = new List<VariantStyleDto>() { new PositionStyleDto() { right = new StyleLength() {  value = new Length() {  value = 10 , unit = Unit.Percent}, keyword = StyleKeyword.Undefined } } } });
            });

            TestForm(form);
        }

        [Test]
        public void WriteRead_Input()
        {
            var form = new ConnectionFormDto().FillConnectionForm();
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());
            form.pages.Add(new PageDto().FillPage());

            form.pages.Select(p => p.group = new GroupDto().FillGroup()).ForEach(g =>
            {
                g.children.Add(FormExtension.RandomInput<string>("Yeah"));
                g.children.Add(FormExtension.RandomInput<int>(26));
                g.children.Add(FormExtension.RandomInput<bool>(true));
                g.children.Add(FormExtension.RandomInput<ColorDto>((Color.green.Dto())));
                g.children.Add(FormExtension.RandomInput<Vector2Dto>((Vector2.one.Dto())));
                g.children.Add(FormExtension.RandomInput<Vector3Dto>((Vector3.one.Dto())));
                g.children.Add(FormExtension.RandomInput<Vector4Dto>((Vector4.one.Dto())));
                g.children.Add(new ButtonDto().FillButton());
                g.children.Add(new LabelDto().FillLabel());
                g.children.Add(new ImageDto().FillImage());
                g.children.Add(FormExtension.RandomEnum<EnumValue<LabelDto>, LabelDto>(new List<EnumValue<LabelDto>>() {
                    FormExtension.RandomEnumValue<LabelDto>(),
                    FormExtension.RandomEnumValue<LabelDto>(),
                    FormExtension.RandomEnumValue<LabelDto>(),
                }));

            });

            TestForm(form);
        }



    }

    static class FormExtension
    {
        public static ItemDto FillItem(this ItemDto @this)
        {
            @this.id = 43;
            return @this;
        }

        public static ConnectionFormDto FillConnectionForm(this ConnectionFormDto @this)
        {
            @this.FillForm();

            @this.globalToken = "globalToken";
            @this.metadata = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            return @this;
        }

        public static FormDto FillForm(this FormDto @this)
        {
            @this.FillItem();

            @this.name = "Test";
            @this.description = "Test description";
            @this.pages = new List<PageDto>();

            return @this;
        }

        public static PageDto FillPage(this PageDto @this)
        {
            @this.FillItem();

            @this.name = "Test";
            @this.group = null;
            return @this;
        }

        public static DivDto FillDiv(this DivDto @this)
        {
            @this.FillItem();

            @this.tooltip = "This is a tooltip";
            @this.styles = new List<StyleDto>();
            return @this;
        }

        public static BaseInputDto FillBaseInput(this BaseInputDto @this)
        {
            @this.FillDiv();

            @this.label = "Hello";

            @this.previousId = 118;
            @this.nextId = 218;

            @this.isInteractable = true;
            @this.submitOnValidate = true;
            return @this;
        }


        public static GroupDto FillGroup(this GroupDto @this)
        {
            @this.FillBaseInput();

            @this.children = new();
            @this.canRemember = true;
            @this.selectFirstInput = true;

            return @this;
        }

        public static ImageDto FillImage(this ImageDto @this)
        {
            @this.FillDiv();

            @this.resource = new ResourceDto() { variants = new List<FileDto>() { new FileDto() { format = "nice image" } } };

            return @this;
        }

        public static ButtonDto FillButton(this ButtonDto @this)
        {
            @this.FillBaseInput();

            @this.buttonType = ButtonType.Reset;

            return @this;
        }

        public static LabelDto FillLabel(this LabelDto @this)
        {
            @this.FillDiv();

            @this.text = "This is a message";

            return @this;
        }

        public static DivDto RandomInput<T>(T value = default)
        {
            DivDto divDto = new InputDto<T>()
            {
                Value = value,
                PlaceHolder = value
            }.FillBaseInput();
            return divDto;
        }

        public static EnumValue<G> RandomEnumValue<G>(G value = default)
            where G : DivDto
        {
            var divDto = new EnumValue<G>()
            {
                item = value,
                isSelected = true
            };
            return divDto;
        }

        public static DivDto RandomEnum<T, G>(List<T> value = default)
            where T : EnumValue<G>
            where G : DivDto
        {
            DivDto divDto = new EnumDto<T, G>()
            {
                values = value,
                canSelectMultiple = true
            }.FillBaseInput();
            return divDto;
        }

    }

}