using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;
using WebSocketSharp;

public class UMI3DDistantEnvironmentNode : UMI3DAbstractDistantEnvironmentNode
{
    protected DistantEnvironmentDto dto;
    [SerializeField]
    private string serverUrl;

    MediaDto media = null;
    UMI3DWorldControllerClient1 wcClient = null;
    UMI3DEnvironmentClient1 nvClient = null;

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

        dto = new DistantEnvironmentDto();

        if (!serverUrl.IsNullOrEmpty())
            Restart();
    }

    public override IEntity ToEntityDto(UMI3DUser user)
    {
        UnityEngine.Debug.Log("hello");
        return dto;
    }

    async void Restart()
    {
        await _Stop();
        await _Start();
    }

    async Task _Start()
    {
        UnityEngine.Debug.Log("start");
        media = new MediaDto()
        {
            name = "other server",
            url = ServerUrl
        };
        wcClient = new UMI3DWorldControllerClient1(media);
        if (await wcClient.Connect())
        {
            nvClient = await wcClient.ConnectToEnvironment();
            dto.environmentDto = nvClient.environement;
        }
    }

    async Task _Stop()
    {
        if (nvClient != null)
        {
            await nvClient.Logout();
            wcClient.Logout();
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