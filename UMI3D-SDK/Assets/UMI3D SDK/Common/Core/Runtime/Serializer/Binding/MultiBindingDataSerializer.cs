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
    /// Serialiser for <see cref="MultiBindingDataDto"/>.
    /// </summary>
    public class MultiBindingDataSerializer : IUMI3DSerializerSubModule<MultiBindingDataDto>
    {
        /// <inheritdoc/>
        public bool Read(ByteContainer container, out MultiBindingDataDto result)
        {
            bool readable = true;
            readable &= UMI3DSerializer.TryRead(container, out int priority);
            readable &= UMI3DSerializer.TryRead(container, out bool partialFit);

            AbstractSimpleBindingDataDto[] bindings = UMI3DSerializer.ReadArray<AbstractSimpleBindingDataDto>(container);

            result = readable ?
                new MultiBindingDataDto()
                {
                    priority = priority,
                    partialFit = partialFit,
                    Bindings = bindings
                }
                : default;

            return readable;
        }

        /// <inheritdoc/>
        public Bytable Write(MultiBindingDataDto multiBindingDto)
        {
            return UMI3DSerializer.Write(multiBindingDto.priority)
                        + UMI3DSerializer.Write(multiBindingDto.partialFit)
                        + UMI3DSerializer.WriteCollection(multiBindingDto.Bindings);
        }
    }
}