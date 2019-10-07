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


        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();
            Alignment = new UMI3DAsyncProperty<TextAnchor>(PropertiesHandler, alignment);
            AlignByGeometry = new UMI3DAsyncProperty<bool>(PropertiesHandler, alignByGeometry);
            TextColor = new UMI3DAsyncProperty<Color>(PropertiesHandler, color);
            TextFont = new UMI3DAsyncProperty<Font>(PropertiesHandler, font);
            FontSize = new UMI3DAsyncProperty<int>(PropertiesHandler, fontSize);
            FontStyle = new UMI3DAsyncProperty<FontStyle>(PropertiesHandler, fontStyle);
            HorizontalOverflow = new UMI3DAsyncProperty<HorizontalWrapMode>(PropertiesHandler, horizontalOverflow);
            LineSpacing = new UMI3DAsyncProperty<float>(PropertiesHandler, lineSpacing);
            ResizeTextForBestFit = new UMI3DAsyncProperty<bool>(PropertiesHandler, resizeTextForBestFit);
            ResizeTextMaxSize = new UMI3DAsyncProperty<int>(PropertiesHandler, resizeTextMaxSize);
            ResizeTextMinSize = new UMI3DAsyncProperty<int>(PropertiesHandler, resizeTextMinSize);
            SupportRichText = new UMI3DAsyncProperty<bool>(PropertiesHandler, supportRichText);
            Text = new UMI3DAsyncProperty<string>(PropertiesHandler, text);
            VerticalOverflow = new UMI3DAsyncProperty<VerticalWrapMode>(PropertiesHandler, verticalOverflow);

    }

        /// <summary>
        /// Update properties.
        /// </summary>
        protected override void SyncProperties()
        {
            base.SyncProperties();
            if (inited)
            {
                Alignment.SetValue(alignment);
                AlignByGeometry.SetValue(alignByGeometry);
                TextColor.SetValue(color);
                TextFont.SetValue(font);
                FontSize.SetValue(fontSize);
                FontStyle.SetValue(fontStyle);
                HorizontalOverflow.SetValue(horizontalOverflow);
                LineSpacing.SetValue(lineSpacing);
                ResizeTextForBestFit.SetValue(resizeTextForBestFit);
                ResizeTextMaxSize.SetValue(resizeTextMaxSize);
                ResizeTextMinSize.SetValue(resizeTextMinSize);
                SupportRichText.SetValue(supportRichText);
                Text.SetValue(text);
                VerticalOverflow.SetValue(verticalOverflow);
            }
        }

        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override UIRectDto CreateDto()
        {
            return new UITextDto();
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override UIRectDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user) as UITextDto;
            dto.alignment = Alignment.GetValue(user);
            dto.alignByGeometry = AlignByGeometry.GetValue(user);
            dto.color = TextColor.GetValue(user);
            dto.font = ((Font)TextFont.GetValue(user)).name;
            dto.fontSize = FontSize.GetValue(user);
            dto.fontStyle = FontStyle.GetValue(user);
            dto.horizontalOverflow = HorizontalOverflow.GetValue(user);
            dto.lineSpacing = LineSpacing.GetValue(user);
            dto.resizeTextForBestFit = ResizeTextForBestFit.GetValue(user);
            dto.resizeTextMaxSize = ResizeTextMaxSize.GetValue(user);
            dto.resizeTextMinSize = ResizeTextMinSize.GetValue(user);
            dto.supportRichText = SupportRichText.GetValue(user);
            dto.text = Text.GetValue(user);
            if (dto.text != null && dto.text.Length > 0 && dto.text[dto.text.Length - 1] == '\\') dto.text += " ";
            dto.verticalOverflow = VerticalOverflow.GetValue(user);
            return dto;
        }

    }
}
