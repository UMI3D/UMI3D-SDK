using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;
using WebSocketSharp;
using umi3d.common.userCapture.tracking;
using BeardedManStudios.Forge.Networking.Frame;
using System.ComponentModel;
using System.Threading;
using umi3d.common.collaboration.dto;

public class UMI3DDistantEnvironmentNode : UMI3DAbstractDistantEnvironmentNode
{

    public bool SendTransaction;

    public string ResourceServerUrl { get; set; }
    [SerializeField]
    private string serverUrl;

    public MediaDto media { get; private set; } = null;
    UMI3DWorldControllerClient wcClient = null;
    UMI3DEnvironmentClient nvClient = null;

    UMI3DAsyncListProperty<BinaryDto> lastTransactionsAsync;
    UMI3DAsyncProperty<BinaryDto> lastTransactionAsync;
    UMI3DAsyncProperty<GlTFEnvironmentDto> environmentDto;
    UMI3DAsyncProperty<string> resourcesUrl;
    UMI3DAsyncProperty<bool> useDto;

    int refresh = 60000;
    int reconnection = 60000;
    bool run = false;

    int count = 0;

    async void Loop(CancellationTokenSource tokenSource)
    {
        var cancellationToken = tokenSource.Token;
        if (run) return;
        run = true;
        while (run && !cancellationToken.IsCancellationRequested)
        {
            if(!wcClient.IsConnected())
            {
                await Connect();
                //return;
                await UMI3DAsyncManager.Delay(reconnection);
                continue;
            }

            await UMI3DAsyncManager.Delay(refresh);

            if (!nvClient.IsConnected() || nvClient.environement == null)
                continue;
            var manager = UMI3DCollaborationServer.MumbleManager;
            if (nvClient.UserDto.answerDto.audioUseMumble && manager != null && manager.ip == nvClient.UserDto.answerDto.audioServerUrl)
            {
                manager.SwitchDefaultRoom(nvClient.UserDto.answerDto.audioChannel, UMI3DCollaborationServer.Collaboration.Users);
            }
            environmentDto.SetValue(nvClient.environement);
            lastTransactionAsync.SetValue(new());
        }
        tokenSource.Dispose();
    }

    /// <summary>
    /// Token set in conectionDto if needed.
    /// It's need to be set before setting the ServerUrl.
    /// </summary>
    public string token;

    /// <summary>
    /// metaData set in conectionDto if needed.
    /// It's need to be set before setting the ServerUrl.
    /// </summary>
    public byte[] metaData;

    public string ServerUrl
    {
        get => serverUrl; set
        {
            serverUrl = value;
            if(UMI3DCollaborationServer.Exists && UMI3DCollaborationServer.Instance.isRunning)
                Restart();
        }
    }

    private void Start()
    {
        UMI3DCollaborationServer.Instance.OnServerStart.AddListener(Restart);
        UMI3DCollaborationServer.Instance.OnServerStop.AddListener(async () => await _Stop());
    }

