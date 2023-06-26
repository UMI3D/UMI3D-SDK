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
    /// Serializer for <see cref="BoneBindingDataDto"/>.
    /// </summary>
    public class BoneBindingSerializer : IUMI3DSerializerSubModule<BoneBindingDataDto>
    {
        /// <inheritdoc/>
        public bool Read(ByteContainer container, out BoneBindingDataDto result)
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

            result = readable ?
                new BoneBindingDataDto()
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
                    priority = priority
                }
                : default;

            return readable;
        }

        /// <inheritdoc/>
        public Bytable Write(BoneBindingDataDto simpleBoneBindingDto)
        {
            return UMI3DSerializer.Write(simpleBoneBindingDto.priority)
                        + UMI3DSerializer.Write(simpleBoneBindingDto.partialFit)

                        + UMI3DSerializer.Write(simpleBoneBindingDto.syncRotation)
                        + UMI3DSerializer.Write(simpleBoneBindingDto.syncScale)
                        + UMI3DSerializer.Write(simpleBoneBindingDto.syncPosition)

                        + UMI3DSerializer.Write(simpleBoneBindingDto.offSetPosition)
                        + UMI3DSerializer.Write(simpleBoneBindingDto.offSetRotation)
                        + UMI3DSerializer.Write(simpleBoneBindingDto.offSetScale)

                        + UMI3DSerializer.Write(simpleBoneBindingDto.anchorPosition)

                        + UMI3DSerializer.Write(simpleBoneBindingDto.userId)
                        + UMI3DSerializer.Write(simpleBoneBindingDto.boneType);
        }
    }
}