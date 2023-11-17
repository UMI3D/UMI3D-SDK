using BeardedManStudios.Forge.Networking.Unity;
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
    private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;
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
                distantEnvironments[distantDto.environmentID] = distantDto;
                UnityEngine.Debug.LogError(distantDto.environmentID+" "+distantDto.resourcesUrl);
                UMI3DEnvironmentLoader.DeclareNewEnvironment(distantDto.environmentID, distantDto.resourcesUrl);
                var e = UMI3DEnvironmentLoader.Instance.RegisterEntity(value.environmentId, distantDto.id, distantDto, null);
                //Id of the distant environment is the id of the DistantEnvironmentDto
                await UMI3DEnvironmentLoader.Instance.InstantiateNodes(distantDto.environmentID, distantDto.environmentDto.scenes);
                e.NotifyLoaded();
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
        if (value.property.property == UMI3DPropertyKeys.DistantEnvironment)
        {
            var obj = value.property.value as BinaryDto;
            UnityEngine.Debug.Log("Need To forward transaction");
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
    {
        //UnityEngine.Debug.Log($"Hello {value?.entity?.dto} {value.entity.dto is DistantEnvironmentDto} {value.propertyKey} {value.propertyKey == UMI3DPropertyKeys.DistantEnvironment}");
        if (value.propertyKey == UMI3DPropertyKeys.DistantEnvironment)
        {
            _SetUMI3DProperty(value);
            return true;
        }

        return false;
    }

    async void _SetUMI3DProperty(SetUMI3DPropertyContainerData value)
    {
        await Task.Yield();
        var obj = UMI3DSerializer.Read<BinaryDto>(value.container);

        MainThreadManager.Run(async () =>
        {
            //UnityEngine.Debug.Log($"Need To forward transaction {obj.data.Length} {obj.groupId}=={UMI3DOperationKeys.Transaction} {obj.environmentid}");

            ByteContainer container = new ByteContainer(obj.environmentid, value.container.timeStep, obj.data);
            uint TransactionId = UMI3DSerializer.Read<uint>(container);
            try
            {
                await UMI3DClientServer.transactionDispatcher.PerformTransaction(container);
            }
            catch (ArgumentException ae)
            {
                // HACK
            }
            catch (Exception ex)
            {
                UMI3DLogger.LogError("Error while performing transaction", scope);
                UMI3DLogger.LogException(ex, scope);
            }

        });
    }
}
