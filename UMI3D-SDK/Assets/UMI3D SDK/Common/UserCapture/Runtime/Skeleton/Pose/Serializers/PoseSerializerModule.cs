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

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Serializer for <see cref="PoseDto"/>, <see cref="BonePoseDto"/> and <see cref="PoseOverriderDto"/>.
    /// </summary>
    public class PoseSerializerModule : UMI3DSerializerModule
    {
        public enum PoseSerializingIndex : int
        {
            DEFAULT,
            ANCHORED,
            NODE_ANCHORED,
            FLOOR_ANCHORED
        }

        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(AnchoredBonePoseDto) => true,
                true when typeof(T) == typeof(NodeAnchoredBonePoseDto) => true,
                true when typeof(T) == typeof(FloorAnchoredBonePoseDto) => true,
                true when typeof(T) == typeof(BonePoseDto) => true,
                true when typeof(T) == typeof(PoseDto) => true,
                true when typeof(T) == typeof(PoseOverriderDto) => true,
                true when typeof(T) == typeof(DurationDto) => true,

                _ => null
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(PoseDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out int id);
                        var bones = UMI3DSerializer.ReadList<BoneDto>(container);

                        readable &= UMI3DSerializer.TryRead(container, out BonePoseDto boneAnchor);

                        if (readable)
                        {
                            PoseDto poseDto = new PoseDto()
                            {
                                bones = bones,
                                boneAnchor = boneAnchor,
                                index = id
                            };

                            result = (T)Convert.ChangeType(poseDto, typeof(PoseDto));
                            return true;
                        }
                        break;
                    }
                case true when typeof(T) == typeof(PoseOverriderDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out int poseIndex);
                        readable &= UMI3DSerializer.TryRead(container, out DurationDto durationDto);
                        readable &= UMI3DSerializer.TryRead(container, out bool interpolable);
                        readable &= UMI3DSerializer.TryRead(container, out ushort activationMode);
                        readable &= UMI3DSerializer.TryRead(container, out bool composable);

                        AbstractPoseConditionDto[] poseConditionDtos = UMI3DSerializer.ReadArray<AbstractPoseConditionDto>(container);

                        if (readable)
                        {
                            PoseOverriderDto poseOverriderDto = new PoseOverriderDto()
                            {
                                poseIndexInPoseManager = poseIndex,
                                poseConditions = poseConditionDtos,
                                duration = durationDto,
                                activationMode = activationMode,
                                isInterpolable = interpolable,
                                isComposable = composable
                            };

                            result = (T)Convert.ChangeType(poseOverriderDto, typeof(PoseOverriderDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(BonePoseDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out int index);

                        if (readable)
                        {
                            BonePoseDto bonePoseDto = null;

                            switch (index)
                            {
                                case (int)PoseSerializingIndex.DEFAULT:
                                    ReadPoseDto(container, out readable, out bonePoseDto);
                                    break;

                                case (int)PoseSerializingIndex.ANCHORED:
                                    ReadPoseDto(container, out readable, out AnchoredBonePoseDto anchorBonePoseDto);
                                    bonePoseDto = anchorBonePoseDto;
                                    break;

                                case (int)PoseSerializingIndex.NODE_ANCHORED:
                                    ReadPoseDto(container, out readable, out NodeAnchoredBonePoseDto nodePositionAnchoredBonePoseDto);
                                    bonePoseDto = nodePositionAnchoredBonePoseDto;
                                    break;

                                case (int)PoseSerializingIndex.FLOOR_ANCHORED:
                                    ReadPoseDto(container, out readable, out FloorAnchoredBonePoseDto floorAnchoredBonePoseDto);
                                    bonePoseDto = floorAnchoredBonePoseDto;
                                    break;
                            }

                            if (bonePoseDto != null)
                            {
                                result = (T)(object)bonePoseDto;
                                return true;
                            }
                        }
                        break;
                    }

                case true when typeof(T) == typeof(DurationDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out ulong duration);
                        readable &= UMI3DSerializer.TryRead(container, out ulong min);
                        readable &= UMI3DSerializer.TryRead(container, out ulong max);

                        if (readable)
                        {
                            DurationDto durationDto = new DurationDto()
                            {
                                duration = duration,
                                min = min,
                                max = max
                            };

                            result = (T)Convert.ChangeType(durationDto, typeof(DurationDto));
                            return true;
                        }
                        break;
                    }
            }
            result = default(T);
            readable = false;
            return false;
        }

        public bool ReadPoseDto<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(FloorAnchoredBonePoseDto):
                    {
                        ReadPoseDto(container, out readable, out BonePoseDto bonePoseDto);

                        if (readable)
                        {
                            FloorAnchoredBonePoseDto nodePositionAnchoredBonePoseDto = new FloorAnchoredBonePoseDto()
                            {
                                bone = bonePoseDto.bone,
                                position = bonePoseDto.position,
                                rotation = bonePoseDto.rotation,
                            };

                            result = (T)Convert.ChangeType(nodePositionAnchoredBonePoseDto, typeof(FloorAnchoredBonePoseDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(NodeAnchoredBonePoseDto):
                    {
                        ReadPoseDto(container, out readable, out BonePoseDto bonePoseDto);

                        readable &= UMI3DSerializer.TryRead(container, out ulong node);

                        if (readable)
                        {
                            NodeAnchoredBonePoseDto nodeAnchoredBonePoseDto = new NodeAnchoredBonePoseDto()
                            {
                                bone = bonePoseDto.bone,
                                position = bonePoseDto.position,
                                rotation = bonePoseDto.rotation,
                                node = node
                            };

                            result = (T)Convert.ChangeType(nodeAnchoredBonePoseDto, typeof(NodeAnchoredBonePoseDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(AnchoredBonePoseDto):
                    {
                        ReadPoseDto(container, out readable, out BonePoseDto bonePoseDto);

                        readable &= UMI3DSerializer.TryRead(container, out uint otherBone);

                        if (readable)
                        {
                            AnchoredBonePoseDto anchorBonePoseDto = new AnchoredBonePoseDto()
                            {
                                bone = bonePoseDto.bone,
                                position = bonePoseDto.position,
                                rotation = bonePoseDto.rotation,
                                otherBone = otherBone
                            };

                            result = (T)Convert.ChangeType(anchorBonePoseDto, typeof(AnchoredBonePoseDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(BonePoseDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out uint bone);
                        readable &= UMI3DSerializer.TryRead(container, out Vector3Dto position);
                        readable &= UMI3DSerializer.TryRead(container, out Vector4Dto rotation);

                        if (readable)
                        {
                            BonePoseDto bonePoseDto = new BonePoseDto()
                            {
                                bone = bone,
                                position = position,
                                rotation = rotation,
                            };

                            result = (T)Convert.ChangeType(bonePoseDto, typeof(BonePoseDto));
                            return true;
                        }
                        break;
                    }
            }
            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case AnchoredBonePoseDto anchorBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseSerializingIndex.ANCHORED)
                        + UMI3DSerializer.Write(anchorBonePoseDto.bone)
                        + UMI3DSerializer.Write(anchorBonePoseDto.position)
                        + UMI3DSerializer.Write(anchorBonePoseDto.rotation)
                        + UMI3DSerializer.Write(anchorBonePoseDto.otherBone);
                    break;

                case NodeAnchoredBonePoseDto nodePositionAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseSerializingIndex.NODE_ANCHORED)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.bone)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.position)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.rotation)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.node);
                    break;

                case FloorAnchoredBonePoseDto floorAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseSerializingIndex.FLOOR_ANCHORED)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.bone)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.position)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.rotation);
                    break;

                case BonePoseDto bonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseSerializingIndex.DEFAULT)
                        + UMI3DSerializer.Write(bonePoseDto.bone)
                        + UMI3DSerializer.Write(bonePoseDto.position)
                        + UMI3DSerializer.Write(bonePoseDto.rotation);
                    break;

                case PoseDto poseDto:
                    bytable = UMI3DSerializer.Write(poseDto.index)
                        + UMI3DSerializer.WriteCollection(poseDto.bones)
                        + UMI3DSerializer.Write(poseDto.boneAnchor);
                    break;

                case PoseOverriderDto poseOverriderDto:
                    bytable = UMI3DSerializer.Write(poseOverriderDto.poseIndexInPoseManager)
                        + UMI3DSerializer.Write(poseOverriderDto.duration)
                        + UMI3DSerializer.Write(poseOverriderDto.isInterpolable)
                        + UMI3DSerializer.Write(poseOverriderDto.activationMode)
                        + UMI3DSerializer.Write(poseOverriderDto.isComposable)
                        + UMI3DSerializer.WriteCollection(poseOverriderDto.poseConditions);
                    break;

                case DurationDto durationDto:
                    bytable = UMI3DSerializer.Write(durationDto.duration)
                        + UMI3DSerializer.Write(durationDto.min)
                        + UMI3DSerializer.Write(durationDto.max);
                    break;

                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}