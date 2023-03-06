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
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;

            switch (true)
            {
                case true when typeof(T) == typeof(SerializableKeyframe):
                    if (container.length >= 3 * 2 * sizeof(float))
                    {
                        UMI3DSerializer.TryRead(container, out Vector2 v0);
                        UMI3DSerializer.TryRead(container, out Vector2 v1);
                        UMI3DSerializer.TryRead(container, out Vector2 v2);

                        result = (T)Convert.ChangeType(new SerializableKeyframe(v0.x, v0.y, v1.x, v2.x, v1.y, v2.y), typeof(T));
                        return true;
                    }
                    break;
                case true when typeof(T) == typeof(SerializableAnimationCurve):
                    List<SerializableKeyframe> keys = UMI3DSerializer.ReadList<SerializableKeyframe>(container);
                    if (keys != null)
                    {
                        result = (T)Convert.ChangeType(new SerializableAnimationCurve(keys), typeof(T));
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                case true when typeof(T) == typeof(UMI3DAnimatorParameterDto):
                    var parameterValue = UMI3DAnimatorParameterDto.FromByte(container);
                    result = (T)Convert.ChangeType(parameterValue, typeof(T));
                    return true;
            }

            result = default(T);
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable)
        {
            Func<byte[], int, int, (int, int)> f;

            switch (value)
            {
                case AnimationCurve animationCurve:
                    bytable = UMI3DSerializer.Write((SerializableAnimationCurve)animationCurve);
                    return true;
                case SerializableAnimationCurve serializableAnimationCurve:
                    bytable = UMI3DSerializer.WriteCollection(serializableAnimationCurve.keys);
                    return true;
                case Keyframe keyframe:
                    bytable = UMI3DSerializer.Write((SerializableKeyframe)keyframe);
                    return true;
                case SerializableKeyframe serializableFrame:
                    bytable = UMI3DSerializer.Write(serializableFrame.point);
                    bytable += UMI3DSerializer.Write(serializableFrame.intTangeant);
                    bytable += UMI3DSerializer.Write(serializableFrame.outTangeant);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}