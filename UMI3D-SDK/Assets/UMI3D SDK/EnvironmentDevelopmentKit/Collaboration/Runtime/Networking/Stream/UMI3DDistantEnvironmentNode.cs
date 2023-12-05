using inetum.unityUtils;
using PlasticGui.Configuration.CloudEdition.Welcome;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using umi3d.common.userCapture.tracking;
using BeardedManStudios.Forge.Networking.Frame;

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

    UMI3DAsyncProperty<object> lastTransactionAsync;

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

    private void Update()
    {
        if (SendTransaction)
        {
            SendTransaction = false;

            var bin = new BinaryDto
            {
                data = new byte[1] {0},
                groupId = 0
            };

            //if (bin.data == null || bin.data.Length <= 0)
            //    return;

            var op = lastTransactionAsync.SetValue(bin);
            var t = op.ToTransaction(true);
            t.Dispatch();
        }
    }

    protected override void InitDefinition(ulong id)
    {
        base.InitDefinition(id);

        lastTransactionAsync = new UMI3DAsyncProperty<object>(Id(), UMI3DPropertyKeys.DistantEnvironment, null);
        dto = new DistantEnvironmentDto();
        dto.id = id;
        UnityEngine.Debug.Log($"ENV {dto.environmentDto != null}");
        //if (!serverUrl.IsNullOrEmpty())
        //    Restart();
    }



    public void OnData(Binary data)
    {
        var bin = new BinaryDto
        {
            data = data.StreamData.byteArr,
            groupId = data.GroupId
        };

        if (bin.data == null || bin.data.Length <= 0)
            return;

        var op = lastTransactionAsync.SetValue(bin);
        var t = op.ToTransaction(true);
        t.Dispatch();
    }

    public void OnAvatarData(BeardedManStudios.Forge.Networking.NetworkingPlayer player, List<UserTrackingFrameDto> data)
    {
        data.ForEach(t => t.environmentId = Id());
        UMI3DCollaborationServer.ForgeServer.trackingRelay.SetFrame(player, data);    
    }

    public override IEntity ToEntityDto(UMI3DUser user)
    {
        UnityEngine.Debug.Log("hello");
        return dto;
    }


    public Task<LoadEntityDto> GetEntity(IEnumerable<ulong> ids)
    {
        return nvClient.GetEntity(0,ids.ToList());
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
        wcClient = new UMI3DWorldControllerClient1(media,this);
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