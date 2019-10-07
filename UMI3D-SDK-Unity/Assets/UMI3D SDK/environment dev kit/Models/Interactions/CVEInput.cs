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
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;


namespace umi3d.edk
{
    /// <summary>
    /// UI Input.
    /// </summary>
    [System.Serializable]
    public class CVEInput
    {
        /// <summary>
        /// Input's ID.
        /// </summary>
        public string key = null;

        /// <summary>
        /// Input's type.
        /// </summary>
        public InputType inputType = null;

        /// <summary>
        /// Input's title.
        /// </summary>
        public string title = null;

        /// <summary>
        /// Input's default value.
        /// </summary>
        public string DefaultValue = null;

        /// <summary>
        /// Input's availables values.
        /// </summary>
        public List<string> ValuesEnum = null;

        /// <summary>
        /// Input's min value.
        /// </summary>
        public string Min;

        /// <summary>
        /// Input's max value.
        /// </summary>
        public string Max;

        /// <summary>
        /// Convert the input to Dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public InputDto ToDto(UMI3DUser user)
        {
            var input = new InputDto();
            input.Key = key;
            input.Title = title;
            input.InputType = inputType;
            input.DefaultValue = DefaultValue;
            input.ValuesEnum = ValuesEnum;
            input.Min = Min;
            input.Max = Max;
            return input;
        }

    }
}
