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
using UnityEngine.Events;

namespace umi3d.edk
{
    public class StringParameter : AbstractInteraction<StringParameterDto>
    {
        /// <summary>
        /// Current input value.
        /// </summary>
        public string value;

        [System.Serializable]
        public class StringListener : UnityEvent<UMI3DUser, string> { }


        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public StringListener onChange = new StringListener();



        public override StringParameterDto CreateDto()
        {
            return new StringParameterDto() { Value = value };
        }

        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt.IsString)
            {
                value = evt.str;
                onChange.Invoke(user, evt.str);
            }
            else
            {
                throw new System.Exception("User interaction not supported (should be boolean)");
            }
        }
    }
}