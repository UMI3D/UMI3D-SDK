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
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.voip;
using umi3d.common.interaction;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Helper module to serialize objects related to the Collaboration module.
    /// </summary>
    public class UMI3DCollaborationSerializerModule : UMI3DSerializerModule
    {
        /// <inheritdoc/>
        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            ulong id;
            switch (true)
            {
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

                case true when typeof(T) == typeof(MaterialOverrideDto):
                    readable = UMI3DSerializer.TryRead<ulong>(container, out id);
                    if (readable)
                    {
                        readable = UMI3DSerializer.TryRead<bool>(container, out bool addMaterialIfNotExists);
                        if (readable)
                        {
                            var mat = new MaterialOverrideDto();
                            mat.overridedMaterialsId = UMI3DSerializer.ReadList<string>(container);
                            mat.newMaterialId = id;
                            mat.addMaterialIfNotExists = addMaterialIfNotExists;

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
                    {
                        string url = null;
                        string pathIfInBundle = null;
                        string libraryKey = null;
                        string format = null;
                        string extension = null;
                        string authorization = null;
                        int resolution = 0;
                        float size = 0;

                        readable = UMI3DSerializer.TryRead<string>(container, out url)
                                   && UMI3DSerializer.TryRead<string>(container, out format)
                                   && UMI3DSerializer.TryRead<string>(container, out extension)
                                   && UMI3DSerializer.TryRead<int>(container, out resolution)
                                   && UMI3DSerializer.TryRead<float>(container, out size)
                                   && UMI3DSerializer.TryRead<string>(container, out pathIfInBundle)
                                   && UMI3DSerializer.TryRead<string>(container, out libraryKey);
                        if (readable)
                        {
                            var file = new FileDto
                            {
                                url = url,
                                pathIfInBundle = pathIfInBundle,
                                libraryKey = libraryKey,
                                format = format,
                                extension = extension,
                                authorization = authorization,
                                metrics = new AssetMetricDto()
                                {
                                    resolution = resolution,
                                    size = size
                                }
                            };

                            result = (T)Convert.ChangeType(file, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                        }

                        return true;
                    }
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
                    {
                        string name, url;
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
                    }

                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }

        /// <inheritdoc/>
        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case LocalInfoRequestParameterValue localInfovalue:
                    bytable = UMI3DSerializer.Write(localInfovalue.read)
                        + UMI3DSerializer.Write(localInfovalue.write);
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

                case MaterialOverrideDto material:
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

        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(LocalInfoRequestParameterValue) => true,
                true when typeof(T) == typeof(BooleanParameterDto) => true,
                true when typeof(T) == typeof(EnumParameterDto<string>) => true,
                true when typeof(T) == typeof(FloatParameterDto) => true,
                true when typeof(T) == typeof(IntegerParameterDto) => true,
                true when typeof(T) == typeof(StringParameterDto) => true,
                true when typeof(T) == typeof(UploadFileParameterDto) => true,
                true when typeof(T) == typeof(IntegerRangeParameterDto) => true,
                true when typeof(T) == typeof(FloatRangeParameterDto) => true,
                true when typeof(T) == typeof(MaterialOverrideDto) => true,
                true when typeof(T) == typeof(ScalableTextureDto) => true,
                true when typeof(T) == typeof(TextureDto) => true,
                true when typeof(T) == typeof(ResourceDto) => true,
                true when typeof(T) == typeof(FileDto) => true,
                true when typeof(T) == typeof(RedirectionDto) => true,
                true when typeof(T) == typeof(ForceLogoutDto) => true,
                true when typeof(T) == typeof(MediaDto) => true,
                true when typeof(T) == typeof(GateDto) => true,
                true when typeof(T) == typeof(VoiceDto) => true,
                true when typeof(T) == typeof(ResourceDto) => true,
                _ => null
            };
        }
    }
}