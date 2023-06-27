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

namespace umi3d.common.userCapture.binding
{
    /// <summary>
    /// Serializer for <see cref="RigBoneBindingDataDto"/>.
    /// </summary>
    public class RigBoneBindingSerializer : IUMI3DSerializerSubModule<RigBoneBindingDataDto>
    {
        /// <inheritdoc/>
        public bool Read(ByteContainer container, out RigBoneBindingDataDto result)
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

            readable &= UMI3DSerializer.TryRead(container, out ulong userId);
            readable &= UMI3DSerializer.TryRead(container, out uint bonetype);

            readable &= UMI3DSerializer.TryRead(container, out string rigName);

            result = readable ?
                new RigBoneBindingDataDto()
                {
                    syncRotation = syncRotation,
                    syncPosition = syncPosition,
                    syncScale = syncScale,
                    offSetPosition = offSetPosition,
                    offSetRotation = offSetRotation,
                    offSetScale = offSetScale,
                    anchorPosition = anchorPosition,
                    userId = userId,
                    boneType = bonetype,
                    partialFit = partialFit,
                    priority = priority,
                    rigName = rigName
                }
                : default;

            return readable;
        }

        /// <inheritdoc/>
        public Bytable Write(RigBoneBindingDataDto rigBindingDataDto)
        {
            return UMI3DSerializer.Write(rigBindingDataDto.priority)
                        + UMI3DSerializer.Write(rigBindingDataDto.partialFit)

                        + UMI3DSerializer.Write(rigBindingDataDto.syncRotation)
                        + UMI3DSerializer.Write(rigBindingDataDto.syncScale)
                        + UMI3DSerializer.Write(rigBindingDataDto.syncPosition)

                        + UMI3DSerializer.Write(rigBindingDataDto.offSetPosition)
                        + UMI3DSerializer.Write(rigBindingDataDto.offSetRotation)
                        + UMI3DSerializer.Write(rigBindingDataDto.offSetScale)

                        + UMI3DSerializer.Write(rigBindingDataDto.anchorPosition)

                        + UMI3DSerializer.Write(rigBindingDataDto.userId)
                        + UMI3DSerializer.Write(rigBindingDataDto.boneType)

                        + UMI3DSerializer.Write(rigBindingDataDto.rigName);
        }
    }
}