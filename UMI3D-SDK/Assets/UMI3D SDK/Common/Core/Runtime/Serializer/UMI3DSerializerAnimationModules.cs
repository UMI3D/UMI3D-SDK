/*
Copyright 2019 - 2021 Inetum

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
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    public class UMI3DSerializerAnimationModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(KeyframeDto) => true,
                true when typeof(T) == typeof(AnimationCurveDto) => true,
                true when typeof(T) == typeof(UMI3DAnimatorParameterDto) => true,
                true when typeof(T) == typeof(Keyframe) => true,
                true when typeof(T) == typeof(AnimationCurve) => true,
                _ => null,
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;

            switch (true)
            {
                case true when typeof(T) == typeof(KeyframeDto):
                    if (container.length >= 3 * 2 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out Vector2Dto v0);
                        UMI3DSerializer.TryRead(container, out Vector2Dto v1);
                        UMI3DSerializer.TryRead(container, out Vector2Dto v2);

                        result = (T)Convert.ChangeType(new KeyframeDto() { point = v0, intTangeant = v1, outTangeant = v2 }, typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(AnimationCurveDto):
                    List<KeyframeDto> keys = UMI3DSerializer.ReadList<KeyframeDto>(container);
                    if (keys != null)
                    {
                        result = (T)Convert.ChangeType(new AnimationCurveDto() { keys = keys}, typeof(T));
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                case true when typeof(T) == typeof(UMI3DAnimatorParameterDto):
                    var parameterValue = AnimatorParameterFromByte(container);
                    result = (T)Convert.ChangeType(parameterValue, typeof(T));
                    return true;
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            Func<byte[], int, int, (int, int)> f;

            switch (value)
            {
                case AnimationCurve animationCurve:
                    bytable = UMI3DSerializer.Write(animationCurve.Dto());
                    return true;
                case AnimationCurveDto serializableAnimationCurve:
                    bytable = UMI3DSerializer.WriteCollection(serializableAnimationCurve.keys);
                    return true;
                case Keyframe keyframe:
                    bytable = UMI3DSerializer.Write(keyframe.Dto());
                    return true;
                case KeyframeDto serializableFrame:
                    bytable = UMI3DSerializer.Write(serializableFrame.point);
                    bytable += UMI3DSerializer.Write(serializableFrame.intTangeant);
                    bytable += UMI3DSerializer.Write(serializableFrame.outTangeant);
                    return true;
                case UMI3DAnimatorParameterDto animatorParameter:
                    bytable = UMI3DSerializer.Write(animatorParameter.type)
                            + UMI3DSerializer.Write(animatorParameter.value);
                    return true;
            }
            bytable = null;
            return false;
        }

        /// <summary>
        /// Reads Animator parameter value from bytes.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        static object AnimatorParameterFromByte(ByteContainer container)
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

            return UMI3DAnimatorParameter.Create(value);
        }
    }
}