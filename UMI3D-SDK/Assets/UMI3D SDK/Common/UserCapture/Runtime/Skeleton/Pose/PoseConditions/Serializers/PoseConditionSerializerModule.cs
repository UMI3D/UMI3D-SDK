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
    /// <summary>
    /// Serializer for <see cref="AbstractPoseConditionDto"/>;
    /// </summary>
    public class PoseConditionSerializerModule : UMI3DSerializerModule
    {
        public enum PoseConditionSerializingIndex : int
        {
            MAGNITUDE_CONDITION,
            RANGE_CONDITION,
            BONE_ROTATION_CONDITION,
            DIRECTION_CONDITION,
            NOT_CONDITION,
            USER_SCALE_CONDITION,
            SCALE_CONDITION
        }

        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(MagnitudeConditionDto) => true,
                true when typeof(T) == typeof(RangeConditionDto) => true,
                true when typeof(T) == typeof(BoneRotationConditionDto) => true,
                true when typeof(T) == typeof(DirectionConditionDto) => true,
                true when typeof(T) == typeof(NotConditionDto) => true,
                true when typeof(T) == typeof(UserScaleConditionDto) => true,
                true when typeof(T) == typeof(ScaleConditionDto) => true,
                true when typeof(T) == typeof(AbstractPoseConditionDto) => true,

                _ => null
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(AbstractPoseConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out int index);

                        if (readable)
                        {
                            AbstractPoseConditionDto poseConditionDto = null;

                            switch (index)
                            {
                                case (int)PoseConditionSerializingIndex.MAGNITUDE_CONDITION:
                                    ReadConditionDTO(container, out readable, out MagnitudeConditionDto magnitudeConditionDto);
                                    poseConditionDto = magnitudeConditionDto;
                                    break;

                                case (int)PoseConditionSerializingIndex.BONE_ROTATION_CONDITION:
                                    ReadConditionDTO(container, out readable, out BoneRotationConditionDto boneRotationConditionDto);
                                    poseConditionDto = boneRotationConditionDto;
                                    break;

                                case (int)PoseConditionSerializingIndex.DIRECTION_CONDITION:
                                    ReadConditionDTO(container, out readable, out DirectionConditionDto directionConditionDto);
                                    poseConditionDto = directionConditionDto;
                                    break;

                                case (int)PoseConditionSerializingIndex.USER_SCALE_CONDITION:
                                    ReadConditionDTO(container, out readable, out UserScaleConditionDto userScaleConditinoDto);
                                    poseConditionDto = userScaleConditinoDto;
                                    break;

                                case (int)PoseConditionSerializingIndex.SCALE_CONDITION:
                                    ReadConditionDTO(container, out readable, out ScaleConditionDto scaleConditionDto);
                                    poseConditionDto = scaleConditionDto;
                                    break;

                                case (int)PoseConditionSerializingIndex.RANGE_CONDITION:
                                    ReadConditionDTO(container, out readable, out RangeConditionDto rangeConditionDto);
                                    poseConditionDto = rangeConditionDto;
                                    break;

                                case (int)PoseConditionSerializingIndex.NOT_CONDITION:
                                    ReadConditionDTO(container, out readable, out NotConditionDto notConditionDto);
                                    poseConditionDto = notConditionDto;
                                    break;
                            }

                            if (poseConditionDto != null)
                            {
                                result = (T)(object)poseConditionDto;
                                return true;
                            }
                        }

                        result = default;
                        return false;
                    }

                default:
                    result = default;
                    readable = false;
                    return false;
            }
        }

        private bool ReadConditionDTO<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(MagnitudeConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out float magnitude);
                        readable &= UMI3DSerializer.TryRead(container, out uint boneOrigin);
                        readable &= UMI3DSerializer.TryRead(container, out ulong targetObjectId);

                        if (readable)
                        {
                            var magnitudeConditionDto = new MagnitudeConditionDto()
                            {
                                TargetNodeId = targetObjectId,
                                BoneOrigin = boneOrigin,
                                Magnitude = magnitude
                            };
                            result = (T)Convert.ChangeType(magnitudeConditionDto, typeof(T));
                            return true;
                        }

                        result = default;
                        return false;
                    }

                case true when typeof(T) == typeof(BoneRotationConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out uint boneId);
                        readable &= UMI3DSerializer.TryRead(container, out Vector4Dto rotation);
                        readable &= UMI3DSerializer.TryRead(container, out float acceptanceRange);

                        if (readable)
                        {
                            var boneRotationConditionDto = new BoneRotationConditionDto()
                            {
                                BoneId = boneId,
                                Rotation = rotation,
                                Threshold = acceptanceRange
                            };
                            result = (T)Convert.ChangeType(boneRotationConditionDto, typeof(T));
                            return true;
                        }

                        result = default;
                        return false;
                    }

                case true when typeof(T) == typeof(DirectionConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out Vector3Dto direction);

                        if (readable)
                        {
                            var directionConditionDto = new DirectionConditionDto()
                            {
                                Direction = direction
                            };
                            result = (T)Convert.ChangeType(directionConditionDto, typeof(T));
                            return true;
                        }

                        result = default;
                        return false;
                    }

                case true when typeof(T) == typeof(UserScaleConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out Vector3Dto scale);

                        if (readable)
                        {
                            var userScaleConditionDto = new UserScaleConditionDto()
                            {
                                Scale = scale
                            };
                            result = (T)Convert.ChangeType(userScaleConditionDto, typeof(T));
                            return true;
                        }

                        result = default;
                        return false;
                    }

                case true when typeof(T) == typeof(ScaleConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out Vector3Dto scale);
                        readable &= UMI3DSerializer.TryRead(container, out ulong targetNodeId);

                        if (readable)
                        {
                            var scaleCondition = new ScaleConditionDto()
                            {
                                Scale = scale,
                                TargetId = targetNodeId
                            };
                            result = (T)Convert.ChangeType(scaleCondition, typeof(T));
                            return true;
                        }

                        result = default;
                        return false;
                    }

                case true when typeof(T) == typeof(RangeConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out AbstractPoseConditionDto conditionA);
                        readable &= UMI3DSerializer.TryRead(container, out AbstractPoseConditionDto conditionB);

                        if (readable)
                        {
                            var rangeConditionDto = new RangeConditionDto()
                            {
                                ConditionA = conditionA,
                                ConditionB = conditionB
                            };
                            result = (T)Convert.ChangeType(rangeConditionDto, typeof(T));
                            return true;
                        }

                        result = default;
                        return false;
                    }

                case true when typeof(T) == typeof(NotConditionDto):
                    {
                        AbstractPoseConditionDto[] conditions;
                        conditions = UMI3DSerializer.ReadArray<AbstractPoseConditionDto>(container);

                        if (conditions != null)
                        {
                            var notConditionDto = new NotConditionDto()
                            {
                                Conditions = conditions
                            };
                            result = (T)Convert.ChangeType(notConditionDto, typeof(T));
                            readable = true;
                            return true;
                        }

                        result = default;
                        readable = false;
                        return false;
                    }
            }
            result = default;
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case MagnitudeConditionDto magnitudeConditionDto:
                    bytable = UMI3DSerializer.Write((int)PoseConditionSerializingIndex.MAGNITUDE_CONDITION)
                        + UMI3DSerializer.Write(magnitudeConditionDto.Magnitude)
                        + UMI3DSerializer.Write(magnitudeConditionDto.BoneOrigin)
                        + UMI3DSerializer.Write(magnitudeConditionDto.TargetNodeId);
                    break;

                case RangeConditionDto rangeConditionDto:
                    bytable = UMI3DSerializer.Write((int)PoseConditionSerializingIndex.RANGE_CONDITION)
                        + UMI3DSerializer.Write(rangeConditionDto.ConditionA)
                        + UMI3DSerializer.Write(rangeConditionDto.ConditionB);
                    break;

                case BoneRotationConditionDto boneRotationConditionDto:
                    bytable = UMI3DSerializer.Write((int)PoseConditionSerializingIndex.BONE_ROTATION_CONDITION)
                        + UMI3DSerializer.Write(boneRotationConditionDto.BoneId)
                        + UMI3DSerializer.Write(boneRotationConditionDto.Rotation)
                        + UMI3DSerializer.Write(boneRotationConditionDto.Threshold);
                    break;

                case DirectionConditionDto directionConditionDto:
                    bytable = UMI3DSerializer.Write((int)PoseConditionSerializingIndex.DIRECTION_CONDITION)
                        + UMI3DSerializer.Write(directionConditionDto.Direction);
                    break;

                case NotConditionDto notConditionDto:
                    bytable = UMI3DSerializer.Write((int)PoseConditionSerializingIndex.NOT_CONDITION)
                        + UMI3DSerializer.WriteCollection(notConditionDto.Conditions);
                    break;

                case UserScaleConditionDto userScaleConditionDto:
                    bytable = UMI3DSerializer.Write((int)PoseConditionSerializingIndex.USER_SCALE_CONDITION)
                        + UMI3DSerializer.Write(userScaleConditionDto.Scale);
                    break;

                case ScaleConditionDto scaleConditionDto:
                    bytable = UMI3DSerializer.Write((int)PoseConditionSerializingIndex.SCALE_CONDITION)
                        + UMI3DSerializer.Write(scaleConditionDto.Scale)
                        + UMI3DSerializer.Write(scaleConditionDto.TargetId);
                    break;

                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}