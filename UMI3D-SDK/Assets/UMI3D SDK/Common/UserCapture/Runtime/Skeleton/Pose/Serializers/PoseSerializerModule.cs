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

namespace umi3d.common.userCapture.pose
{
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
                            PoseDto poseDto = new PoseDto(
                                bones: bones,
                                boneAnchor: boneAnchor
                            );
                            poseDto.id = id;

                            result = (T)Convert.ChangeType(poseDto, typeof(PoseDto));
                            return true;
                        }
                        break;
                    }
                case true when typeof(T) == typeof(PoseOverriderDto):
                    {
                        int poseIndex;
                        PoseConditionDto[] poseConditionDtos;
                        DurationDto durationDto;
                        bool interpolationable;
                        bool composable;

                        readable = UMI3DSerializer.TryRead(container, out poseIndex);
                        readable &= UMI3DSerializer.TryRead(container, out durationDto);
                        readable &= UMI3DSerializer.TryRead(container, out interpolationable);
                        readable &= UMI3DSerializer.TryRead(container, out bool isHoverEnter);
                        readable &= UMI3DSerializer.TryRead(container, out bool isHoverExit);
                        readable &= UMI3DSerializer.TryRead(container, out bool isRelease);
                        readable &= UMI3DSerializer.TryRead(container, out bool isTrigger);
                        readable &= UMI3DSerializer.TryRead(container, out composable);
                        poseConditionDtos = UMI3DSerializer.ReadArray<PoseConditionDto>(container);

                        if (readable)
                        {
                            PoseOverriderDto poseOverriderDto = new PoseOverriderDto(
                                poseIndexinPoseManager: poseIndex,
                                poseConditionDtos: poseConditionDtos,
                                isHoverEnter: isHoverEnter,
                                isHoverExit: isHoverExit,
                                isRelease: isRelease,
                                isTrigger: isTrigger,
                                duration: durationDto,
                                interpolationable: interpolationable,
                                composable: composable
                            );

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
                            DurationDto durationDto = new DurationDto(
                                duration: duration,
                                min: min,
                                max: max
                            );

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
                            FloorAnchoredBonePoseDto nodePositionAnchoredBonePoseDto = new FloorAnchoredBonePoseDto(
                                bonePoseDto: bonePoseDto
                            );

                            result = (T)Convert.ChangeType(nodePositionAnchoredBonePoseDto, typeof(FloorAnchoredBonePoseDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(NodeAnchoredBonePoseDto):
                    {
                        ReadPoseDto(container, out readable, out BonePoseDto bonePoseDto);

                        readable &= UMI3DSerializer.TryRead(container, out uint node);

                        if (readable)
                        {
                            NodeAnchoredBonePoseDto nodeAnchoredBonePoseDto = new NodeAnchoredBonePoseDto(
                                bonePoseDto: bonePoseDto,
                                node: node
                            );

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
                            AnchoredBonePoseDto anchorBonePoseDto = new AnchoredBonePoseDto(
                                bonePoseDto: bonePoseDto,
                                otherBone: otherBone
                            );

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
                            BonePoseDto bonePoseDto = new BonePoseDto(
                                bone: bone,
                                position: position,
                                rotation: rotation
                            );

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
                        + UMI3DSerializer.Write(anchorBonePoseDto.Bone)
                        + UMI3DSerializer.Write(anchorBonePoseDto.Position)
                        + UMI3DSerializer.Write(anchorBonePoseDto.Rotation)
                        + UMI3DSerializer.Write(anchorBonePoseDto.otherBone);
                    break;

                case NodeAnchoredBonePoseDto nodePositionAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseSerializingIndex.NODE_ANCHORED)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.Bone)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.Position)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.Rotation)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.node);
                    break;

                case FloorAnchoredBonePoseDto floorAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseSerializingIndex.FLOOR_ANCHORED)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.Bone)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.Position)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.Rotation);
                    break;

                case BonePoseDto bonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseSerializingIndex.DEFAULT)
                        + UMI3DSerializer.Write(bonePoseDto.Bone)
                        + UMI3DSerializer.Write(bonePoseDto.Position)
                        + UMI3DSerializer.Write(bonePoseDto.Rotation);
                    break;

                case PoseDto poseDto:
                    bytable = UMI3DSerializer.Write(poseDto.id)
                        + UMI3DSerializer.WriteCollection(poseDto.bones)
                        + UMI3DSerializer.Write(poseDto.boneAnchor);
                    break;

                case PoseOverriderDto poseOverriderDto:
                    bytable = UMI3DSerializer.Write(poseOverriderDto.poseIndexinPoseManager)
                        + UMI3DSerializer.Write(poseOverriderDto.duration)
                        + UMI3DSerializer.Write(poseOverriderDto.interpolationable)
                        + UMI3DSerializer.Write(poseOverriderDto.isHoverEnter)
                        + UMI3DSerializer.Write(poseOverriderDto.isHoverExit)
                        + UMI3DSerializer.Write(poseOverriderDto.isRelease)
                        + UMI3DSerializer.Write(poseOverriderDto.isTrigger)
                        + UMI3DSerializer.Write(poseOverriderDto.composable)
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