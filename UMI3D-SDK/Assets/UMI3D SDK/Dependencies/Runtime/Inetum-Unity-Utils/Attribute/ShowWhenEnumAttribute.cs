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
using System;
using UnityEngine;

namespace inetum.unityUtils
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowWhenEnumAttribute : PropertyAttribute
    {
        public readonly string enumFieldName;
        public readonly int[] values;

        /// <summary>
        /// Attribute used to show or hide the Field depending on the values of the enum
        /// </summary>
        /// <param name="enumFieldName">Name of the enum Field to compare. (Best use with nameof())</param>
        /// <param name="values">Values to compare</param>
        public ShowWhenEnumAttribute(string enumFieldName, int[] values = null)
        {
            this.enumFieldName = enumFieldName;
            this.values = values;
        }
    }
}
