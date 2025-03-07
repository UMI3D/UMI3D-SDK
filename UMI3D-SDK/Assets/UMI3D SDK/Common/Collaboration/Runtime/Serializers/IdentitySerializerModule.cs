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
using System.Collections.Generic;
using System.ComponentModel;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.collaboration.dto.voip;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class IdentitySerializerModule : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(PrivateIdentityDto) => true,
                true when typeof(T) == typeof(EnvironmentConnectionDto) => true,
                true when typeof(T) == typeof(AssetLibraryDto) => true,
                true when typeof(T) == typeof(UMI3DLocalAssetFilesDto) => true,
                true when typeof(T) == typeof(FileListDto) => true,
                true when typeof(T) == typeof(AssetMetricDto) => true,
                _ => null
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when (typeof(T) == typeof(AssetMetricDto)):
                    {
                        readable = true;

                        readable &= UMI3DSerializer.TryRead(container, out int resolution);
                        readable &= UMI3DSerializer.TryRead(container, out float size);

                        if (readable)
                        {
                            AssetMetricDto metrics = new AssetMetricDto()
                            {
                                resolution = resolution,
                                size = size
                            };
                            result = (T)Convert.ChangeType(metrics, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                            readable = false;
                            return false;
                        }

                        return true;
                    }
                case true when (typeof(T) == typeof(FileListDto)):
                    {
                        readable = true;

                        readable &= UMI3DSerializer.TryRead(container, out string baseUrl);

                        List<string> files = UMI3DSerializer.ReadList<string>(container);

                        if (readable)
                        {
                            FileListDto fileList = new FileListDto()
                            {
                                baseUrl = baseUrl,
                                files = files
                            };
                            result = (T)Convert.ChangeType(fileList, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                            readable = false;
                            return false;
                        }

                        return true;
                    }
                case true when (typeof(T) == typeof(UMI3DLocalAssetFilesDto)):
                    {
                        readable = true;

                        readable &= UMI3DSerializer.TryRead(container, out string name);
                        readable &= UMI3DSerializer.TryRead(container, out FileListDto files);
                        readable &= UMI3DSerializer.TryRead(container, out AssetMetricDto metrics);

                        List<string> formats = UMI3DSerializer.ReadList<string>(container);

                        if (readable)
                        {
                            UMI3DLocalAssetFilesDto localAssetFiles = new UMI3DLocalAssetFilesDto()
                            {
                                name = name,
                                files = files,
                                metrics = new AssetMetricDto() { resolution = 1, size = 0 },
                                formats = formats
                            };
                            result = (T)Convert.ChangeType(localAssetFiles, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                            readable = false;
                            return false;
                        }

                        return true;
                    }
                case true when (typeof(T) == typeof(AssetLibraryDto)):
                    {
                        readable = true;

                        readable &= UMI3DSerializer.TryRead(container, out ulong id);
                        readable &= UMI3DSerializer.TryRead(container, out string libraryId);
                        readable &= UMI3DSerializer.TryRead(container, out string baseUrl);
                        readable &= UMI3DSerializer.TryRead(container, out string version);

                        List<UMI3DLocalAssetFilesDto> variants = UMI3DSerializer.ReadList<UMI3DLocalAssetFilesDto>(container);

                        if (readable)
                        {
                            AssetLibraryDto assetLibrary = new AssetLibraryDto()
                            {
                                id = id,
                                libraryId = libraryId,
                                baseUrl = baseUrl,
                                version = version,
                                variants = variants
                            };
                            result = (T)Convert.ChangeType(assetLibrary, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                            readable = false;
                            return false;
                        }

                        return true;
                    }
                case true when (typeof(T) == typeof(EnvironmentConnectionDto)):
                    {
                        readable = true;

                        readable &= UMI3DSerializer.TryRead(container, out string name);
                        readable &= UMI3DSerializer.TryRead(container, out string httpUrl);
                        readable &= UMI3DSerializer.TryRead(container, out string resourcesUrl);
                        readable &= UMI3DSerializer.TryRead(container, out bool authorizationInHeader);
                        readable &= UMI3DSerializer.TryRead(container, out string forgeHost);
                        readable &= UMI3DSerializer.TryRead(container, out string forgeMasterServerHost);
                        readable &= UMI3DSerializer.TryRead(container, out string forgeNatServerHost);
                        readable &= UMI3DSerializer.TryRead(container, out ushort forgeServerPort);
                        readable &= UMI3DSerializer.TryRead(container, out ushort forgeMasterServerPort);
                        readable &= UMI3DSerializer.TryRead(container, out ushort forgeNatServerPort);
                        readable &= UMI3DSerializer.TryRead(container, out string version);

                        if (readable)
                        {
                            EnvironmentConnectionDto environmentConnection = new EnvironmentConnectionDto()
                            {
                                name = name,
                                httpUrl = httpUrl,
                                resourcesUrl = resourcesUrl,
                                authorizationInHeader = authorizationInHeader,
                                forgeHost = forgeHost,
                                forgeMasterServerHost = forgeMasterServerHost,
                                forgeNatServerHost = forgeNatServerHost,
                                forgeServerPort = forgeServerPort,
                                forgeMasterServerPort = forgeMasterServerPort,
                                forgeNatServerPort = forgeNatServerPort,
                                version = version
                            };
                            result = (T)Convert.ChangeType(environmentConnection, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                            readable = false;
                            return false;
                        }

                        return true;
                    }
                case true when (typeof(T) == typeof(PrivateIdentityDto)):
                    {
                        readable = true;

                        readable &= UMI3DSerializer.TryRead(container, out string globalToken);
                        readable &= UMI3DSerializer.TryRead(container, out EnvironmentConnectionDto connectionDto);

                        List<AssetLibraryDto> libraries = UMI3DSerializer.ReadList<AssetLibraryDto>(container);

                        readable &= UMI3DSerializer.TryRead(container, out string localToken);
                        readable &= UMI3DSerializer.TryRead(container, out string headerToken);
                        readable &= UMI3DSerializer.TryRead(container, out string key);
                        readable &= UMI3DSerializer.TryRead(container, out string guid);
                        readable &= UMI3DSerializer.TryRead(container, out ulong userId);
                        readable &= UMI3DSerializer.TryRead(container, out string login);
                        readable &= UMI3DSerializer.TryRead(container, out string displayName);
                        readable &= UMI3DSerializer.TryRead(container, out bool isServer);

                        if (readable)
                        {
                            PrivateIdentityDto privateIdentityDto = new PrivateIdentityDto()
                            {
                                globalToken = globalToken,
                                connectionDto = connectionDto,
                                libraries = libraries,
                                localToken = localToken,
                                headerToken = headerToken,
                                key = key,
                                guid = guid,
                                userId = userId,
                                login = login,
                                displayName = displayName,
                                isServer = isServer
                            };
                            result = (T)Convert.ChangeType(privateIdentityDto, typeof(T));
                        }
                        else
                        {
                            result = default(T);
                            readable = false;
                            return false;
                        }

                        return true;
                    }
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case PrivateIdentityDto privateIdentityDto:
                    bytable = UMI3DSerializer.Write(privateIdentityDto.globalToken)
                            + UMI3DSerializer.Write(privateIdentityDto.connectionDto)
                            + UMI3DSerializer.WriteCollection(privateIdentityDto.libraries)
                            + UMI3DSerializer.Write(privateIdentityDto.localToken)
                            + UMI3DSerializer.Write(privateIdentityDto.headerToken)
                            + UMI3DSerializer.Write(privateIdentityDto.key)
                            + UMI3DSerializer.Write(privateIdentityDto.guid)
                            + UMI3DSerializer.Write(privateIdentityDto.userId)
                            + UMI3DSerializer.Write(privateIdentityDto.login)
                            + UMI3DSerializer.Write(privateIdentityDto.displayName)
                            + UMI3DSerializer.Write(privateIdentityDto.isServer);
                    return true;
                case EnvironmentConnectionDto connectionDto:
                    bytable = UMI3DSerializer.Write(connectionDto.name)
                            + UMI3DSerializer.Write(connectionDto.httpUrl)
                            + UMI3DSerializer.Write(connectionDto.resourcesUrl)
                            + UMI3DSerializer.Write(connectionDto.authorizationInHeader)
                            + UMI3DSerializer.Write(connectionDto.forgeHost)
                            + UMI3DSerializer.Write(connectionDto.forgeMasterServerHost)
                            + UMI3DSerializer.Write(connectionDto.forgeNatServerHost)
                            + UMI3DSerializer.Write(connectionDto.forgeServerPort)
                            + UMI3DSerializer.Write(connectionDto.forgeMasterServerPort)
                            + UMI3DSerializer.Write(connectionDto.forgeNatServerPort)
                            + UMI3DSerializer.Write(connectionDto.version);
                    return true;
                case AssetLibraryDto assetLibraryDto:
                    bytable = UMI3DSerializer.Write(assetLibraryDto.id)
                            + UMI3DSerializer.Write(assetLibraryDto.libraryId)
                            + UMI3DSerializer.Write(assetLibraryDto.baseUrl)
                            + UMI3DSerializer.Write(assetLibraryDto.version)
                            + UMI3DSerializer.WriteCollection(assetLibraryDto.variants);
                    return true;
                case UMI3DLocalAssetFilesDto uMI3DLocalAssetFilesDto:
                    bytable = UMI3DSerializer.Write(uMI3DLocalAssetFilesDto.name)
                            + UMI3DSerializer.Write(uMI3DLocalAssetFilesDto.files)
                            + UMI3DSerializer.Write(uMI3DLocalAssetFilesDto.metrics)
                            + UMI3DSerializer.WriteCollection(uMI3DLocalAssetFilesDto.formats);
                    return true;
                case FileListDto fileListDto:
                    bytable = UMI3DSerializer.Write(fileListDto.baseUrl)
                            + UMI3DSerializer.WriteCollection(fileListDto.files);
                    return true;
                case AssetMetricDto assetMetricDto:
                    bytable = UMI3DSerializer.Write(assetMetricDto.resolution)
                            + UMI3DSerializer.Write(assetMetricDto.size);
                    return true;
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
        }
    }
}