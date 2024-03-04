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

using NUnit.Framework;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.binding;
using umi3d.common.interaction.form;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using inetum.unityUtils;
using static UnityEditor.Progress;
using UnityEngine.Windows;
using umi3d.common.collaboration;
using System;
using System.Reflection.Emit;

namespace EditMode_Tests.Core.Binding.Common
{

    public class FormSerializer_Test : MonoBehaviour
    {
        [OneTimeSetUp]
        public virtual void InitSerializer()
        {

        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {

        }

        void TestForm(FormDto form,FormDto other)
        {
            TestItem(form, other);

            Assert.AreEqual(form.Name, other.Name);
            Assert.AreEqual(form.Description, other.Description);
            Assert.AreEqual(form.globalToken, other.globalToken);
            Assert.AreEqual(form.metadata, other.metadata);
            Assert.AreEqual(form.Pages?.Count ?? -1, other.Pages?.Count ?? -1);
            if(form.Pages != null)
            {
                form.Pages.Zip(other.Pages, (a, b) => (a, b)).ForEach(c => TestPage(c.a, c.b));
            }
        }

        void TestPage(PageDto page, PageDto other)
        {
            TestItem(page, other);

            Assert.AreEqual(page.Name, other.Name);
            Assert.AreEqual(page.Group is null, other.Group is null);
            if(page.Group is not null)
                TestGroup(page.Group, other.Group);
        }

        void TestItem(ItemDto item, ItemDto other)
        {
            Assert.AreEqual(item.Id, other.Id);
        }

        void TestBaseInput(BaseInputDto input, BaseInputDto other)
        {
            TestItem(input, other);

            Assert.AreEqual(input.Label, other.Label);
            Assert.AreEqual(input.PreviousId, other.PreviousId);
            Assert.AreEqual(input.NextId, other.NextId);
            Assert.AreEqual(input.IsInteractable, other.IsInteractable);
            Assert.AreEqual(input.SubmitOnValidate, other.SubmitOnValidate);
        }

        void TestGroup(GroupDto group, GroupDto other)
        {
            TestBaseInput(group, other);

            Assert.AreEqual(group.CanRemember, other.CanRemember);
            Assert.AreEqual(group.SelectFirstInput, other.SelectFirstInput);
            Assert.AreEqual(group.Children?.Count ?? 0, other.Children?.Count ?? 0);
            if (group.Children is not null && group.Children.Count > 0)
                group.Children.Zip(other.Children, (a, b) => (a, b)).ForEach(c => TestDiv(c.a, c.b));
        }
        
        void TestDiv(DivDto div, DivDto other)
        {
            TestItem(div, other);

            Assert.AreEqual(div.GetType(), other.GetType());
            Assert.AreEqual(div.Tooltip, other.Tooltip);
            Assert.AreEqual(div.Styles?.Count ?? -1, other.Styles?.Count ?? -1);
            if (div.Styles is not null)
                div.Styles.Zip(other.Styles, (a, b) => (a, b)).ForEach(c => TestStyle(c.a, c.b));

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
            Assert.AreEqual(input.Tooltip, other.Tooltip);
        }


        void TestStyle(StyleDto style, StyleDto other)
        {
            Assert.AreEqual(style.Variants?.Count ?? -1, other.Variants?.Count ?? -1);
            if (style.Variants is not null)
                style.Variants.Zip(other.Variants, (a, b) => (a, b)).ForEach(c => TestVariantStyle(c.a, c.b));

            //need to test inheritedclass

        }

        void TestVariantStyle(VariantStyleDto style, VariantStyleDto other)
        {
            //Assert.AreEqual(style.Variants?.Count ?? -1, other.Variants?.Count ?? -1);
            //if (style.Variants is not null)
            //    style.Variants.Zip(other.Variants, (a, b) => (a, b)).ForEach(c => TestDiv(c.a, c.b));

            //            public string Tooltip { get; set; }
            //public List<StyleDto> Styles { get; set; }

            //need to test inheritedclass
        }

        [Test]
        public void WriteRead_Form()
        {
            var form = new FormDto().FillForm();

            TestForm(form);
        }

        [Test]
        public void WriteRead_Page()
        {
            var form = new FormDto().FillForm();
            form.Pages.Add(new PageDto().FillPage());

            TestForm(form);
        }

        [Test]
        public void WriteRead_Group()
        {
            var form = new FormDto().FillForm();
            form.Pages.Add(new PageDto().FillPage());
            form.Pages.Add(new PageDto().FillPage());
            form.Pages.Add(new PageDto().FillPage());

            form.Pages.ForEach(p => p.Group = new GroupDto().FillGroup());

            TestForm(form);
        }

        void TestForm(FormDto form)
        {
            var _form = form.ToJson(TypeNameHandling.None, new List<JsonConverter>() { new WritteParameterConverter()});
            var dForm = UMI3DDtoSerializer.FromJson<FormDto>(_form, converters: new List<JsonConverter>() { new ParameterConverter() });

            TestForm(form, dForm);
        }


        [Test]
        public void WriteRead_Div()
        {
            var form = new FormDto().FillForm();
            form.Pages.Add(new PageDto().FillPage());
            form.Pages.Add(new PageDto().FillPage());
            form.Pages.Add(new PageDto().FillPage());

            form.Pages.Select(p => p.Group = new GroupDto().FillGroup()).ForEach(g => { 
                g.Children.Add(FormExtension.RandomInput<string>());
            });

            TestForm(form);
        }

        [Test]
        public void WriteRead_Input()
        {
            var form = new FormDto().FillForm();
            form.Pages.Add(new PageDto().FillPage());
            form.Pages.Add(new PageDto().FillPage());
            form.Pages.Add(new PageDto().FillPage());

            form.Pages.Select(p => p.Group = new GroupDto().FillGroup()).ForEach(g => {
                g.Children.Add(FormExtension.RandomInput<string>("Yeah"));
                g.Children.Add(FormExtension.RandomInput<int>(26));
                g.Children.Add(FormExtension.RandomInput<bool>(true));
                g.Children.Add(FormExtension.RandomInput<ColorDto>((Color.green.Dto())));
                g.Children.Add(FormExtension.RandomInput<Vector2Dto>((Vector2.one.Dto())));
                g.Children.Add(FormExtension.RandomInput<Vector3Dto>((Vector3.one.Dto())));
                g.Children.Add(FormExtension.RandomInput<Vector4Dto>((Vector4.one.Dto())));
                g.Children.Add(new ButtonDto().FillButton());
                g.Children.Add(new LabelDto().FillLabel());
                g.Children.Add(new ImageDto().FillImage());
                g.Children.Add(FormExtension.RandomEnum<EnumValue<LabelDto>, LabelDto>(new List<EnumValue<LabelDto>>() {
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
            @this.Id = 43;
            return @this;
        }

        public static FormDto FillForm(this FormDto @this)
        {
            @this.FillItem();

            @this.Name = "Test";
            @this.Description = "Test description";
            @this.globalToken = "globalToken";
            @this.metadata = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            @this.Pages = new List<PageDto>();

            return @this;
        }

        public static PageDto FillPage(this PageDto @this)
        {
            @this.FillItem();

            @this.Name = "Test";
            @this.Group = null;
            return @this;
        }

        public static DivDto FillDiv(this DivDto @this)
        {
            @this.FillItem();

            @this.Tooltip = "This is a tooltip";
            return @this;
        }

        public static BaseInputDto FillBaseInput(this BaseInputDto @this)
        {
            @this.FillDiv();

            @this.Label = "Hello";

            @this.PreviousId = 118;
            @this.NextId = 218;

            @this.IsInteractable = true;
            @this.SubmitOnValidate = true;
            return @this;
        }


        public static GroupDto FillGroup(this GroupDto @this)
        {
            @this.FillBaseInput();

            @this.Children = new();
            @this.CanRemember = true;
            @this.SelectFirstInput = true;

            return @this;
        }

        public static ImageDto FillImage(this ImageDto @this)
        {
            @this.FillDiv();

            @this.Resource = new ResourceDto() { variants = new List<FileDto>() { new FileDto() { format = "nice image" } } };

            return @this;
        }

        public static ButtonDto FillButton(this ButtonDto @this)
        {
            @this.FillBaseInput();

            @this.ButtonType = ButtonType.Reset;

            return @this;
        }

        public static LabelDto FillLabel(this LabelDto @this)
        {
            @this.FillDiv();

            @this.Text = "This is a message";

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
                Item = value,
                IsSelected = true
            };
            return divDto;
        }

        public static DivDto RandomEnum<T, G>(List<T> value = default)
            where T : EnumValue<G>
            where G : DivDto
        {
            DivDto divDto = new EnumDto<T,G>()
            {
                Values = value,
                CanSelectMultiple = true
            }.FillBaseInput();
            return divDto;
        }

    }

}