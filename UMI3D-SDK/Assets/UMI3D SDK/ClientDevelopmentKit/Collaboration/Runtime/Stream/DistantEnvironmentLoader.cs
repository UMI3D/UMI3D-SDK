using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using inetum.unityUtils;
using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using umi3d;
using umi3d.cdk;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
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
        if (value.dto is DistantEnvironmentDto distantDto)
        {
            try
            {
                distantEnvironments[distantDto.id] = distantDto;
                UnityEngine.Debug.LogError(distantDto.id + " " + distantDto.resourcesUrl);
                UMI3DEnvironmentLoader.DeclareNewEnvironment(distantDto.id, distantDto.resourcesUrl);
                var e = UMI3DEnvironmentLoader.Instance.RegisterEntity(value.environmentId, distantDto.id, distantDto, null);
                //Id of the distant environment is the id of the DistantEnvironmentDto

                UMI3DEnvironmentLoader.Instance.RegisterEntity(distantDto.id, UMI3DGlobalID.EnvironementId, distantDto.environmentDto, null).NotifyLoaded();

                await UMI3DEnvironmentLoader.Instance.ReadUMI3DExtension(distantDto.id, distantDto.environmentDto, null);

                await UMI3DEnvironmentLoader.Instance.InstantiateNodes(distantDto.id, distantDto.environmentDto.scenes);

                foreach (var item in distantDto.binaries)
                    await ReadBinaryDto(item, 0, distantDto);
                
                e.NotifyLoaded();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
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
        if (value.propertyKey == UMI3DPropertyKeys.DistantEnvironment && value?.entity.dto is DistantEnvironmentDto dto)
        {
            _SetUMI3DProperty(value, dto);
            return true;
        }

        return false;
    }

    async void _SetUMI3DProperty(SetUMI3DPropertyContainerData value, DistantEnvironmentDto dto)
    {
        int index;
        BinaryDto obj;
        switch (value.operationId)
        {
            case UMI3DOperationKeys.SetEntityListAddProperty:
                index = UMI3DSerializer.Read<int>(value.container);
                obj = UMI3DSerializer.Read<BinaryDto>(value.container);
                MainThreadManager.Run(async () =>
                {
                    await ReadBinaryDto(obj, value.container.timeStep, dto);
                });
                break;
            case UMI3DOperationKeys.SetEntityListRemoveProperty:
                // RemoveUserAt(container.environmentId, dto, UMI3DSerializer.Read<int>(container));
                break;
            case UMI3DOperationKeys.SetEntityListProperty:
                index = UMI3DSerializer.Read<int>(value.container);
                obj = UMI3DSerializer.Read<BinaryDto>(value.container);
                MainThreadManager.Run(async () =>
                {
                    await ReadBinaryDto(obj, value.container.timeStep, dto);
                });
                break;
            default:
                var list = UMI3DSerializer.ReadList<BinaryDto>(value.container);
                MainThreadManager.Run(async () =>
                {
                   foreach (var item in list)
                        await ReadBinaryDto(item, value.container.timeStep, dto);
                });
                break;
        }
    }



    async Task ReadBinaryDto(BinaryDto obj, ulong timeStep, DistantEnvironmentDto dto)
    {
        //Log(obj);
        ByteContainer container = new ByteContainer(dto.id, timeStep, obj.data);
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
    }

    async void Log(BinaryDto data)
    {
        await Task.Yield();
        ByteContainer container = new ByteContainer(0, 0, data.data);
        uint TransactionId = UMI3DSerializer.Read<uint>(container);
        UnityEngine.Debug.Log( PerformTransaction(container));
    }

    public string PerformTransaction(ByteContainer container)
    {
        string s = "Transaction"+ System.Environment.NewLine;
        int i = 0;
        foreach (ByteContainer c in UMI3DSerializer.ReadIndexesList(container))
        {
            s += PerformOperation(c,i++);
        }
        return s;
    }

    public string PerformOperation(ByteContainer container, int i)
    {
        string s = $" - Operation {i}"+ System.Environment.NewLine +"    -> ";
        uint operationId = UMI3DSerializer.Read<uint>(container);
        switch (operationId)
        {
            case UMI3DOperationKeys.LoadEntity:
                s += ("Load entity");
                break;
            case UMI3DOperationKeys.DeleteEntity:
                {
                    ulong entityId = UMI3DSerializer.Read<ulong>(container);
                    s += ($"Load entity {entityId}");
                    break;
                }
            case UMI3DOperationKeys.MultiSetEntityProperty:
                s += ("Multi SetEntityProperty");
                break;
            case UMI3DOperationKeys.StartInterpolationProperty:
                s += ("StartInterpolationProperty");
                break;
            case UMI3DOperationKeys.StopInterpolationProperty:
                s += ("StopInterpolationProperty");
                break;

            default:
                if (UMI3DOperationKeys.SetEntityProperty <= operationId && operationId <= UMI3DOperationKeys.SetEntityMatrixProperty)
                {
                    ulong entityId = UMI3DSerializer.Read<ulong>(container);
                    uint propertyKey = UMI3DSerializer.Read<uint>(container);
                    s += ($"SetEntityProperty {operationId} {entityId} {propertyKey}");
                }
                else
                    s += ($"Other");
                break;
        }
        return s;
    }


}