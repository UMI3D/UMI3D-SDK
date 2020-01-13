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
    /// <summary>
    /// Checkbox UI Input.
    /// </summary>
    public class BooleanParameter : AbstractInteraction<BooleanParameterDto>
    {
        /// <summary>
        /// Current input value.
        /// </summary>
        public bool value = false;
        
        [System.Serializable]
        public class CheckboxListener : UnityEvent<UMI3DUser, bool> { }


        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public CheckboxListener onChange = new CheckboxListener();

        /// <summary>
        /// Event raised when value changes to true.
        /// </summary>
        public UMI3DUserEvent onChangeTrue = new UMI3DUserEvent();

        /// <summary>
        /// Event raised when value changes to false.
        /// </summary>
        public UMI3DUserEvent onChangeFalse = new UMI3DUserEvent();


        public override BooleanParameterDto CreateDto()
        {
            return new BooleanParameterDto() { Value = value };
        }

        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt.IsBool)
            {
                value = evt.b;
                onChange.Invoke(user, evt.b);
                if (evt.b)
                    onChangeTrue.Invoke(user);
                else
                    onChangeFalse.Invoke(user);
            }
            else
            {
                throw new System.Exception("User interaction not supported (should be boolean)");
            }
        }
    }
}
