using inetum.unityUtils;
using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d;
using umi3d.cdk;
using umi3d.common;
using UnityEngine;

public class DistantEnvironmentLoader : AbstractLoader
{
    UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.7", "*");

    public override UMI3DVersion.VersionCompatibility version => _version;

    static Dictionary<ulong, DistantEnvironmentDto> distantEnvironments = new();

    public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
    {
        return data.dto is DistantEnvironmentDto;
    }

    public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
    {
        if(value.dto is DistantEnvironmentDto distantDto)
        {
            UnityEngine.Debug.Log($"Read a distant Environment Start {distantDto != null} {distantDto?.environmentDto != null} {distantDto?.environmentDto?.scenes != null}");
            try
            {
                distantEnvironments[distantDto.id] = distantDto;
                UMI3DEnvironmentLoader.DeclareNewEnvironment(distantDto.id, distantDto.resourcesUrl);
                //Id of the distant environment is the id of the DistantEnvironmentDto
                await UMI3DEnvironmentLoader.Instance.InstantiateNodes(distantDto.id,distantDto.environmentDto.scenes);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            UnityEngine.Debug.Log("Read a distant Environment End");
        }
    }

    public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
    {
        if (value.entity.dto is DistantEnvironmentDto v && value.property.property == UMI3DPropertyKeys.DistantEnvironment)
        {
            var obj = value.property.value;
            UnityEngine.Debug.Log("Need To forward transaction");
        }

        return Task.FromResult(false);
    }

    public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
    {
        if (value.entity.dto is DistantEnvironmentDto v && value.propertyKey == UMI3DPropertyKeys.DistantEnvironment)
        {
            var obj = value.container;


            UnityEngine.Debug.Log("Need To forward transaction");

        }

        return Task.FromResult(false);
    }
}
