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

        public static Vector2 GetTextSize(this TMPro.TMP_Text tmpText, string text, Whitespace whitespace = Whitespace.KeepWhitespace)
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
    }
}