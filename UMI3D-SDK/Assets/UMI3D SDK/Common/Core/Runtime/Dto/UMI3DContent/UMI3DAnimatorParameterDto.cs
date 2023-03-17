using System;
using System.Collections;
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
    /// <summary>
    /// Representation of a <see cref="UnityEngine.Animator"/> parameter as they can have different types.
    /// </summary>
    [System.Serializable]
    public class UMI3DAnimatorParameterDto : UMI3DDto, IBytable
    {
        public readonly int type;

        public readonly object value;

        public UMI3DAnimatorParameterDto(object value)
        {
            this.value = value;

            switch (value)
            {
                case long:
                    type = (int)UMI3DAnimatorParameterType.Integer;
                    this.value = (int)((long)this.value % Int32.MaxValue);
                    break;
                case int:
                    type = (int)UMI3DAnimatorParameterType.Integer;
                    break;
                case double:
                    this.value = (float)(double)this.value;
                    type = (int)UMI3DAnimatorParameterType.Float;
                    break;
                case float:
                    type = (int)UMI3DAnimatorParameterType.Float;
                    break;
                case bool:
                    type = (int)UMI3DAnimatorParameterType.Bool;
                    break;
                default:
                    UMI3DLogger.LogError("Animator parameter type not supported " + value.GetType(), DebugScope.Animation);
                    break;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool IsCountable()
        {
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Bytable ToBytableArray(params object[] parameters)
        {
            return UMI3DSerializer.Write(type)
                    + UMI3DSerializer.Write(value);
        }

        /// <summary>
        /// Reads Animator parameter value from bytes.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static object FromByte(ByteContainer container)
        {
            UMI3DAnimatorParameterType type = (UMI3DAnimatorParameterType)UMI3DSerializer.Read<int>(container);

            object value = null;

            switch (type)
            {
                case UMI3DAnimatorParameterType.Bool:
                    value = UMI3DSerializer.Read<bool>(container);
                    break;
                case UMI3DAnimatorParameterType.Float:
                    value = UMI3DSerializer.Read<float>(container);
                    break;
                case UMI3DAnimatorParameterType.Integer:
                    value = UMI3DSerializer.Read<int>(container);
                    break;
            }

            return new UMI3DAnimatorParameterDto(value);
        }
    }


    public enum UMI3DAnimatorParameterType
    {
        Bool, Float, Integer
    }
}