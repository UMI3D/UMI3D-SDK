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
using System.Text.RegularExpressions;
using UnityEngine;

namespace inetum.unityUtils.extensions
{
    public static class StringExtensions 
    {
        /// <summary>
        /// Regex to check if a string is an url or not.
        /// </summary>
        static Regex validateURLRegex = new("^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$");

        /// <summary>
        /// Return a valide url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetValideURL(this string url)
        {
            if (validateURLRegex.IsMatch(url))
            {
                // Nothing to do.
            }
            else if (url.EndsWith(".com") || url.EndsWith(".net") || url.EndsWith(".fr") || url.EndsWith(".org"))
            {
                url = "http://" + url;
            }
            else
            {
                url = "https://www.google.com/search?q=" + url;
            }

            return url;
        }
    }
}