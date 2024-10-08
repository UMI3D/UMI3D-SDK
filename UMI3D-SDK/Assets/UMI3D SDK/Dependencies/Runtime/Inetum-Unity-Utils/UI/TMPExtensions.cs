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

using System;
using TMPro;
using UnityEngine;

namespace inetum.unityUtils.ui
{
    public static class TMPExtensions
    {
        public enum Whitespace
        {
            /// <summary>
            /// Whitespace are ignored from computation.
            /// </summary>
            IgnoreWhitespace,
            /// <summary>
            /// Only leading whitespace are ignored.
            /// </summary>
            IgnoreLeadingWhitespace,
            /// <summary>
            /// Only trailing whitespace are ignored.
            /// </summary>
            IgnoreTrailingWhitespace,
            /// <summary>
            /// Whitespace are not ignored from computation.
            /// </summary>
            KeepWhitespace
        }

        public static Vector2 GetTextSize(this TMP_Text tmpText, string text, Whitespace whitespace = Whitespace.KeepWhitespace)
        {
            Vector2 eSize = Vector2.zero;

            if (whitespace != Whitespace.IgnoreWhitespace)
            {
                eSize = tmpText.GetPreferredValues("e");
            }

            switch (whitespace)
            {
                case Whitespace.IgnoreWhitespace:
                    break;
                case Whitespace.IgnoreLeadingWhitespace:
                    text = text + "e";
                    break;
                case Whitespace.IgnoreTrailingWhitespace:
                    text = "e" + text;
                    break;
                case Whitespace.KeepWhitespace:
                    text = $"e{text}e";
                    break;
            }

            Vector2 result = tmpText.GetPreferredValues(text);

            switch (whitespace)
            {
                case Whitespace.IgnoreWhitespace:
                    break;
                case Whitespace.IgnoreLeadingWhitespace:
                case Whitespace.IgnoreTrailingWhitespace:
                    result.x -= eSize.x;
                    break;
                case Whitespace.KeepWhitespace:
                    result.x -= 2 * eSize.x;
                    break;
            }

            return result;
        }

        public static float GetAlignmentOffset(this TMP_InputField inputField)
        {
            TMP_Text textTMP = inputField.textComponent;

            float alignOffset = 0f;
            switch (inputField.textComponent.alignment)
            {
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.Left:
                case TextAlignmentOptions.BottomLeft:
                case TextAlignmentOptions.BaselineLeft:
                case TextAlignmentOptions.MidlineLeft:
                case TextAlignmentOptions.CaplineLeft:
                case TextAlignmentOptions.TopJustified:
                case TextAlignmentOptions.Justified:
                case TextAlignmentOptions.BottomJustified:
                case TextAlignmentOptions.BaselineJustified:
                case TextAlignmentOptions.MidlineJustified:
                case TextAlignmentOptions.CaplineJustified:
                    // The offset is 0;
                    break;
                case TextAlignmentOptions.Top:
                case TextAlignmentOptions.Center:
                case TextAlignmentOptions.Bottom:
                case TextAlignmentOptions.Baseline:
                case TextAlignmentOptions.Midline:
                case TextAlignmentOptions.Capline:
                    alignOffset = (inputField.textViewport.rect.width - textTMP.GetTextSize(inputField.text).x) / 2f;
                    break;
                case TextAlignmentOptions.TopRight:
                case TextAlignmentOptions.Right:
                case TextAlignmentOptions.BottomRight:
                case TextAlignmentOptions.BaselineRight:
                case TextAlignmentOptions.MidlineRight:
                case TextAlignmentOptions.CaplineRight:
                    alignOffset = inputField.textViewport.rect.width - textTMP.GetTextSize(inputField.text).x;
                    break;
                case TextAlignmentOptions.TopFlush:
                case TextAlignmentOptions.Flush:
                case TextAlignmentOptions.BottomFlush:
                case TextAlignmentOptions.BaselineFlush:
                case TextAlignmentOptions.MidlineFlush:
                case TextAlignmentOptions.CaplineFlush:
                    UnityEngine.Debug.LogError($"Not handle case");
                    break;
                case TextAlignmentOptions.TopGeoAligned:
                case TextAlignmentOptions.CenterGeoAligned:
                case TextAlignmentOptions.BottomGeoAligned:
                case TextAlignmentOptions.BaselineGeoAligned:
                case TextAlignmentOptions.MidlineGeoAligned:
                case TextAlignmentOptions.CaplineGeoAligned:
                case TextAlignmentOptions.Converted:
                    UnityEngine.Debug.LogError($"Not handle case");
                    break;
                default:
                    UnityEngine.Debug.LogError($"Not handle case");
                    break;
            }

            return alignOffset;
        }
    }
}