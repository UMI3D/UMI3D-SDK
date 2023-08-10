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

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Serializer for <see cref="BoneDto"/>.
    /// </summary>
    [UMI3DSerializerOrder(5)]
    public class BoneSerializer : UMI3DSerializerModule
    {
        /// <inheritdoc/>
        public bool? IsCountable<T>()
        {
            return typeof(T) == typeof(BoneDto) ? true : null;
        }

        /// <inheritdoc/>
        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            if (typeof(T) == typeof(BoneDto))
            {
                if (UMI3DSerializer.TryRead(container, out uint type)
                    && UMI3DSerializer.TryRead(container, out Vector4Dto rot))
                {
                    var bone = new BoneDto() { boneType = type, rotation = rot };

                    result = (T)Convert.ChangeType(bone, typeof(T));
                    readable = true;
                    return true;
                }
            }

            result = default(T);
            readable = false;
            return false;
        }

        /// <inheritdoc/>
        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (value is BoneDto c)
            {
                bytable = UMI3DSerializer.Write(c.boneType)
                        + UMI3DSerializer.Write(c.rotation ?? new Vector4Dto());
                return true;
            }

            bytable = null;
            return false;
        }
    }
}