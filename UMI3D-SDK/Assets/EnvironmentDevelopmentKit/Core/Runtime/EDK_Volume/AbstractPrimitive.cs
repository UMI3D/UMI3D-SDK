using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk.volume
{
    public abstract class AbstractPrimitive : MonoBehaviour, IVolume
    {
        public UMI3DUserEvent onUserEnter = new UMI3DUserEvent();
        public UMI3DUserEvent onUserExit = new UMI3DUserEvent();
        public UMI3DUserEvent GetUserEnter() => onUserEnter;
        public UMI3DUserEvent GetUserExit() => onUserExit;



        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entity = this,
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
            };
            return operation;
        }
       
        
        private string id = "";
        public string Id()
        {
            if (id.Equals(""))
                id = Random.Range(0, 10000000000000000).ToString();
            return id;
        }

        #region filter
        HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

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

        public abstract IEntity ToEntityDto(UMI3DUser user);
    }
}