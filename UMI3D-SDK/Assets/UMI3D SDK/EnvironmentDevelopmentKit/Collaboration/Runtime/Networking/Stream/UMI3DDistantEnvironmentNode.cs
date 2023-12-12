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
    protected DistantEnvironmentDto dto;
    [SerializeField]
    private string serverUrl;

    public MediaDto media { get; private set; } = null;
    UMI3DWorldControllerClient1 wcClient = null;
    UMI3DEnvironmentClient1 nvClient = null;

    UMI3DAsyncListProperty<BinaryDto> lastTransactionsAsync;
    UMI3DAsyncProperty<BinaryDto> lastTransactionAsync;

    int refresh = 60000;
    bool run = false;

    async void Loop()
    {
        if (run) return;
        //run = true;

        while (run)
        {
            await UMI3DAsyncManager.Delay(refresh);

            if (!nvClient.IsConnected() || nvClient.environement == null)
                continue;

            dto.environmentDto = nvClient.environement;
            lastTransactionAsync.SetValue(new());
        }
    }

    public string ServerUrl
    {
        get => serverUrl; set
        {
            serverUrl = value;
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
        lastTransactionAsync = new UMI3DAsyncProperty<BinaryDto>(Id(), UMI3DPropertyKeys.DistantEnvironmentUnreliable, new());
        dto = new DistantEnvironmentDto();
        dto.id = id;
        //if (!serverUrl.IsNullOrEmpty())
        //    Restart();
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
        //foreach(var tk in data)
        //if (dto.environmentDto.extensions.umi3d is UMI3DCollaborationEnvironmentDto _dto && !_dto.userList.Any(u => u.id == tk.userId))
        //{
        //    _dto.userList = 

        //}
    }

    public override IEntity ToEntityDto(UMI3DUser user)
    {
        var nDto = new DistantEnvironmentDto()
        {
            id = dto.id,
            environmentDto = dto.environmentDto,
            resourcesUrl = dto.resourcesUrl,
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

    async Task _Start()
    {
        Id();
        UnityEngine.Debug.Log("start");
        media = new MediaDto()
        {
            name = "other server",
            url = ServerUrl
        };
        wcClient = new UMI3DWorldControllerClient1(media, this);
        if (await wcClient.Connect())
        {
            nvClient = await wcClient.ConnectToEnvironment();

            while (!nvClient.IsConnected() || nvClient.environement == null)
                await Task.Yield();

            UnityEngine.Debug.Log($"{nvClient != null} {dto != null}");
            dto.environmentDto = nvClient.environement;
            UnityEngine.Debug.Log($"ENV {dto.environmentDto != null}");
            //if (dto.environmentDto?.scenes != null)
            //    dto.environmentDto.scenes.SelectMany(s => s.nodes).Debug();
            ResourceServerUrl = nvClient.connectionDto.resourcesUrl;
            dto.resourcesUrl = ResourceServerUrl;
            dto.useDto = nvClient.useDto;

            Loop();
        }
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
    }

    async void OnUserCreated(UMI3DUser user)
    {
        if (user is UMI3DCollaborationUser cuser)
        {
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