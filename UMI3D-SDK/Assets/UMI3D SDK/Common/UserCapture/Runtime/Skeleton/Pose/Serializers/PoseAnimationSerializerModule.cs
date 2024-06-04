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
using System.ComponentModel;
using umi3d.common.userCapture.description;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Serializer for <see cref="PoseDto"/>, <see cref="PoseAnchorDto"/> and <see cref="PoseAnimatorDto"/>.
    /// </summary>
    public class PoseAnimationSerializerModule : UMI3DSerializerModule
    {
        UMI3DVersion.VersionCompatibility PlayPoseClipDtoOld = new UMI3DVersion.VersionCompatibility("2.6", "2.9.b.240529");

        /// <inheritdoc/>
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(PoseClipDto) => true,
                true when typeof(T) == typeof(PoseAnimatorDto) => true,
                true when typeof(T) == typeof(DurationDto) => true,
                true when typeof(T) == typeof(CheckPoseAnimatorConditionsRequestDto) => true,
                true when typeof(T) == typeof(PlayPoseClipDto) => true,

                _ => null
            };
        }

        /// <inheritdoc/>
        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(PoseClipDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out ulong id);
                        readable &= UMI3DSerializer.TryRead(container, out PoseDto pose);
                        readable &= UMI3DSerializer.TryRead(container, out bool isInterpolable);
                        readable &= UMI3DSerializer.TryRead(container, out bool isComposable);

                        if (readable)
                        {
                            PoseClipDto poseDto = new()
                            {
                                id = id,
                                pose = pose,
                                isInterpolable = isInterpolable,
                                isComposable = isComposable
                            };

                            result = (T)Convert.ChangeType(poseDto, typeof(PoseClipDto));
                            return true;
                        }
                        break;
                    }
                case true when typeof(T) == typeof(PoseAnimatorDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out ulong id);
                        readable &= UMI3DSerializer.TryRead(container, out ulong poseId);
                        readable &= UMI3DSerializer.TryRead(container, out bool isAnchored);
                        readable &= UMI3DSerializer.TryRead(container, out ulong boneConstraintId);
                        readable &= UMI3DSerializer.TryRead(container, out ulong relativeNodeId);
                        readable &= UMI3DSerializer.TryRead(container, out DurationDto durationDto);
                        readable &= UMI3DSerializer.TryRead(container, out ushort activationMode);

                        AbstractPoseConditionDto[] poseConditionDtos = UMI3DSerializer.ReadArray<AbstractPoseConditionDto>(container);

                        if (readable)
                        {
                            PoseAnimatorDto poseOverriderDto = new()
                            {
                                id = id,
                                poseClipId = poseId,
                                isAnchored = isAnchored,
                                boneConstraintId = boneConstraintId,
                                relatedNodeId = relativeNodeId,
                                poseConditions = poseConditionDtos,
                                duration = durationDto,
                                activationMode = activationMode,
                            };

                            result = (T)Convert.ChangeType(poseOverriderDto, typeof(PoseAnimatorDto));
                            return true;
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

                case true when typeof(T) == typeof(CheckPoseAnimatorConditionsRequestDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out ulong poseOverriderId);
                        readable &= UMI3DSerializer.TryRead(container, out bool shouldActivate);

                        if (readable)
                        {
                            CheckPoseAnimatorConditionsRequestDto activatePoseOverriderDto = new()
                            {
                                PoseAnimatorId = poseOverriderId,
                                ShouldActivate = shouldActivate
                            };

                            result = (T)Convert.ChangeType(activatePoseOverriderDto, typeof(CheckPoseAnimatorConditionsRequestDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(PlayPoseClipDto):
                    {
                        if (PlayPoseClipDtoOld.IsCompatible(container.version))
                        {
                            readable = UMI3DSerializer.TryRead(container, out ulong poseId);
                            readable &= UMI3DSerializer.TryRead(container, out bool stopPose);

                            if (readable)
                            {
                                PlayPoseClipDto activatePoseOverriderDto = new()
                                {
                                    poseId = poseId,
                                    stopPose = stopPose,
                                    transitionDuration = 0
                                };

                                result = (T)Convert.ChangeType(activatePoseOverriderDto, typeof(PlayPoseClipDto));
                                return true;
                            }
                        }
                        else
                        {
                            readable = UMI3DSerializer.TryRead(container, out ulong poseId);
                            readable &= UMI3DSerializer.TryRead(container, out bool stopPose);
                            readable &= UMI3DSerializer.TryRead(container, out float transitionDuration);

                            if (readable)
                            {
                                PlayPoseClipDto activatePoseOverriderDto = new()
                                {
                                    poseId = poseId,
                                    stopPose = stopPose,
                                    transitionDuration = transitionDuration
                                };

                                result = (T)Convert.ChangeType(activatePoseOverriderDto, typeof(PlayPoseClipDto));
                                return true;
                            }
                        }
                        break;
                    }


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
                case PoseClipDto poseDto:
                    bytable = UMI3DSerializer.Write(poseDto.id)
                        + UMI3DSerializer.Write(poseDto.pose)
                        + UMI3DSerializer.Write(poseDto.isInterpolable)
                        + UMI3DSerializer.Write(poseDto.isComposable);
                    break;

                case PoseAnimatorDto poseOverriderDto:
                    bytable = UMI3DSerializer.Write(poseOverriderDto.id)
                        + UMI3DSerializer.Write(poseOverriderDto.poseClipId)
                        + UMI3DSerializer.Write(poseOverriderDto.isAnchored)
                        + UMI3DSerializer.Write(poseOverriderDto.boneConstraintId)
                        + UMI3DSerializer.Write(poseOverriderDto.relatedNodeId)
                        + UMI3DSerializer.Write(poseOverriderDto.duration)
                        + UMI3DSerializer.Write(poseOverriderDto.activationMode)
                        + UMI3DSerializer.WriteCollection(poseOverriderDto.poseConditions);
                    break;

                case DurationDto durationDto:
                    bytable = UMI3DSerializer.Write(durationDto.duration)
                        + UMI3DSerializer.Write(durationDto.min)
                        + UMI3DSerializer.Write(durationDto.max);
                    break;

                case CheckPoseAnimatorConditionsRequestDto activatePoseOverriderDto:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.CheckPoseAnimatorConditionsRequest)
                        + UMI3DSerializer.Write(activatePoseOverriderDto.PoseAnimatorId)
                        + UMI3DSerializer.Write(activatePoseOverriderDto.ShouldActivate);
                    break;

                case PlayPoseClipDto playPoseAnimationDto:
                    if (PlayPoseClipDtoOld.IsCompatible(UMI3DSerializer.version))
                    {
                        bytable = UMI3DSerializer.Write(UMI3DOperationKeys.PlayPoseRequest)
                        + UMI3DSerializer.Write(playPoseAnimationDto.poseId)
                        + UMI3DSerializer.Write(playPoseAnimationDto.stopPose);
                        break;
                    }

                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.PlayPoseRequest)
                    + UMI3DSerializer.Write(playPoseAnimationDto.poseId)
                    + UMI3DSerializer.Write(playPoseAnimationDto.stopPose)
                    + UMI3DSerializer.Write(playPoseAnimationDto.transitionDuration);

                    break;

                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}