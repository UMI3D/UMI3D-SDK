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
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {

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
                            scale = UMI3DNetworkingHelper.Read<float>(container),
                            projectionMatrix = UMI3DNetworkingHelper.Read<SerializableMatrix4x4>(container),
                            boneType = UMI3DNetworkingHelper.Read<uint>(container)
                        };
                        result = (T)Convert.ChangeType(usercam, typeof(T));
                    }
                    else
                    {
                        result = default(T);
                    }

                    return true;

                case true when typeof(T) == typeof(VoiceDto):
                    if (UMI3DNetworkingHelper.TryRead(container, out string voiceUrl) &&
                        UMI3DNetworkingHelper.TryRead(container, out string login) &&
                        UMI3DNetworkingHelper.TryRead(container, out string password) &&
                        UMI3DNetworkingHelper.TryRead(container, out string channel))
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
                    if (UMI3DNetworkingHelper.TryRead(container, out type)
                        && UMI3DNetworkingHelper.TryRead(container, out rot))
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
                    if (UMI3DNetworkingHelper.TryRead(container, out bindingId)
                        && UMI3DNetworkingHelper.TryRead(container, out rigName)
                        && UMI3DNetworkingHelper.TryRead(container, out active)
                        && UMI3DNetworkingHelper.TryRead(container, out boneType)
                        && UMI3DNetworkingHelper.TryRead(container, out objectId)
                        && UMI3DNetworkingHelper.TryRead(container, out offsetPosition)
                        && UMI3DNetworkingHelper.TryRead(container, out offsetRotation)
                        && UMI3DNetworkingHelper.TryRead(container, out offsetScale)
                        && UMI3DNetworkingHelper.TryRead(container, out syncPosition)
                        && UMI3DNetworkingHelper.TryRead(container, out syncRotation)
                        && UMI3DNetworkingHelper.TryRead(container, out freezeWorldScale))
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
                    if (UMI3DNetworkingHelper.TryRead(container, out id)
                        && UMI3DNetworkingHelper.TryRead(container, out Name)
                        && UMI3DNetworkingHelper.TryRead(container, out IsActive)
                        && UMI3DNetworkingHelper.TryRead(container, out HoverPose)
                        && UMI3DNetworkingHelper.TryRead(container, out isRelativeToNode)
                        && UMI3DNetworkingHelper.TryRead(container, out RightHandPosition)
                        && UMI3DNetworkingHelper.TryRead(container, out RightHandEulerRotation)
                        && UMI3DNetworkingHelper.TryRead(container, out LeftHandPosition)
                        && UMI3DNetworkingHelper.TryRead(container, out LeftHandEulerRotation))
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
                    readable = UMI3DNetworkingHelper.TryRead<bool>(container, out conf.allAvailableByDefault);
                    readable &= UMI3DNetworkingHelper.TryRead<string>(container, out conf.defaultStateName);

                    if (readable)
                    {
                        readable = UMI3DNetworkingHelper.TryRead<int>(container, out int nbEmotes);
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

                    readable = UMI3DNetworkingHelper.TryRead<ulong>(container, out e.id);
                    readable &= UMI3DNetworkingHelper.TryRead<string>(container, out e.label);
                    readable &= UMI3DNetworkingHelper.TryRead<string>(container, out e.stateName);
                    readable &= UMI3DNetworkingHelper.TryRead<bool>(container, out e.available);
                    readable &= UMI3DNetworkingHelper.TryRead<FileDto>(container, out e.iconResource);

                    if (!readable)
                        return false;
                    result = (T)Convert.ChangeType(e, typeof(T));
                    return true;
                case true when typeof(T) == typeof(UMI3DRenderedNodeDto.MaterialOverrideDto):
                    var mat = new UMI3DRenderedNodeDto.MaterialOverrideDto();
                    readable = UMI3DNetworkingHelper.TryRead<ulong>(container, out mat.newMaterialId);
                    if (readable)
                    {
                        readable = UMI3DNetworkingHelper.TryRead<bool>(container, out mat.addMaterialIfNotExists);
                        if (readable)
                        {
                            mat.overridedMaterialsId = UMI3DNetworkingHelper.ReadList<string>(container);
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
                            variants = UMI3DNetworkingHelper.ReadList<FileDto>(container)
                        };
                        if (UMI3DNetworkingHelper.TryRead(container, out string animationId)
                            && UMI3DNetworkingHelper.TryRead(container, out string audioSourceId)
                            && UMI3DNetworkingHelper.TryRead(container, out string streamingFromUserId)
                            && UMI3DNetworkingHelper.TryRead(container, out float scale))
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
                            variants = UMI3DNetworkingHelper.ReadList<FileDto>(container)
                        };
                        if (texture.variants.Count == 0)
                        {
                            result = default(T);
                            readable = true;
                            return true;
                        }
                        else if (UMI3DNetworkingHelper.TryRead(container, out string animationId)
                          && UMI3DNetworkingHelper.TryRead(container, out string audioSourceId)
                          && UMI3DNetworkingHelper.TryRead(container, out string streamingFromUserId))
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
                        variants = UMI3DNetworkingHelper.ReadList<FileDto>(container)
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
                    readable = UMI3DNetworkingHelper.TryRead<string>(container, out file.url)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.format)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.extension)
                               && UMI3DNetworkingHelper.TryRead<int>(container, out file.metrics.resolution)
                               && UMI3DNetworkingHelper.TryRead<float>(container, out file.metrics.size)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.pathIfInBundle)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.libraryKey);
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
                        UMI3DNetworkingHelper.TryRead<MediaDto>(container, out media)
                        && UMI3DNetworkingHelper.TryRead<GateDto>(container, out gate)
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
                        UMI3DNetworkingHelper.TryRead<string>(container, out reason)
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
                        UMI3DNetworkingHelper.TryRead<string>(container, out gateid)
                        )
                    {
                        data = UMI3DNetworkingHelper.ReadArray<byte>(container);

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
                        UMI3DNetworkingHelper.TryRead(container, out name)
                        && UMI3DNetworkingHelper.TryRead(container, out icon2D)
                        && UMI3DNetworkingHelper.TryRead(container, out icon3D)
                        && UMI3DNetworkingHelper.TryRead(container, out versionMajor)
                        && UMI3DNetworkingHelper.TryRead(container, out versionMinor)
                        && UMI3DNetworkingHelper.TryRead(container, out versionStatus)
                        && UMI3DNetworkingHelper.TryRead(container, out versionDate)
                        && UMI3DNetworkingHelper.TryRead(container, out url)
                        )
                    {
                        var _media = new MediaDto
                        {
                            name = name,
                            icon2D = icon2D,
                            icon3D = icon3D,
                            versionMajor = versionMajor,
                            versionMinor = versionMinor,
                            versionStatus = versionStatus,
                            versionDate = versionDate,
                            url = url
                        };
                        readable = true;
                        result = (T)Convert.ChangeType(_media, typeof(T));

                        return true;
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

        public override bool Write<T>(T value, out Bytable bytable)
        {
            switch (value)
            {
                case LocalInfoRequestParameterValue localInfovalue:
                    bytable = UMI3DNetworkingHelper.Write(localInfovalue.read)
                        + UMI3DNetworkingHelper.Write(localInfovalue.write);
                    break;
                case UserCameraPropertiesDto camera:
                    bytable = UMI3DNetworkingHelper.Write(camera.scale)
                        + UMI3DNetworkingHelper.Write(camera.projectionMatrix)
                        + UMI3DNetworkingHelper.Write(camera.boneType);
                    break;
                case EnumParameterDto<string> param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Enum)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value);
                    break;
                case BooleanParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Bool)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value);
                    break;
                case FloatParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Float)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value);
                    break;
                case IntegerParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Int)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value);

                    break;
                case StringParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.String)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value);
                    break;
                case UploadFileParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.StringUploadFile)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value)
                        + UMI3DNetworkingHelper.Write(param.authorizedExtensions);
                    break;
                case IntegerRangeParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.IntRange)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value)
                        + UMI3DNetworkingHelper.Write(param.min)
                        + UMI3DNetworkingHelper.Write(param.max)
                        + UMI3DNetworkingHelper.Write(param.increment);
                    break;
                case FloatRangeParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.FloatRange)
                        + UMI3DNetworkingHelper.Write(param.privateParameter)
                        + UMI3DNetworkingHelper.Write(param.value)
                        + UMI3DNetworkingHelper.Write(param.min)
                        + UMI3DNetworkingHelper.Write(param.max)
                        + UMI3DNetworkingHelper.Write(param.increment);
                    break;
                case UMI3DRenderedNodeDto.MaterialOverrideDto material:
                    bytable = UMI3DNetworkingHelper.Write(material.newMaterialId)
                        + UMI3DNetworkingHelper.Write(material.addMaterialIfNotExists)
                        + UMI3DNetworkingHelper.WriteCollection(material.overridedMaterialsId);
                    break;
                case ScalableTextureDto scalableTextureDto:
                    bytable = UMI3DNetworkingHelper.WriteCollection(scalableTextureDto.variants)
                        + UMI3DNetworkingHelper.Write(scalableTextureDto.animationId)
                        + UMI3DNetworkingHelper.Write(scalableTextureDto.audioSourceId)
                        + UMI3DNetworkingHelper.Write(scalableTextureDto.streamingFromUserId)
                        + UMI3DNetworkingHelper.Write(scalableTextureDto.scale);
                    break;
                case TextureDto textureDto:
                    bytable = UMI3DNetworkingHelper.WriteCollection(textureDto.variants)
                        + UMI3DNetworkingHelper.Write(textureDto.animationId)
                        + UMI3DNetworkingHelper.Write(textureDto.audioSourceId)
                        + UMI3DNetworkingHelper.Write(textureDto.streamingFromUserId);
                    break;
                case ResourceDto resourceDto:
                    bytable = UMI3DNetworkingHelper.WriteCollection(resourceDto.variants);
                    break;
                case FileDto fileDto:
                    bytable = UMI3DNetworkingHelper.Write(fileDto.url)
                        + UMI3DNetworkingHelper.Write(fileDto.format)
                        + UMI3DNetworkingHelper.Write(fileDto.extension)
                        + UMI3DNetworkingHelper.Write(fileDto.metrics?.resolution ?? 0)
                        + UMI3DNetworkingHelper.Write(fileDto.metrics?.size ?? 0f)
                        + UMI3DNetworkingHelper.Write(fileDto.pathIfInBundle)
                        + UMI3DNetworkingHelper.Write(fileDto.libraryKey);
                    break;
                case RedirectionDto redirection:
                    bytable = UMI3DNetworkingHelper.Write(redirection.media)
                        + UMI3DNetworkingHelper.Write(redirection.gate);
                    break;
                case ForceLogoutDto forceLogout:
                    bytable = UMI3DNetworkingHelper.Write(forceLogout.reason);
                    break;
                case MediaDto media:
                    bytable = UMI3DNetworkingHelper.Write(media.name)
                        + UMI3DNetworkingHelper.Write(media.icon2D)
                        + UMI3DNetworkingHelper.Write(media.icon3D)
                        + UMI3DNetworkingHelper.Write(media.versionMajor)
                        + UMI3DNetworkingHelper.Write(media.versionMinor)
                        + UMI3DNetworkingHelper.Write(media.versionStatus)
                        + UMI3DNetworkingHelper.Write(media.versionDate)
                        + UMI3DNetworkingHelper.Write(media.url);
                    break;
                case GateDto gate:
                    bytable = UMI3DNetworkingHelper.Write(gate.gateId)
                        + UMI3DNetworkingHelper.WriteCollection(gate.metaData);
                    break;
                case VoiceDto voice:
                    bytable = UMI3DNetworkingHelper.Write(voice.url)
                        + UMI3DNetworkingHelper.Write(voice.login)
                        + UMI3DNetworkingHelper.Write(voice.password)
                        + UMI3DNetworkingHelper.Write(voice.channelName);
                    break;
                default:
                    if (typeof(T) == typeof(ResourceDto))
                    {
                        // value is null
                        bytable = UMI3DNetworkingHelper.WriteCollection(new System.Collections.Generic.List<FileDto>());
                        return true;
                    }
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}