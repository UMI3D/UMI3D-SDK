﻿/*
Copyright 2019 - 2021 Inetum

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
using System.Collections.Generic;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Helper module to serialize objects related to the Collaboration module.
    /// </summary>
    public class UMI3DCollaborationSerializerModule : UMI3DSerializerModule
    {
        /// <inheritdoc/>
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserCameraPropertiesDto):
                    readable = container.length >= (17 * sizeof(float)) + sizeof(uint);
                    if (readable)
                    {
                        var usercam = new UserCameraPropertiesDto
                        {
                            scale = UMI3DSerializer.Read<float>(container),
                            projectionMatrix = UMI3DSerializer.Read<SerializableMatrix4x4>(container),
                            boneType = UMI3DSerializer.Read<uint>(container)
                        };
                        result = (T)Convert.ChangeType(usercam, typeof(T));
                    }
                    else
                    {
                        result = default(T);
                    }

                    return true;

                case true when typeof(T) == typeof(VoiceDto):
                    if (UMI3DSerializer.TryRead(container, out string voiceUrl) &&
                        UMI3DSerializer.TryRead(container, out string login) &&
                        UMI3DSerializer.TryRead(container, out string password) &&
                        UMI3DSerializer.TryRead(container, out string channel))
                    {
                        readable = true;
                        var voice = new VoiceDto
                        {
                            url = voiceUrl,
                            login = login,
                            password = password,
                            channelName = channel
                        };
                        result = (T)Convert.ChangeType(voice, typeof(T));
                    }
                    else
                    {
                        readable = false;
                        result = default(T);
                    }

                    return true;

                case true when typeof(T) == typeof(ControllerDto):
                    uint type;
                    SerializableVector4 rot;
                    SerializableVector3 pos;
                    bool isOverrider;
                    if (UMI3DSerializer.TryRead(container, out type)
                        && UMI3DSerializer.TryRead(container, out rot)
                        && UMI3DSerializer.TryRead(container, out pos)
                        && UMI3DSerializer.TryRead(container, out isOverrider))
                    {
                        var controller = new ControllerDto() { boneType = type, rotation = rot, position = pos, isOverrider = isOverrider };
                        result = (T)Convert.ChangeType(controller, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;

                case true when typeof(T) == typeof(BoneDto):
                    //uint type;
                    //SerializableVector4 rot;
                    if (UMI3DSerializer.TryRead(container, out type)
                        && UMI3DSerializer.TryRead(container, out rot))
                    {
                        var bone = new BoneDto() { boneType = type, rotation = rot };
                        result = (T)Convert.ChangeType(bone, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;
                case true when typeof(T) == typeof(DurationDto):
                    {
                        ulong duration;
                        ulong min;
                        ulong max;

                        readable = UMI3DSerializer.TryRead(container, out duration);
                        readable &= UMI3DSerializer.TryRead(container, out min);
                        readable &= UMI3DSerializer.TryRead(container, out max);

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

                        result = default(T);
                        return false;
                    }
                #region Pose
                case true when typeof(T) == typeof(PoseDto):
                    {
                        int index_id;
                        List<BoneDto> bones;
                        BonePoseDto boneAnchor;

                        readable = UMI3DSerializer.TryRead(container, out index_id);
                        bones = UMI3DSerializer.ReadList<BoneDto>(container);

                        UMI3DSerializer.TryRead<BonePoseDto>(container, out boneAnchor);

                        if (readable)
                        {
                            PoseDto poseDto = new PoseDto(
                                bones: bones,
                                boneAnchor: boneAnchor
                            );
                            poseDto.id = index_id;

                            result = (T)Convert.ChangeType(poseDto, typeof(PoseDto));
                            return true;
                        }

                        result = default(T);
                        return false;
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
                        readable &= UMI3DSerializer.TryRead(container, out composable);
                        poseConditionDtos = UMI3DSerializer.ReadArray<PoseConditionDto>(container);

                        if (readable)
                        {
                            PoseOverriderDto poseOverriderDto = new PoseOverriderDto(
                                poseIndexinPoseManager: poseIndex,
                                poseConditionDtos: poseConditionDtos,
                                duration: durationDto,
                                interpolationable: interpolationable,
                                composable: composable
                            );

                            result = (T)Convert.ChangeType(poseOverriderDto, typeof(PoseOverriderDto));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }
                #endregion
                #region Bone Pose
                case true when typeof(T) == typeof(BonePoseDto):
                    {
                        int index;
                        readable = UMI3DSerializer.TryRead(container, out index);

                        if (readable)
                        {
                            BonePoseDto bonePoseDto = null;
                            AnchoredBonePoseDto anchorBonePoseDto;
                            NodeAnchoredBonePoseDto nodePositionAnchoredBonePoseDto;
                            FloorAnchoredBonePoseDto floorAnchoredBonePoseDto;

                            switch (index)
                            {
                                case 0:
                                    ReadPoseDto(container, out readable, out bonePoseDto);
                                    break;
                                case 1:
                                    ReadPoseDto(container, out readable, out anchorBonePoseDto);
                                    bonePoseDto = anchorBonePoseDto;
                                    break;
                                case 2:
                                    ReadPoseDto(container, out readable, out nodePositionAnchoredBonePoseDto);
                                    bonePoseDto = nodePositionAnchoredBonePoseDto;
                                    break;
                                case 3:
                                    ReadPoseDto(container, out readable, out floorAnchoredBonePoseDto);
                                    bonePoseDto = floorAnchoredBonePoseDto;
                                    break;
                            }

                            if (bonePoseDto != null)
                            {
                                result = (T)(object)bonePoseDto;
                                return true;
                            }
                        }

                        result = default(T);
                        return false;
                    }


                #endregion
                #region Pose Condition

                case true when typeof(T) == typeof(PoseConditionDto):
                    {
                        int index;
                        readable = UMI3DSerializer.TryRead(container, out index);

                        if (readable)
                        {
                            PoseConditionDto poseConditionDto = null;

                            MagnitudeConditionDto magnitudeConditionDto;
                            BoneRotationConditionDto boneRotationConditionDto;
                            DirectionConditionDto directionConditionDto;
                            UserScaleConditionDto userScaleConditinoDto;
                            ScaleConditionDto scaleConditionDto;

                            RangeConditionDto rangeConditionDto;
                            NotConditionDto notConditionDto;

                            switch (index)
                            {
                                case 1:
                                    ReadConditionDTO(container, out readable, out magnitudeConditionDto);
                                    poseConditionDto = magnitudeConditionDto;
                                    break;
                                case 2:
                                    ReadConditionDTO(container, out readable, out rangeConditionDto);
                                    poseConditionDto = rangeConditionDto;
                                    break;
                                case 3:
                                    ReadConditionDTO(container, out readable, out boneRotationConditionDto);
                                    poseConditionDto = boneRotationConditionDto;
                                    break;
                                case 4:
                                    ReadConditionDTO(container, out readable, out directionConditionDto);
                                    poseConditionDto = directionConditionDto;
                                    break;
                                case 5:
                                    ReadConditionDTO(container, out readable, out notConditionDto);
                                    poseConditionDto = notConditionDto;
                                    break;
                                case 6:
                                    ReadConditionDTO(container, out readable, out userScaleConditinoDto);
                                    poseConditionDto = userScaleConditinoDto;
                                    break;
                                case 7:
                                    ReadConditionDTO(container, out readable, out scaleConditionDto);
                                    poseConditionDto = scaleConditionDto;
                                    break;
                            }

                            if (poseConditionDto != null)
                            {
                                result = (T)(object)poseConditionDto;
                                return true;
                            }
                        }

                        result = default(T);
                        return false;
                    }
                #endregion

                case true when typeof(T) == typeof(UMI3DHandPoseDto):
                    ulong id;
                    string Name;
                    bool IsActive;
                    bool HoverPose;
                    bool isRelativeToNode;
                    SerializableVector3 RightHandPosition;
                    SerializableVector3 RightHandEulerRotation;
                    SerializableVector3 LeftHandPosition;
                    SerializableVector3 LeftHandEulerRotation;
                    if (UMI3DSerializer.TryRead(container, out id)
                        && UMI3DSerializer.TryRead(container, out Name)
                        && UMI3DSerializer.TryRead(container, out IsActive)
                        && UMI3DSerializer.TryRead(container, out HoverPose)
                        && UMI3DSerializer.TryRead(container, out isRelativeToNode)
                        && UMI3DSerializer.TryRead(container, out RightHandPosition)
                        && UMI3DSerializer.TryRead(container, out RightHandEulerRotation)
                        && UMI3DSerializer.TryRead(container, out LeftHandPosition)
                        && UMI3DSerializer.TryRead(container, out LeftHandEulerRotation))
                    {
                        var HandPose = new UMI3DHandPoseDto()
                        {
                            id = id,
                            Name = Name,
                            IsActive = IsActive,
                            HoverPose = HoverPose,
                            isRelativeToNode = isRelativeToNode,
                            RightHandPosition = RightHandPosition,
                            RightHandEulerRotation = RightHandEulerRotation,
                            LeftHandPosition = LeftHandPosition,
                            LeftHandEulerRotation = LeftHandEulerRotation
                        };
                        result = (T)Convert.ChangeType(HandPose, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;
                case true when typeof(T) == typeof(UMI3DEmotesConfigDto):
                    var conf = new UMI3DEmotesConfigDto();
                    result = default(T);
                    readable = UMI3DSerializer.TryRead(container, out conf.id);
                    readable &= UMI3DSerializer.TryRead(container, out conf.allAvailableByDefault);

                    if (readable)
                    {
                        readable = UMI3DSerializer.TryRead(container, out int nbEmotes);
                        if (readable)
                        {
                            for (uint i = 0; i < nbEmotes; i++)
                            {
                                Read(container, out readable, out UMI3DEmoteDto emote);
                                if (!readable)
                                    break;
                                else
                                    conf.emotes.Add(emote);
                            }
                            result = (T)Convert.ChangeType(conf, typeof(T));
                        }
                    }
                    return true;

                case true when typeof(T) == typeof(UMI3DEmoteDto):
                    var e = new UMI3DEmoteDto();
                    result = default(T);

                    readable = UMI3DSerializer.TryRead(container, out e.id);
                    readable &= UMI3DSerializer.TryRead(container, out e.label);
                    readable &= UMI3DSerializer.TryRead(container, out e.animationId);
                    readable &= UMI3DSerializer.TryRead(container, out e.available);
                    readable &= UMI3DSerializer.TryRead(container, out e.iconResource);

                    if (!readable)
                        return false;
                    result = (T)Convert.ChangeType(e, typeof(T));
                    return true;
                case true when typeof(T) == typeof(UMI3DRenderedNodeDto.MaterialOverrideDto):
                    var mat = new UMI3DRenderedNodeDto.MaterialOverrideDto();
                    readable = UMI3DSerializer.TryRead<ulong>(container, out mat.newMaterialId);
                    if (readable)
                    {
                        readable = UMI3DSerializer.TryRead<bool>(container, out mat.addMaterialIfNotExists);
                        if (readable)
                        {
                            mat.overridedMaterialsId = UMI3DSerializer.ReadList<string>(container);
                            result = (T)Convert.ChangeType(mat, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                        }
                    }
                    else
                    {
                        result = default(T);
                    }

                    return true;
                case true when typeof(T) == typeof(ScalableTextureDto):
                    {
                        var scalableTexture = new ScalableTextureDto
                        {
                            variants = UMI3DSerializer.ReadList<FileDto>(container)
                        };
                        if (UMI3DSerializer.TryRead(container, out string animationId)
                            && UMI3DSerializer.TryRead(container, out string audioSourceId)
                            && UMI3DSerializer.TryRead(container, out string streamingFromUserId)
                            && UMI3DSerializer.TryRead(container, out float scale))
                        {
                            scalableTexture.animationId = animationId;
                            scalableTexture.audioSourceId = audioSourceId;
                            scalableTexture.streamingFromUserId = streamingFromUserId;
                            scalableTexture.scale = scale;
                            readable = true;
                            result = (T)Convert.ChangeType(scalableTexture, typeof(T));
                            return true;
                        }
                        readable = false;
                        result = default(T);
                        return true;
                    }
                case true when typeof(T) == typeof(TextureDto):
                    {
                        var texture = new TextureDto
                        {
                            variants = UMI3DSerializer.ReadList<FileDto>(container)
                        };
                        if (texture.variants.Count == 0)
                        {
                            result = default(T);
                            readable = true;
                            return true;
                        }
                        else if (UMI3DSerializer.TryRead(container, out string animationId)
                          && UMI3DSerializer.TryRead(container, out string audioSourceId)
                          && UMI3DSerializer.TryRead(container, out string streamingFromUserId))
                        {
                            texture.animationId = animationId;
                            texture.audioSourceId = audioSourceId;
                            texture.streamingFromUserId = streamingFromUserId;
                            readable = true;
                            result = (T)Convert.ChangeType(texture, typeof(T));
                            return true;
                        }
                        readable = false;
                        result = default(T);
                        return true;
                    }
                case true when typeof(T) == typeof(ResourceDto):
                    var resource = new ResourceDto
                    {
                        variants = UMI3DSerializer.ReadList<FileDto>(container)
                    };
                    if (resource.variants.Count == 0)
                    {
                        result = default(T);
                        readable = true;
                        return true;
                    }
                    readable = true;
                    result = (T)Convert.ChangeType(resource, typeof(T));
                    return true;
                case true when typeof(T) == typeof(FileDto):
                    var file = new FileDto
                    {
                        metrics = new AssetMetricDto()
                    };
                    readable = UMI3DSerializer.TryRead<string>(container, out file.url)
                               && UMI3DSerializer.TryRead<string>(container, out file.format)
                               && UMI3DSerializer.TryRead<string>(container, out file.extension)
                               && UMI3DSerializer.TryRead<int>(container, out file.metrics.resolution)
                               && UMI3DSerializer.TryRead<float>(container, out file.metrics.size)
                               && UMI3DSerializer.TryRead<string>(container, out file.pathIfInBundle)
                               && UMI3DSerializer.TryRead<string>(container, out file.libraryKey);
                    if (readable)
                    {
                        result = (T)Convert.ChangeType(file, typeof(T));
                    }
                    else
                    {
                        result = default(T);
                    }

                    return true;
                case true when typeof(T) == typeof(RedirectionDto):
                    MediaDto media;
                    GateDto gate;
                    if (
                        UMI3DSerializer.TryRead<MediaDto>(container, out media)
                        && UMI3DSerializer.TryRead<GateDto>(container, out gate)
                        )
                    {

                        var redirection = new RedirectionDto
                        {
                            media = media,
                            gate = gate
                        };
                        readable = true;
                        result = (T)Convert.ChangeType(redirection, typeof(T));

                        return true;
                    }
                    result = default(T);
                    readable = false;
                    return false;
                case true when typeof(T) == typeof(ForceLogoutDto):
                    string reason;
                    if (
                        UMI3DSerializer.TryRead<string>(container, out reason)
                        )
                    {

                        var forceLogoutDto = new ForceLogoutDto
                        {
                            reason = reason
                        };
                        readable = true;
                        result = (T)Convert.ChangeType(forceLogoutDto, typeof(T));

                        return true;
                    }
                    result = default(T);
                    readable = false;
                    return false;
                case true when typeof(T) == typeof(GateDto):
                    string gateid;
                    byte[] data;
                    if (
                        UMI3DSerializer.TryRead<string>(container, out gateid)
                        )
                    {
                        data = UMI3DSerializer.ReadArray<byte>(container);

                        var _gate = new GateDto
                        {
                            gateId = gateid,
                            metaData = data
                        };
                        readable = true;
                        result = (T)Convert.ChangeType(_gate, typeof(T));

                        return true;
                    }
                    result = default(T);
                    readable = false;
                    return false;
                case true when typeof(T) == typeof(MediaDto):
                    string name, versionMajor, versionMinor, versionStatus, versionDate, url;
                    ResourceDto icon2D, icon3D;
                    if (
                        UMI3DSerializer.TryRead(container, out name)
                        && UMI3DSerializer.TryRead(container, out icon2D)
                        && UMI3DSerializer.TryRead(container, out icon3D)
                        && UMI3DSerializer.TryRead(container, out url)
                        )
                    {
                        var _media = new MediaDto
                        {
                            name = name,
                            icon2D = icon2D,
                            icon3D = icon3D,
                            url = url
                        };
                        readable = true;
                        result = (T)Convert.ChangeType(_media, typeof(T));

                        return true;
                    }
                    result = default(T);
                    readable = false;
                    return false;
                case true when typeof(T) == typeof(UserTrackingFrameDto):
                    {
                        uint idKey = 0;
                        ulong userId, parentId;
                        //float skeletonHighOffset, refreshFrequency;
                        SerializableVector3 position;
                        SerializableVector4 rotation;

                        if (
                            UMI3DSerializer.TryRead(container, out idKey)
                            && UMI3DSerializer.TryRead(container, out userId)
                            && UMI3DSerializer.TryRead(container, out parentId)
                            //&& UMI3DSerializer.TryRead(container, out skeletonHighOffset)
                            && UMI3DSerializer.TryRead(container, out position)
                            && UMI3DSerializer.TryRead(container, out rotation)
                            //&& UMI3DSerializer.TryRead(container, out refreshFrequency)
                            )
                        {
                            System.Collections.Generic.List<ControllerDto> bones = UMI3DSerializer.ReadList<ControllerDto>(container);

                            if (bones != default)
                            {
                                var trackingFrame = new UserTrackingFrameDto
                                {
                                    userId = userId,
                                    parentId = parentId,
                                    //skeletonHighOffset = skeletonHighOffset,
                                    position = position,
                                    rotation = rotation,
                                    //refreshFrequency = refreshFrequency,
                                    trackedBones = bones
                                };
                                readable = true;
                                result = (T)Convert.ChangeType(trackingFrame, typeof(T));

                                return true;
                            }
                        }
                        result = default(T);
                        readable = false;
                        return false;
                    }
                case true when typeof(T) == typeof(UserTrackingBoneDto):
                    {
                        uint idKey = 0;
                        ulong userId;
                        ControllerDto boneDto;

                        if (
                            UMI3DSerializer.TryRead(container, out idKey)
                            && UMI3DSerializer.TryRead(container, out userId)
                            && UMI3DSerializer.TryRead(container, out boneDto)
                            )
                        {
                            var trackingBone = new UserTrackingBoneDto
                            {
                                userId = userId,
                                bone = boneDto
                            };
                            readable = true;
                            result = (T)Convert.ChangeType(trackingBone, typeof(T));

                            return true;

                        }
                        result = default(T);
                        readable = false;
                        return false;
                    }

                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }
        private bool ReadPoseDto<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(FloorAnchoredBonePoseDto):
                    {
                        BonePoseDto bonePoseDto;
                        ReadPoseDto(container, out readable, out bonePoseDto);

                        if (readable)
                        {
                            FloorAnchoredBonePoseDto nodePositionAnchoredBonePoseDto = new FloorAnchoredBonePoseDto(
                                bonePoseDto: bonePoseDto
                            );

                            result = (T)Convert.ChangeType(nodePositionAnchoredBonePoseDto, typeof(FloorAnchoredBonePoseDto));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(NodeAnchoredBonePoseDto):
                    {
                        BonePoseDto bonePoseDto;
                        ReadPoseDto(container, out readable, out bonePoseDto);

                        uint node;
                        readable &= UMI3DSerializer.TryRead(container, out node);

                        if (readable)
                        {
                            NodeAnchoredBonePoseDto nodeAnchoredBonePoseDto = new NodeAnchoredBonePoseDto(
                                bonePoseDto: bonePoseDto,
                                node: node
                            );

                            result = (T)Convert.ChangeType(nodeAnchoredBonePoseDto, typeof(NodeAnchoredBonePoseDto));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(AnchoredBonePoseDto):
                    {
                        BonePoseDto bonePoseDto;
                        ReadPoseDto(container, out readable, out bonePoseDto);

                        uint otherBone;
                        readable &= UMI3DSerializer.TryRead(container, out otherBone);

                        if (readable)
                        {
                            AnchoredBonePoseDto anchorBonePoseDto = new AnchoredBonePoseDto(
                                bonePoseDto: bonePoseDto,
                                otherBone: otherBone
                            );

                            result = (T)Convert.ChangeType(anchorBonePoseDto, typeof(AnchoredBonePoseDto));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(BonePoseDto):
                    {
                        uint bone;
                        SerializableVector3 position;
                        SerializableVector4 rotation;

                        readable = UMI3DSerializer.TryRead(container, out bone);
                        readable &= UMI3DSerializer.TryRead(container, out position);
                        readable &= UMI3DSerializer.TryRead(container, out rotation);

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

                        result = default(T);
                        return false;
                    }

                default:
                    result = default(T);
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
                        float magnitude;
                        uint boneOrigin;
                        uint targetObjectId;
                        readable = UMI3DSerializer.TryRead(container, out magnitude);
                        readable &= UMI3DSerializer.TryRead(container, out boneOrigin);
                        readable &= UMI3DSerializer.TryRead(container, out targetObjectId);

                        if (readable)
                        {
                            MagnitudeConditionDto magnitudeConditionDto = new MagnitudeConditionDto(
                                targetObjectId: targetObjectId,
                                boneOrigine: boneOrigin,
                                magnitude: magnitude
                            );
                            result = (T)Convert.ChangeType(magnitudeConditionDto, typeof(T));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(BoneRotationConditionDto):
                    {
                        uint boneId;
                        SerializableVector4 rotation;
                        readable = UMI3DSerializer.TryRead(container, out boneId);
                        readable &= UMI3DSerializer.TryRead(container, out rotation);

                        if (readable)
                        {
                            BoneRotationConditionDto boneRotationConditionDto = new BoneRotationConditionDto(
                                boneId: boneId,
                                rotation: rotation
                            );
                            result = (T)Convert.ChangeType(boneRotationConditionDto, typeof(T));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(DirectionConditionDto):
                    {
                        SerializableVector3 direction;
                        readable = UMI3DSerializer.TryRead(container, out direction);

                        if (readable)
                        {
                            DirectionConditionDto directionConditionDto = new DirectionConditionDto(
                                direction: direction
                            );
                            result = (T)Convert.ChangeType(directionConditionDto, typeof(T));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(UserScaleConditionDto):
                    {
                        SerializableVector3 scale;
                        readable = UMI3DSerializer.TryRead(container, out scale);

                        if (readable)
                        {
                            UserScaleConditionDto userScaleConditinoDto = new UserScaleConditionDto(
                                scale: scale
                            );
                            result = (T)Convert.ChangeType(userScaleConditinoDto, typeof(T));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(ScaleConditionDto):
                    {
                        SerializableVector3 scale;
                        readable = UMI3DSerializer.TryRead(container, out scale);

                        if (readable)
                        {
                            ScaleConditionDto scaleCondition = new ScaleConditionDto(
                                scale: scale
                            );
                            result = (T)Convert.ChangeType(scaleCondition, typeof(T));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(RangeConditionDto):
                    {
                        PoseConditionDto conditionA;
                        PoseConditionDto conditionB;
                        readable = UMI3DSerializer.TryRead(container, out conditionA);
                        readable &= UMI3DSerializer.TryRead(container, out conditionB);

                        if (readable)
                        {
                            RangeConditionDto rangeConditionDto = new RangeConditionDto(
                                conditionA: conditionA,
                                conditionB: conditionB
                            );
                            result = (T)Convert.ChangeType(rangeConditionDto, typeof(T));
                            return true;
                        }

                        result = default(T);
                        return false;
                    }

                case true when typeof(T) == typeof(NotConditionDto):
                    {
                        PoseConditionDto[] conditions;
                        conditions = UMI3DSerializer.ReadArray<PoseConditionDto>(container);

                        if (conditions != null)
                        {
                            NotConditionDto notConditionDto = new NotConditionDto(
                                conditions: conditions
                            );
                            result = (T)Convert.ChangeType(notConditionDto, typeof(T));
                            readable = true;
                            return true;
                        }

                        result = default(T);
                        readable = false;
                        return false;
                    }
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }


        /// <inheritdoc/>
        public override bool Write<T>(T value, out Bytable bytable)
        {
            switch (value)
            {
                case LocalInfoRequestParameterValue localInfovalue:
                    bytable = UMI3DSerializer.Write(localInfovalue.read)
                        + UMI3DSerializer.Write(localInfovalue.write);
                    break;
                case UserCameraPropertiesDto camera:
                    bytable = UMI3DSerializer.Write(camera.scale)
                        + UMI3DSerializer.Write(camera.projectionMatrix)
                        + UMI3DSerializer.Write(camera.boneType);
                    break;
                case EnumParameterDto<string> param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.Enum)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value);
                    break;
                case BooleanParameterDto param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.Bool)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value);
                    break;
                case FloatParameterDto param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.Float)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value);
                    break;
                case IntegerParameterDto param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.Int)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value);

                    break;
                case StringParameterDto param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.String)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value);
                    break;
                case UploadFileParameterDto param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.StringUploadFile)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value)
                        + UMI3DSerializer.Write(param.authorizedExtensions);
                    break;
                case IntegerRangeParameterDto param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.IntRange)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value)
                        + UMI3DSerializer.Write(param.min)
                        + UMI3DSerializer.Write(param.max)
                        + UMI3DSerializer.Write(param.increment);
                    break;
                case FloatRangeParameterDto param:
                    bytable = UMI3DSerializer.Write(UMI3DParameterKeys.FloatRange)
                        + UMI3DSerializer.Write(param.privateParameter)
                        + UMI3DSerializer.Write(param.value)
                        + UMI3DSerializer.Write(param.min)
                        + UMI3DSerializer.Write(param.max)
                        + UMI3DSerializer.Write(param.increment);
                    break;
                case UMI3DRenderedNodeDto.MaterialOverrideDto material:
                    bytable = UMI3DSerializer.Write(material.newMaterialId)
                        + UMI3DSerializer.Write(material.addMaterialIfNotExists)
                        + UMI3DSerializer.WriteCollection(material.overridedMaterialsId);
                    break;
                case ScalableTextureDto scalableTextureDto:
                    bytable = UMI3DSerializer.WriteCollection(scalableTextureDto.variants)
                        + UMI3DSerializer.Write(scalableTextureDto.animationId)
                        + UMI3DSerializer.Write(scalableTextureDto.audioSourceId)
                        + UMI3DSerializer.Write(scalableTextureDto.streamingFromUserId)
                        + UMI3DSerializer.Write(scalableTextureDto.scale);
                    break;
                case TextureDto textureDto:
                    bytable = UMI3DSerializer.WriteCollection(textureDto.variants)
                        + UMI3DSerializer.Write(textureDto.animationId)
                        + UMI3DSerializer.Write(textureDto.audioSourceId)
                        + UMI3DSerializer.Write(textureDto.streamingFromUserId);
                    break;
                case ResourceDto resourceDto:
                    bytable = UMI3DSerializer.WriteCollection(resourceDto.variants);
                    break;
                case FileDto fileDto:
                    bytable = UMI3DSerializer.Write(fileDto.url)
                        + UMI3DSerializer.Write(fileDto.format)
                        + UMI3DSerializer.Write(fileDto.extension)
                        + UMI3DSerializer.Write(fileDto.metrics?.resolution ?? 0)
                        + UMI3DSerializer.Write(fileDto.metrics?.size ?? 0f)
                        + UMI3DSerializer.Write(fileDto.pathIfInBundle)
                        + UMI3DSerializer.Write(fileDto.libraryKey);
                    break;
                case RedirectionDto redirection:
                    bytable = UMI3DSerializer.Write(redirection.media)
                        + UMI3DSerializer.Write(redirection.gate);
                    break;
                case ForceLogoutDto forceLogout:
                    bytable = UMI3DSerializer.Write(forceLogout.reason);
                    break;
                case MediaDto media:
                    bytable = UMI3DSerializer.Write(media.name)
                        + UMI3DSerializer.Write(media.icon2D)
                        + UMI3DSerializer.Write(media.icon3D)
                        + UMI3DSerializer.Write(media.url);
                    break;
                case GateDto gate:
                    bytable = UMI3DSerializer.Write(gate.gateId)
                        + UMI3DSerializer.WriteCollection(gate.metaData);
                    break;
                case VoiceDto voice:
                    bytable = UMI3DSerializer.Write(voice.url)
                        + UMI3DSerializer.Write(voice.login)
                        + UMI3DSerializer.Write(voice.password)
                        + UMI3DSerializer.Write(voice.channelName);
                    break;
                #region Pose Dto
                case AnchoredBonePoseDto anchorBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)1)
                        + UMI3DSerializer.Write(anchorBonePoseDto.bone)
                        + UMI3DSerializer.Write(anchorBonePoseDto.Position)
                        + UMI3DSerializer.Write(anchorBonePoseDto.Rotation)
                        + UMI3DSerializer.Write(anchorBonePoseDto.otherBone);
                    break;
                case NodeAnchoredBonePoseDto nodePositionAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)2)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.bone)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.Position)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.Rotation)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.node);
                    break;
                case FloorAnchoredBonePoseDto floorAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)3)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.bone)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.Position)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.Rotation);
                    break;
                case BonePoseDto bonePoseDto:
                    bytable = UMI3DSerializer.Write((int)0)
                        + UMI3DSerializer.Write(bonePoseDto.bone)
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
                        + UMI3DSerializer.Write(poseOverriderDto.composable)
                        + UMI3DSerializer.WriteCollection(poseOverriderDto.poseConditions);
                    break;
                case DurationDto durationDto:
                    bytable = UMI3DSerializer.Write(durationDto.duration)
                        + UMI3DSerializer.Write(durationDto.min)
                        + UMI3DSerializer.Write(durationDto.max);
                    break;
                #endregion
                #region PoseCondition Dto
                case MagnitudeConditionDto magnitudeConditionDto:
                    bytable = UMI3DSerializer.Write((int)1)
                        + UMI3DSerializer.Write(magnitudeConditionDto.magnitude)
                        + UMI3DSerializer.Write(magnitudeConditionDto.boneOrigine)
                        + UMI3DSerializer.Write(magnitudeConditionDto.targetObjectId);
                    break;
                case RangeConditionDto rangeConditionDto:
                    bytable = UMI3DSerializer.Write((int)2)
                        + UMI3DSerializer.Write(rangeConditionDto.conditionA)
                        + UMI3DSerializer.Write(rangeConditionDto.conditionB);
                    break;
                case BoneRotationConditionDto boneRotationConditionDto:
                    bytable = UMI3DSerializer.Write((int)3)
                        + UMI3DSerializer.Write(boneRotationConditionDto.boneId)
                        + UMI3DSerializer.Write(boneRotationConditionDto.rotation);
                    break;
                case DirectionConditionDto directionConditionDto:
                    bytable = UMI3DSerializer.Write((int)4)
                        + UMI3DSerializer.Write(directionConditionDto.direction);
                    break;
                case NotConditionDto notConditionDto:
                    bytable = UMI3DSerializer.Write((int)5)
                        + UMI3DSerializer.WriteCollection(notConditionDto.conditions);
                    break;
                case UserScaleConditionDto userScaleConditinoDto:
                    bytable = UMI3DSerializer.Write((int)6)
                        + UMI3DSerializer.Write(userScaleConditinoDto.scale);
                    break;
                case ScaleConditionDto scaleConditionDto:
                    bytable = UMI3DSerializer.Write((int)7)
                        + UMI3DSerializer.Write(scaleConditionDto.scale);
                    break;
                #endregion

                default:
                    if (typeof(T) == typeof(ResourceDto))
                    {
                        // value is null
                        bytable = UMI3DSerializer.WriteCollection(new System.Collections.Generic.List<FileDto>());
                        return true;
                    }
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}