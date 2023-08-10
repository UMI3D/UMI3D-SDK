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

using umi3d.common.collaboration.dto.emotes;

namespace umi3d.common.collaboration.emotes
{
    /// <summary>
    /// Serializer module for <see cref="UMI3DEmoteDto"/>.
    /// </summary>
    public class UMI3DEmoteSerializerModule : UMI3DSerializerModule<UMI3DEmoteDto>
    {
        /// <inheritdoc/>
        public bool Read(ByteContainer container, out bool readable, out UMI3DEmoteDto result)
        {
            readable = true;
            result = default;

            readable &= UMI3DSerializer.TryRead(container, out ulong id);
            readable &= UMI3DSerializer.TryRead(container, out string label);
            readable &= UMI3DSerializer.TryRead(container, out ulong animationId);
            readable &= UMI3DSerializer.TryRead(container, out bool available);
            readable &= UMI3DSerializer.TryRead(container, out ResourceDto iconResource);

            if (readable)
            {
                UMI3DEmoteDto e = new()
                {
                    id = id,
                    label = label,
                    animationId = animationId,
                    available = available,
                    iconResource = iconResource
                };
                result = e;
            }

            return readable;
        }

        /// <inheritdoc/>
        public bool Write(UMI3DEmoteDto dto, out Bytable bytable, params object[] parameters)
        {
            bytable = UMI3DSerializer.Write(dto.id)
                + UMI3DSerializer.Write(dto.label)
                + UMI3DSerializer.Write(dto.animationId)
                + UMI3DSerializer.Write(dto.available)
                + UMI3DSerializer.Write(dto.iconResource);

            return true;
        }

        /// <inheritdoc/>
        public bool IsCountable()
        {
            return true;
        }
    }
}