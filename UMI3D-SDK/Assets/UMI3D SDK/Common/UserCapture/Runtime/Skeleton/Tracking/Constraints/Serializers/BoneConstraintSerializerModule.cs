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

using System;
using umi3d.common.userCapture.description;

namespace umi3d.common.userCapture.tracking.constraint
{
    /// <summary>
    /// Serializer for <see cref="PoseDto"/>, <see cref="PoseAnchorDto"/> and <see cref="PoseAnimatorDto"/>.
    /// </summary>
    public class BoneConstraintSerializerModule : UMI3DSerializerModule
    {
        /// <summary>
        /// Used to serialize inherited types.
        /// </summary>
        public enum ConstraintSerializingIndex : int
        {
            DEFAULT,
            NODE_CONSTRAINED,
            BONE_CONSTRAINED,
            FLOOR_CONSTRAINED
        }

        /// <inheritdoc/>
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(AbstractBoneConstraintDto) => true,
                true when typeof(T) == typeof(NodeBoneConstraintDto) => true,
                true when typeof(T) == typeof(BoneBoneConstraintDto) => true,
                true when typeof(T) == typeof(FloorBoneConstraintDto) => true,

                _ => null
            };
        }

        public bool ReadConstraintDto<ConstraintType>(ByteContainer container, out bool readable, out ConstraintType result) where ConstraintType : AbstractBoneConstraintDto
        {
            switch (true)
            {
                case true when typeof(ConstraintType) == typeof(NodeBoneConstraintDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out ulong id);
                        readable &= UMI3DSerializer.TryRead(container, out bool isActive);
                        readable &= UMI3DSerializer.TryRead(container, out uint bone);
                        readable &= UMI3DSerializer.TryRead(container, out Vector3Dto position);
                        readable &= UMI3DSerializer.TryRead(container, out Vector4Dto rotation);
                        readable &= UMI3DSerializer.TryRead(container, out ulong nodeId);

                        if (readable)
                        {
                            NodeBoneConstraintDto nodePositionAnchoredBonePoseDto = new()
                            {
                                id = id,
                                ShouldBeApplied = isActive,
                                ConstrainedBone = bone,
                                PositionOffset = position,
                                RotationOffset = rotation,
                                ConstrainingNodeId = nodeId
                            };

                            result = (ConstraintType)Convert.ChangeType(nodePositionAnchoredBonePoseDto, typeof(ConstraintType));
                            return true;
                        }
                        break;
                    }
                case true when typeof(ConstraintType) == typeof(BoneBoneConstraintDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out ulong id);
                        readable &= UMI3DSerializer.TryRead(container, out bool isActive);
                        readable &= UMI3DSerializer.TryRead(container, out uint bone);
                        readable &= UMI3DSerializer.TryRead(container, out Vector3Dto position);
                        readable &= UMI3DSerializer.TryRead(container, out Vector4Dto rotation);
                        readable &= UMI3DSerializer.TryRead(container, out uint otherBone);

                        if (readable)
                        {
                            BoneBoneConstraintDto nodePositionAnchoredBonePoseDto = new()
                            {
                                id = id,
                                ShouldBeApplied = isActive,
                                ConstrainedBone = bone,
                                PositionOffset = position,
                                RotationOffset = rotation,
                                ConstrainingBone = otherBone
                            };

                            result = (ConstraintType)Convert.ChangeType(nodePositionAnchoredBonePoseDto, typeof(ConstraintType));
                            return true;
                        }
                        break;
                    }
                case true when typeof(ConstraintType) == typeof(FloorBoneConstraintDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out ulong id);
                        readable &= UMI3DSerializer.TryRead(container, out bool isActive);
                        readable &= UMI3DSerializer.TryRead(container, out uint bone);
                        readable &= UMI3DSerializer.TryRead(container, out Vector3Dto position);
                        readable &= UMI3DSerializer.TryRead(container, out Vector4Dto rotation);

                        if (readable)
                        {
                            FloorBoneConstraintDto nodePositionAnchoredBonePoseDto = new()
                            {
                                id = id,
                                ShouldBeApplied = isActive,
                                ConstrainedBone = bone,
                                PositionOffset = position,
                                RotationOffset = rotation
                            };

                            result = (ConstraintType)Convert.ChangeType(nodePositionAnchoredBonePoseDto, typeof(ConstraintType));
                            return true;
                        }
                        break;
                    }
            }
            result = default(ConstraintType);
            readable = false;
            return false;
        }

        /// <inheritdoc/>
        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(AbstractBoneConstraintDto).IsAssignableFrom(typeof(T)):
                    {
                        readable = UMI3DSerializer.TryRead(container, out int index);

                        if (readable)
                        {
                            AbstractBoneConstraintDto constraintDto = null;

                            switch (index)
                            {
                                case (int)ConstraintSerializingIndex.DEFAULT:
                                    ReadConstraintDto(container, out readable, out constraintDto);
                                    break;

                                case (int)ConstraintSerializingIndex.NODE_CONSTRAINED:
                                    ReadConstraintDto(container, out readable, out NodeBoneConstraintDto nodePositionAnchoredBonePoseDto);
                                    constraintDto = nodePositionAnchoredBonePoseDto;
                                    break;

                                case (int)ConstraintSerializingIndex.BONE_CONSTRAINED:
                                    ReadConstraintDto(container, out readable, out BoneBoneConstraintDto anchorBonePoseDto);
                                    constraintDto = anchorBonePoseDto;
                                    break;

                                case (int)ConstraintSerializingIndex.FLOOR_CONSTRAINED:
                                    ReadConstraintDto(container, out readable, out FloorBoneConstraintDto floorAnchoredBonePoseDto);
                                    constraintDto = floorAnchoredBonePoseDto;
                                    break;
                            }

                            if (constraintDto != null)
                            {
                                result = (T)(object)constraintDto;
                                return true;
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
            result = default(T);
            readable = false;
            return false;
        }

        /// <inheritdoc/>
        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case NodeBoneConstraintDto nodeBoneConstraintDto:
                    bytable = UMI3DSerializer.Write((int)ConstraintSerializingIndex.NODE_CONSTRAINED)
                        + UMI3DSerializer.Write(nodeBoneConstraintDto.id)
                        + UMI3DSerializer.Write(nodeBoneConstraintDto.ShouldBeApplied)
                        + UMI3DSerializer.Write(nodeBoneConstraintDto.ConstrainedBone)
                        + UMI3DSerializer.Write(nodeBoneConstraintDto.PositionOffset)
                        + UMI3DSerializer.Write(nodeBoneConstraintDto.RotationOffset)
                        + UMI3DSerializer.Write(nodeBoneConstraintDto.ConstrainingNodeId);
                    break;

                case BoneBoneConstraintDto boneBoneConstraintDto:
                    bytable = UMI3DSerializer.Write((int)ConstraintSerializingIndex.BONE_CONSTRAINED)
                        + UMI3DSerializer.Write(boneBoneConstraintDto.id)
                        + UMI3DSerializer.Write(boneBoneConstraintDto.ShouldBeApplied)
                        + UMI3DSerializer.Write(boneBoneConstraintDto.ConstrainedBone)
                        + UMI3DSerializer.Write(boneBoneConstraintDto.PositionOffset)
                        + UMI3DSerializer.Write(boneBoneConstraintDto.RotationOffset)
                        + UMI3DSerializer.Write(boneBoneConstraintDto.ConstrainingBone);
                    break;

                case FloorBoneConstraintDto floorBoneConstraintDto:
                    bytable = UMI3DSerializer.Write((int)ConstraintSerializingIndex.FLOOR_CONSTRAINED)
                        + UMI3DSerializer.Write(floorBoneConstraintDto.id)
                        + UMI3DSerializer.Write(floorBoneConstraintDto.ShouldBeApplied)
                        + UMI3DSerializer.Write(floorBoneConstraintDto.ConstrainedBone)
                        + UMI3DSerializer.Write(floorBoneConstraintDto.PositionOffset)
                        + UMI3DSerializer.Write(floorBoneConstraintDto.RotationOffset);
                    break;

                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}