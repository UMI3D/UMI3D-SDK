using System;
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

namespace umi3d.common
{
    public static class UMI3DAnimatorParameter
    {
        public static UMI3DAnimatorParameterDto Create(object value)
        {
            var dto = new UMI3DAnimatorParameterDto();
            dto.value = value;

            switch (value)
            {
                case long:
                    dto.type = (int)UMI3DAnimatorParameterType.Integer;
                    dto.value = (int)((long)dto.value % Int32.MaxValue);
                    break;
                case int:
                    dto.type = (int)UMI3DAnimatorParameterType.Integer;
                    break;
                case double:
                    dto.value = (float)(double)dto.value;
                    dto.type = (int)UMI3DAnimatorParameterType.Float;
                    break;
                case float:
                    dto.type = (int)UMI3DAnimatorParameterType.Float;
                    break;
                case bool:
                    dto.type = (int)UMI3DAnimatorParameterType.Bool;
                    break;
                default:
                    UMI3DLogger.LogError("Animator parameter type not supported " + value.GetType(), DebugScope.Animation);
                    break;
            }
            return dto;
        }
    }
}