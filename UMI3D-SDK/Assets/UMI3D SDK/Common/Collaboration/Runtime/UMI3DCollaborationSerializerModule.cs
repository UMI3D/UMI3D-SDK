/*
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
using umi3d.common.interaction;
using umi3d.common.userCapture;

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

                case true when typeof(T) == typeof(BoneDto):
                    uint type;
                    SerializableVector4 rot;
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
                case true when typeof(T) == typeof(BoneBindingDto):
                    string bindingId;
                    string rigName;
                    bool active;
                    uint boneType;
                    ulong objectId;
                    bool syncPosition;
                    bool syncRotation;
                    bool freezeWorldScale;
                    SerializableVector3 offsetPosition;
                    SerializableVector4 offsetRotation;
                    SerializableVector3 offsetScale;
                    if (UMI3DSerializer.TryRead(container, out bindingId)
                        && UMI3DSerializer.TryRead(container, out rigName)
                        && UMI3DSerializer.TryRead(container, out active)
                        && UMI3DSerializer.TryRead(container, out boneType)
                        && UMI3DSerializer.TryRead(container, out objectId)
                        && UMI3DSerializer.TryRead(container, out offsetPosition)
                        && UMI3DSerializer.TryRead(container, out offsetRotation)
                        && UMI3DSerializer.TryRead(container, out offsetScale)
                        && UMI3DSerializer.TryRead(container, out syncPosition)
                        && UMI3DSerializer.TryRead(container, out syncRotation)
                        && UMI3DSerializer.TryRead(container, out freezeWorldScale))
                    {
                        var bone = new BoneBindingDto()
                        {
                            bindingId = bindingId,
                            rigName = rigName,
                            active = active,
                            boneType = boneType,
                            objectId = objectId,
                            offsetPosition = offsetPosition,
                            offsetRotation = offsetRotation,
                            offsetScale = offsetScale,
                            syncPosition = syncPosition,
                            syncRotation = syncRotation,
                            freezeWorldScale = freezeWorldScale
                        };
                        result = (T)Convert.ChangeType(bone, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;
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
                    readable = UMI3DSerializer.TryRead<bool>(container, out conf.allAvailableByDefault);
                    readable &= UMI3DSerializer.TryRead<string>(container, out conf.defaultStateName);

                    if (readable)
                    {
                        readable = UMI3DSerializer.TryRead<int>(container, out int nbEmotes);
                        if (readable)
                        {
                            for (uint i = 0; i < nbEmotes; i++)
                            {
                                UMI3DEmoteDto emote;
                                Read<UMI3DEmoteDto>(container, out readable, out emote);
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

                    readable = UMI3DSerializer.TryRead<ulong>(container, out e.id);
                    readable &= UMI3DSerializer.TryRead<string>(container, out e.label);
                    readable &= UMI3DSerializer.TryRead<string>(container, out e.stateName);
                    readable &= UMI3DSerializer.TryRead<bool>(container, out e.available);
                    readable &= UMI3DSerializer.TryRead<FileDto>(container, out e.iconResource);

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
                    uint idKey = 0;
                    ulong userId, parentId;
                    float skeletonHighOffset, refreshFrequency;
                    SerializableVector3 position;
                    SerializableVector4 rotation;

                    if (
                        UMI3DSerializer.TryRead(container, out idKey)
                        && UMI3DSerializer.TryRead(container, out userId)
                        && UMI3DSerializer.TryRead(container, out parentId)
                        && UMI3DSerializer.TryRead(container, out skeletonHighOffset)
                        && UMI3DSerializer.TryRead(container, out position)
                        && UMI3DSerializer.TryRead(container, out rotation)
                        && UMI3DSerializer.TryRead(container, out refreshFrequency)
                        )
                    {
                        System.Collections.Generic.List<BoneDto> bones = UMI3DSerializer.ReadList<BoneDto>(container);

                        if (bones != default)
                        {
                            var trackingFrame = new UserTrackingFrameDto
                            {
                                userId = userId,
                                parentId = parentId,
                                skeletonHighOffset = skeletonHighOffset,
                                position = position,
                                rotation = rotation,
                                refreshFrequency = refreshFrequency,
                                bones = bones
                            };
                            readable = true;
                            result = (T)Convert.ChangeType(trackingFrame, typeof(T));

                            return true;
                        }
                        else
                        {
                            result = default(T);
                            readable = false;
                            return false;
                        }
                    }
                    result = default(T);
                    readable = false;
                    return false;
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