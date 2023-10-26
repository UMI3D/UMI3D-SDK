using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d;
using umi3d.cdk;
using umi3d.common;
using UnityEngine;

public class DistantEnvironmentLoader : AbstractLoader
{
    UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.7", "*");

    public override UMI3DVersion.VersionCompatibility version => _version;

    public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
    {
        return data.dto is DistantEnvironmentDto;
    }

    public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
    {
        if(value.dto is DistantEnvironmentDto distantDto)
        {
            UnityEngine.Debug.Log("Read a distant Environment");
            await Task.CompletedTask;
        }
    }

    public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
    {
        return Task.FromResult(false);
    }

    public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
    {
        return Task.FromResult(false);
    }
}
