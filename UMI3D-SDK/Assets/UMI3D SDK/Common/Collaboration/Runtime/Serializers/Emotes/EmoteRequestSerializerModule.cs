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
    /// Serializer for <see cref="EmoteRequestDto"/>.
    /// </summary>
    public class EmoteRequestSerializerModule : UMI3DSerializerModule<EmoteRequestDto>
    {
        /// <inheritdoc/>
        public bool Read(ByteContainer container, out bool readable, out EmoteRequestDto result)
        {
            readable = true;
            result = default;

            readable &= UMI3DSerializer.TryRead(container, out ulong emoteId);
            readable &= UMI3DSerializer.TryRead(container, out bool shouldTrigger);

            if (readable)
            {
                var request = new EmoteRequestDto()
                {
                    emoteId = emoteId,
                    shouldTrigger = shouldTrigger
                };
                result = request;
            }

            return readable;
        }

        /// <inheritdoc/>
        public bool Write(EmoteRequestDto dto, out Bytable bytable, params object[] parameters)
        {
            bytable = UMI3DSerializer.Write(UMI3DOperationKeys.EmoteRequest)
                        + UMI3DSerializer.Write(dto.emoteId)
                        + UMI3DSerializer.Write(dto.shouldTrigger);
            return true;
        }

        /// <inheritdoc/>
        public bool IsCountable()
        {
            return true;
        }
    }
}