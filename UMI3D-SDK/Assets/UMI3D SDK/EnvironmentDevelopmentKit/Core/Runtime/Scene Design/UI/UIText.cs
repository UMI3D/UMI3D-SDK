﻿/*
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
        public UMI3DAsyncProperty<TextAnchor> Alignment { get { Register(); return _alignment; } protected set => _alignment = value; }

        /// <summary>
        /// Use the extents of glyph geometry to perform horizontal alignment rather than glyph metrics.
        /// </summary>
        public UMI3DAsyncProperty<bool> AlignByGeometry { get { Register(); return _alignByGeometry; } protected set => _alignByGeometry = value; }

        /// <summary>
        /// Text color.
        /// </summary>
        public UMI3DAsyncProperty<Color> TextColor { get { Register(); return _textColor; } protected set => _textColor = value; }

        /// <summary>
        /// Text font.
        /// </summary>
        public UMI3DAsyncProperty<Font> TextFont { get { Register(); return _textFont; } protected set => _textFont = value; }

        /// <summary>
        /// Font size.
        /// </summary>
        public UMI3DAsyncProperty<int> FontSize { get { Register(); return _fontSize; } protected set => _fontSize = value; }

        /// <summary>
        /// Font style.
        /// </summary>
        public UMI3DAsyncProperty<FontStyle> FontStyle { get { Register(); return _fontStyle; } protected set => _fontStyle = value; }

        /// <summary>
        /// Horizontal overflow mode.
        /// </summary>
        public UMI3DAsyncProperty<HorizontalWrapMode> HorizontalOverflow { get { Register(); return _horizontalOverflow; } protected set => _horizontalOverflow = value; }

        /// <summary>
        /// Line spacing.
        /// </summary>
        public UMI3DAsyncProperty<float> LineSpacing { get { Register(); return _lineSpacing; } protected set => _lineSpacing = value; }

        /// <summary>
        /// Auto resize text.
        /// </summary>
        public UMI3DAsyncProperty<bool> ResizeTextForBestFit { get { Register(); return _resizeTextForBestFit; } protected set => _resizeTextForBestFit = value; }

        /// <summary>
        /// Maximum size allowed for text resizing. 
        /// </summary>
        public UMI3DAsyncProperty<int> ResizeTextMaxSize { get { Register(); return _resizeTextMaxSize; } protected set => _resizeTextMaxSize = value; }

        /// <summary>
        /// Minimum size allowed for text resizing. 
        /// </summary>
        public UMI3DAsyncProperty<int> ResizeTextMinSize { get { Register(); return _resizeTextMinSize; } protected set => _resizeTextMinSize = value; }

        /// <summary>
        /// Does the text support rich format.
        /// </summary>
        public UMI3DAsyncProperty<bool> SupportRichText { get { Register(); return _supportRichText; } protected set => _supportRichText = value; }

        /// <summary>
        /// Text content.
        /// </summary>
        public UMI3DAsyncProperty<string> Text { get { Register(); return _text; } protected set => _text = value; }

        /// <summary>
        /// Vertical overflow mode.
        /// </summary>
        public UMI3DAsyncProperty<VerticalWrapMode> VerticalOverflow { get { Register(); return _verticalOverflow; } protected set => _verticalOverflow = value; }


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

        private UMI3DAsyncProperty<TextAnchor> _alignment;
        private UMI3DAsyncProperty<bool> _alignByGeometry;
        private UMI3DAsyncProperty<Color> _textColor;
        private UMI3DAsyncProperty<Font> _textFont;
        private UMI3DAsyncProperty<int> _fontSize;
        private UMI3DAsyncProperty<FontStyle> _fontStyle;
        private UMI3DAsyncProperty<HorizontalWrapMode> _horizontalOverflow;
        private UMI3DAsyncProperty<float> _lineSpacing;
        private UMI3DAsyncProperty<bool> _resizeTextForBestFit;
        private UMI3DAsyncProperty<int> _resizeTextMaxSize;
        private UMI3DAsyncProperty<int> _resizeTextMinSize;
        private UMI3DAsyncProperty<bool> _supportRichText;
        private UMI3DAsyncProperty<string> _text;
        private UMI3DAsyncProperty<VerticalWrapMode> _verticalOverflow;

        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
            Alignment = new UMI3DAsyncProperty<TextAnchor>(objectId, UMI3DPropertyKeys.Alignement, alignment, (a, u) => a.Convert());
            AlignByGeometry = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.AlignByGeometry, alignByGeometry);
            TextColor = new UMI3DAsyncProperty<Color>(objectId, UMI3DPropertyKeys.TextColor, color, ToUMI3DSerializable.ToSerializableColor);
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

        ///<inheritdoc/>
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

        public override Bytable ToBytes(UMI3DUser user)
        {
            var text = Text.GetValue(user);
            if (text != null && text.Length > 0 && text[text.Length - 1] == '\\') text += " ";

            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write((int)Alignment.GetValue(user).Convert())
                + UMI3DNetworkingHelper.Write(AlignByGeometry.GetValue(user))
                + UMI3DNetworkingHelper.Write(TextColor.GetValue(user))
                + UMI3DNetworkingHelper.Write(((Font)TextFont.GetValue(user)).name)
                + UMI3DNetworkingHelper.Write(FontSize.GetValue(user))
                + UMI3DNetworkingHelper.Write((int)FontStyle.GetValue(user).Convert())
                + UMI3DNetworkingHelper.Write((int)HorizontalOverflow.GetValue(user).Convert())
                + UMI3DNetworkingHelper.Write(LineSpacing.GetValue(user))
                + UMI3DNetworkingHelper.Write(ResizeTextForBestFit.GetValue(user))
                + UMI3DNetworkingHelper.Write(ResizeTextMaxSize.GetValue(user))
                + UMI3DNetworkingHelper.Write(ResizeTextMinSize.GetValue(user))
                + UMI3DNetworkingHelper.Write(SupportRichText.GetValue(user))
                + UMI3DNetworkingHelper.Write(text)
                + UMI3DNetworkingHelper.Write((int)VerticalOverflow.GetValue(user).Convert());
        }

    }
}
