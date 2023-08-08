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
    /// Serialiser for <see cref="NodeBindingDataDto"/>.
    /// </summary>
    public class RigNodeBindingDataSerializer : IUMI3DSerializerSubModule<RigNodeBindingDataDto>
    {
        /// <inheritdoc/>
        public virtual bool Read(ByteContainer container, out RigNodeBindingDataDto result)
        {
            bool readable = true;
            readable &= UMI3DSerializer.TryRead(container, out int priority);
            readable &= UMI3DSerializer.TryRead(container, out bool partialFit);

            readable &= UMI3DSerializer.TryRead(container, out bool syncRotation);
            readable &= UMI3DSerializer.TryRead(container, out bool syncScale);
            readable &= UMI3DSerializer.TryRead(container, out bool syncPosition);

            readable &= UMI3DSerializer.TryRead(container, out Vector3Dto offSetPosition);
            readable &= UMI3DSerializer.TryRead(container, out Vector4Dto offSetRotation);
            readable &= UMI3DSerializer.TryRead(container, out Vector3Dto offSetScale);

            readable &= UMI3DSerializer.TryRead(container, out Vector3Dto anchorPosition);

            readable &= UMI3DSerializer.TryRead(container, out ulong nodeId);
            readable &= UMI3DSerializer.TryRead(container, out string rigname);

            result = readable ?
                new RigNodeBindingDataDto()
                {
                    priority = priority,
                    partialFit = partialFit,

                    syncRotation = syncRotation,
                    syncPosition = syncPosition,
                    syncScale = syncScale,

                    offSetPosition = offSetPosition,
                    offSetRotation = offSetRotation,
                    offSetScale = offSetScale,

                    anchorPosition = anchorPosition,

                    parentNodeId = nodeId,
                    rigName = rigname,
                }
                : default;

            return readable;
        }

        /// <inheritdoc/>
        public virtual Bytable Write(RigNodeBindingDataDto nodeBindingDto)
        {
            return UMI3DSerializer.Write(nodeBindingDto.priority)
                        + UMI3DSerializer.Write(nodeBindingDto.partialFit)

                        + UMI3DSerializer.Write(nodeBindingDto.syncRotation)
                        + UMI3DSerializer.Write(nodeBindingDto.syncScale)
                        + UMI3DSerializer.Write(nodeBindingDto.syncPosition)

                        + UMI3DSerializer.Write(nodeBindingDto.offSetPosition)
                        + UMI3DSerializer.Write(nodeBindingDto.offSetRotation)
                        + UMI3DSerializer.Write(nodeBindingDto.offSetScale)

                        + UMI3DSerializer.Write(nodeBindingDto.anchorPosition)

                        + UMI3DSerializer.Write(nodeBindingDto.parentNodeId)

                        + UMI3DSerializer.Write(nodeBindingDto.rigName);


        }
    }
}