    protected override void InitDefinition(ulong id)
    {
        base.InitDefinition(id);

        lastTransactionsAsync = new UMI3DAsyncListProperty<BinaryDto>(Id(), UMI3DPropertyKeys.DistantEnvironmentReliable, new());
        lastTransactionAsync = new UMI3DAsyncProperty<BinaryDto>(Id(), UMI3DPropertyKeys.DistantEnvironmentUnreliable, null);
        environmentDto = new UMI3DAsyncProperty<GlTFEnvironmentDto>(Id(), 0, null);
        resourcesUrl = new UMI3DAsyncProperty<string>(Id(), UMI3DPropertyKeys.DistantEnvironmentResourceUrl, null);
        useDto = new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.DistantEnvironmentUseDto, false);
    }

    public void OnData(Binary data)
    {
        var bin = new BinaryDto
        {
            data = data.StreamData.byteArr,
            groupId = data.GroupId,
            timestep = data.TimeStep
        };

        if (bin.data == null || bin.data.Length <= 0)
            return;
        if (data.IsReliable)
            Log(data);

        var op = (data.IsReliable) ? lastTransactionsAsync.Add(bin) : lastTransactionAsync.SetValue(bin);
        var t = op.ToTransaction(data.IsReliable);
        t.Dispatch();
    }

    async void Log(Binary data)
    {
        await Task.Yield();
        ByteContainer container = new ByteContainer(0, 0, data.StreamData.byteArr);
        uint TransactionId = UMI3DSerializer.Read<uint>(container);
        UnityEngine.Debug.Log(PerformTransaction(container));
    }

    public string PerformTransaction(ByteContainer container)
    {
        string s = "Transaction" + System.Environment.NewLine;
        int i = 0;
        foreach (ByteContainer c in UMI3DSerializer.ReadIndexesList(container))
        {
            s += PerformOperation(c, i++);
        }
        return s;
    }

    public string PerformOperation(ByteContainer container, int i)
    {
        string s = $" - Operation {i}" + System.Environment.NewLine + "    -> ";
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

    public void OnAvatarData(BeardedManStudios.Forge.Networking.NetworkingPlayer player, List<UserTrackingFrameDto> data)
    {
        data.ForEach(t => t.environmentId = Id());
        UMI3DCollaborationServer.ForgeServer.trackingRelay.SetFrame(player, data);
    }

    public override IEntity ToEntityDto(UMI3DUser user)
    {
        var nDto = new DistantEnvironmentDto()
        {
            id = Id(),
            environmentDto = environmentDto.GetValue(user),
            resourcesUrl = resourcesUrl.GetValue(user),
            binaries = lastTransactionsAsync.GetValue(user).ToList()
        };

        return nDto;
    }


    public Task<LoadEntityDto> GetEntity(IEnumerable<ulong> ids)
    {
        return nvClient.GetEntity(0, ids.ToList());
    }

    async void Restart()
    {
        await _Stop();
        await _Start();

        UMI3DCollaborationServer.Instance.OnUserCreated.AddListener(OnUserCreated);
    }

    CancellationTokenSource tokenSource;
    Task _Start()
    {
        Id();
        UnityEngine.Debug.Log("start");
        media = new MediaDto()
        {
            name = "other server",
            url = ServerUrl
        };
        wcClient = new UMI3DWorldControllerClient(media, this);

        tokenSource = new CancellationTokenSource();
        Loop(tokenSource);
        return Task.CompletedTask;
    }

    async Task<bool> Connect()
    {
        if (await wcClient.Connect())
        {
            UnityEngine.Debug.Log($"Distant Connection to WC : OK");
            nvClient = await wcClient.ConnectToEnvironment();

            while (!nvClient.IsConnected() || nvClient.environement == null)
                await Task.Yield();

            UnityEngine.Debug.Log($"Distant Connection to ENV : OK");

            environmentDto.SetValue(nvClient.environement);
            //if (dto.environmentDto?.scenes != null)
            //    dto.environmentDto.scenes.SelectMany(s => s.nodes).Debug();
            ResourceServerUrl = nvClient.connectionDto.resourcesUrl;
            resourcesUrl.SetValue(ResourceServerUrl);
            useDto.SetValue(nvClient.useDto);
            
            GetLoadEntity().ToTransaction(true).Dispatch();

            UnityEngine.Debug.Log($"Distant Connection : END");

            return true;
        }
        return false;
    }

    async Task _Stop()
    {
        UMI3DCollaborationServer.Instance.OnUserCreated.RemoveListener(OnUserCreated);
        if (nvClient != null)
        {
            await nvClient.Logout();
            wcClient.Logout();
        }
        run = false;
        if (tokenSource is not null)
        {
            tokenSource.Cancel();
            tokenSource = null;
        }
    }

    async void OnUserCreated(UMI3DUser user)
    {
        if (user is UMI3DCollaborationUser cuser)
        {
            while (run && nvClient == null)
                await Task.Yield();
            if (!run || user is null || cuser.status != StatusType.NONE)
                return;

            var dto = cuser.identityDto;
            await nvClient.HttpClient.SendPostRegisterDistantUser(dto);
        }
    }
}

public abstract class UMI3DAbstractDistantEnvironmentNode : MonoBehaviour, UMI3DLoadableEntity
{

    /// <summary>
    /// Indicates if InitDefinition has been called.
    /// </summary>
    protected bool inited = false;

    #region filter
    private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

    public bool LoadOnConnection(UMI3DUser user)
    {
        return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
    }

    public bool AddConnectionFilter(UMI3DUserFilter filter)
    {
        return ConnectionFilters.Add(filter);
    }

    public bool RemoveConnectionFilter(UMI3DUserFilter filter)
    {
        return ConnectionFilters.Remove(filter);
    }

    #endregion

    public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
    {
        var operation = new DeleteEntity()
        {
            entityId = Id(),
            users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
        };
        return operation;
    }

    public LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
    {
        var operation = new LoadEntity()
        {
            entities = new List<UMI3DLoadableEntity>() { this },
            users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
        };
        return operation;
    }

    /// <summary>
    /// The objects's unique id. 
    /// </summary>
    protected ulong objectId;

    /// <summary>
    /// The public Getter for objectId.
    /// </summary>
    public ulong Id()
    {
        Register();
        return objectId;
    }

    /// <summary>
    /// Check if the AbstractObject3D has been registered to to the UMI3DScene and do it if not
    /// </summary>
    public virtual void Register()
    {
        if (objectId == 0 && UMI3DEnvironment.Exists)
        {
            objectId = UMI3DEnvironment.Register(this);
            UnityEngine.Debug.Log(objectId);
            InitDefinition(objectId);
            inited = true;
        }
    }

    /// <summary>
    /// Initialize object's properties.
    /// </summary>
    protected virtual void InitDefinition(ulong id)
    {
        BeardedManStudios.Forge.Networking.Unity.MainThreadManager.Run(() =>
        {
            if (this != null)
            {
                foreach (UMI3DUserFilter f in GetComponents<UMI3DUserFilter>())
                    AddConnectionFilter(f);
            }
        });
        objectId = id;
    }



    public virtual Bytable ToBytes(UMI3DUser user)
    {
        return UMI3DSerializer.Write(Id());
    }

    public abstract IEntity ToEntityDto(UMI3DUser user);
}