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
    public class FloatRangeParameter : AbstractInteraction<FloatRangeParameterDto>
    {
        /// <summary>
        /// Current input value.
        /// </summary>
        public float value = 0;

        /// <summary>
        /// Range's minium value.
        /// </summary>
        public float min = 0;

        /// <summary>
        /// Range's maximum value.
        /// </summary>
        public float max = 1;


        [System.Serializable]
        public class FloatRangeListener : UnityEvent<UMI3DUser, float> { }

        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public FloatRangeListener onChange = new FloatRangeListener();


        public override FloatRangeParameterDto CreateDto()
        {
            return new FloatRangeParameterDto() { Min = min, Max = max, Value = value };
        }

        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt.IsNumber)
            {
                float submitedValue = evt.f;
                if ((submitedValue < min) || (submitedValue > max))
                {
                    throw new System.Exception("Value is out of range");
                }
                else
                {
                    value = submitedValue;
                    onChange.Invoke(user, value);
                }
            }
            else
            {
                throw new System.Exception("User interaction not supported (should be a number)");
            }
        }
    }
}