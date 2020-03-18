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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine.Events;

namespace umi3d.edk
{
    /// <summary>
    /// String options selection from UI.
    /// </summary>
    public class StringEnumParameter : AbstractInteraction<EnumParameterDto<string>>
    {
        /// <summary>
        /// Current value.
        /// </summary>
        public string value = null;

        /// <summary>
        /// Availables options.
        /// </summary>
        public List<string> options = new List<string>();

        [System.Serializable]
        public class OnChangeListener : UnityEvent<UMI3DUser, string> { }

        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public OnChangeListener onChange = new OnChangeListener();

        public override EnumParameterDto<string> CreateDto()
        {
            return new EnumParameterDto<string>() { PossibleValues = options, Value = value };
        }

        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt.IsString)
            {
                if (options.Contains(evt.str))
                {
                    value = evt.str;
                    onChange.Invoke(user, value);
                }
                else
                {
                    throw new System.Exception("Option given isn't valid");
                }
            }
            else
            {
                throw new System.Exception("User interaction not supported (should be a string)");
            }
        }
    }
}