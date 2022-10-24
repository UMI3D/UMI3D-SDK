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

namespace umi3d.common
{
    /// <summary>
    /// DTO describing a textual component for UI.
    /// </summary>
    /// For Unity, see <see cref="UnityEngine.UI.Text"/>.
    [System.Serializable]
    public class UITextDto : UIRectDto
    {
        /// <summary>
        /// Text anchor alignment relatively to the boxing rectangle.
        /// </summary>
        public TextAnchorType alignment;

        /// <summary>
        /// Should the horizontal aligment be computed using glyph geometery instead of glyph metrics? 
        /// </summary>
        public bool alignByGeometry;

        /// <summary>
        /// Text main color.
        /// </summary>
        public SerializableColor color;

        /// <summary>
        /// Name of the font used.
        /// </summary>
        public string font;

        /// <summary>
        /// Size of the font.
        /// </summary>
        /// Beware to have a size that fits in the boxing rectangle.
        public int fontSize;

        /// <summary>
        /// Font style (bold, italic, ...).
        /// </summary>
        public FontStyleType fontStyle;

        /// <summary>
        /// The way to handle the overflow when the text is horizontally too long.
        /// </summary>
        public HorizontalWrapType horizontalOverflow;

        /// <summary>
        /// Space between two lines, expressed as a factor of the font size.
        /// </summary>
        /// When set to 1, it is normal line spacing.
        public float lineSpacing;

        /// <summary>
        /// Should the text be resized to fit better when required?
        /// </summary>
        public bool resizeTextForBestFit;

        /// <summary>
        /// Maximal size when resizing the text.
        /// </summary>
        /// A value of 1 indicates that the text could be infinitly large.
        public int resizeTextMaxSize;

        /// <summary>
        ///  Minimum size when resizing the text
        /// </summary>
        public int resizeTextMinSize;

        /// <summary>
        /// Does the text supports Rich Text?
        /// </summary>
        /// Rich text handles several markups (bold, italic, color...).
        public bool supportRichText;

        /// <summary>
        /// Textual content to display.
        /// </summary>
        public string text;

        /// <summary>
        /// The way to handle the overflow when the text is vertically too long.
        /// </summary>
        public VerticalWrapType verticalOverflow;


        public UITextDto() : base() { }
    }
}