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

using umi3d.common.dto.binding;

namespace umi3d.common.binding
{
    /// <summary>
    /// Serialiser for <see cref="BindingDto"/>.
    /// </summary>
    public class BindingSerializer : IUMI3DSerializerSubModule<BindingDto>
    {
        /// <inheritdoc/>
        public bool Read(ByteContainer container, out BindingDto result)
        {
            bool readable = true;

            readable &= UMI3DSerializer.TryRead(container, out ulong bindingId);
            readable &= UMI3DSerializer.TryRead(container, out ulong boundNodeId);
            readable &= UMI3DSerializer.TryRead(container, out AbstractBindingDataDto bindingData);

            result = readable ?
                new BindingDto()
                {
                    id = bindingId,
                    boundNodeId = boundNodeId,
                    data = bindingData
                }
                : default;

            return readable;
        }

        /// <inheritdoc/>
        public Bytable Write(BindingDto dto)
        {
            return UMI3DSerializer.Write(dto.id)
                    + UMI3DSerializer.Write(dto.boundNodeId)
                    + UMI3DSerializer.Write(dto.data);
        }
    }
}