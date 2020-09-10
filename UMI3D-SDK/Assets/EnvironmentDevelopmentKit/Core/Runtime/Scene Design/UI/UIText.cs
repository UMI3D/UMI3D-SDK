/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.edk
{
    /// <summary>
    /// UI Text.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class UIText : UIRect
    {
        /// <summary>
        /// Text aligment.
        /// </summary>
        /// <see cref="TextAnchor"/>
        public UMI3DAsyncProperty<TextAnchor> Alignment;

        /// <summary>
        /// Use the extents of glyph geometry to perform horizontal alignment rather than glyph metrics.
        /// </summary>
        public UMI3DAsyncProperty<bool> AlignByGeometry;

        /// <summary>
        /// Text color.
        /// </summary>
        public UMI3DAsyncProperty<Color> TextColor;

        /// <summary>
        /// Text font.
        /// </summary>
        public UMI3DAsyncProperty<Font> TextFont;

        /// <summary>
        /// Font size.
        /// </summary>
        public UMI3DAsyncProperty<int> FontSize;

        /// <summary>
        /// Font style.
        /// </summary>
        public UMI3DAsyncProperty<FontStyle> FontStyle;

        /// <summary>
        /// Horizontal overflow mode.
        /// </summary>
        public UMI3DAsyncProperty<HorizontalWrapMode> HorizontalOverflow;

        /// <summary>
        /// Line spacing.
        /// </summary>
        public UMI3DAsyncProperty<float> LineSpacing;

        /// <summary>
        /// Auto resize text.
        /// </summary>
        public UMI3DAsyncProperty<bool> ResizeTextForBestFit;

        /// <summary>
        /// Maximum size allowed for text resizing. 
        /// </summary>
        public UMI3DAsyncProperty<int> ResizeTextMaxSize;

        /// <summary>
        /// Minimum size allowed for text resizing. 
        /// </summary>
        public UMI3DAsyncProperty<int> ResizeTextMinSize;

        /// <summary>
        /// Does the text support rich format.
        /// </summary>
        public UMI3DAsyncProperty<bool> SupportRichText;

        /// <summary>
        /// Text content.
        /// </summary>
        public UMI3DAsyncProperty<string> Text;

        /// <summary>
        /// Vertical overflow mode.
        /// </summary>
        public UMI3DAsyncProperty<VerticalWrapMode> VerticalOverflow;


        /// <see cref="Alignment"/>
        TextAnchor alignment { get { return GetComponent<Text>().alignment; } }

        /// <see cref="AlignByGeometry"/>
        bool alignByGeometry { get { return GetComponent<Text>().alignByGeometry; } }

        /// <see cref="CTextColor"/>
        Color color { get { return GetComponent<Text>().color; } }

        /// <see cref="TextFont"/>
        Font font { get { return GetComponent<Text>().font; } }

        /// <see cref="FontSize"/>
        int fontSize { get { return GetComponent<Text>().fontSize; } }

        /// <see cref="FontStyle"/>
        FontStyle fontStyle { get { return GetComponent<Text>().fontStyle; } }

        /// <see cref="TextFont"/>
        HorizontalWrapMode horizontalOverflow { get { return GetComponent<Text>().horizontalOverflow; } }

        /// <see cref="LineSpacing"/>
        float lineSpacing { get { return GetComponent<Text>().lineSpacing; } }

        /// <see cref="ResizeTextForBestFit"/>
        bool resizeTextForBestFit { get { return GetComponent<Text>().resizeTextForBestFit; } }

        /// <see cref="ResizeTextMaxSize"/>
        int resizeTextMaxSize { get { return GetComponent<Text>().resizeTextMaxSize; } }

        /// <see cref="ResizeTextMinSize"/>
        int resizeTextMinSize { get { return GetComponent<Text>().resizeTextMinSize; } }

        /// <see cref="SupportRichText"/>
        bool supportRichText { get { return GetComponent<Text>().supportRichText; } }

        /// <see cref="Text"/>
        string text { get { return GetComponent<Text>().text; } }

        /// <see cref="VerticalOverflow"/>
        VerticalWrapMode verticalOverflow { get { return GetComponent<Text>().verticalOverflow; } }

        UMI3DAsyncPropertyEquality equality = new UMI3DAsyncPropertyEquality();
        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void InitDefinition(string id)
        {
            base.InitDefinition(id);
            Alignment = new UMI3DAsyncProperty<TextAnchor>(objectId, UMI3DPropertyKeys.Alignement, alignment, (a,u)=>a.Convert());
            AlignByGeometry = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.AlignByGeometry, alignByGeometry);
            TextColor = new UMI3DAsyncProperty<Color>(objectId, UMI3DPropertyKeys.AlignByGeometry, color, ToUMI3DSerializable.ToSerializableColor);
            TextFont = new UMI3DAsyncProperty<Font>(objectId, UMI3DPropertyKeys.TextFont, font, (a, u) => a.name);
            FontSize = new UMI3DAsyncProperty<int>(objectId, UMI3DPropertyKeys.FontSize, fontSize);
            FontStyle = new UMI3DAsyncProperty<FontStyle>(objectId, UMI3DPropertyKeys.FontStyle, fontStyle, (a, u) => a.Convert());
            HorizontalOverflow = new UMI3DAsyncProperty<HorizontalWrapMode>(objectId, UMI3DPropertyKeys.HorizontalOverflow, horizontalOverflow, (a, u) => a.Convert());
            LineSpacing = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.LineSpacing, lineSpacing, null, equality.FloatEquality);
            ResizeTextForBestFit = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.ResizeTextForBestFit, resizeTextForBestFit);
            ResizeTextMaxSize = new UMI3DAsyncProperty<int>(objectId, UMI3DPropertyKeys.ResizeTextMaxSize, resizeTextMaxSize);
            ResizeTextMinSize = new UMI3DAsyncProperty<int>(objectId, UMI3DPropertyKeys.ResizeTextMinSize, resizeTextMinSize);
            SupportRichText = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.SupportRichText, supportRichText);
            Text = new UMI3DAsyncProperty<string>(objectId, UMI3DPropertyKeys.Text, text);
            VerticalOverflow = new UMI3DAsyncProperty<VerticalWrapMode>(objectId, UMI3DPropertyKeys.VerticalOverflow, verticalOverflow, (a, u) => a.Convert());
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UITextDto();
        }

        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            UITextDto textDto = dto as UITextDto;
            textDto.alignment = Alignment.GetValue(user).Convert();
            textDto.alignByGeometry = AlignByGeometry.GetValue(user);
            textDto.color = TextColor.GetValue(user);
            textDto.font = ((Font)TextFont.GetValue(user)).name;
            textDto.fontSize = FontSize.GetValue(user);
            textDto.fontStyle = FontStyle.GetValue(user).Convert();
            textDto.horizontalOverflow = HorizontalOverflow.GetValue(user).Convert();
            textDto.lineSpacing = LineSpacing.GetValue(user);
            textDto.resizeTextForBestFit = ResizeTextForBestFit.GetValue(user);
            textDto.resizeTextMaxSize = ResizeTextMaxSize.GetValue(user);
            textDto.resizeTextMinSize = ResizeTextMinSize.GetValue(user);
            textDto.supportRichText = SupportRichText.GetValue(user);
            textDto.text = Text.GetValue(user);
            if (textDto.text != null && textDto.text.Length > 0 && textDto.text[textDto.text.Length - 1] == '\\') textDto.text += " ";
            textDto.verticalOverflow = VerticalOverflow.GetValue(user).Convert();
        }
    }
}
