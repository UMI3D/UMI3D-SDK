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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{

    public class EntityGroup : UMI3DLoadableEntity
    {
        private ulong entityId;
        [SerializeField, EditorReadOnly]
        private List<UMI3DMediaEntity> _entities = new List<UMI3DMediaEntity>();
        public UMI3DAsyncListProperty<UMI3DMediaEntity> entities { get { Id(); return _entitiesObject; } private set => _entitiesObject = value; }

        private UMI3DAsyncListProperty<UMI3DMediaEntity> _entitiesObject;


        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
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


        public ulong Id()
        {
            if (entityId == 0)
            {
                entityId = UMI3DEnvironment.Register(this);
                InitDefinition();
            }

            return entityId;
        }

        private void InitDefinition()
        {
            entities = new UMI3DAsyncListProperty<UMI3DMediaEntity>(entityId, UMI3DPropertyKeys.EntityGroupIds, _entities, (e, u) => e.Id());
        }


        public IEntity ToEntityDto(UMI3DUser user)
        {
            return new EntityGroupDto() { id = Id(), entitiesId = entities.GetValue(user).Select(e => e.Id()).ToList() };
        }

        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.SetEntityProperty)
                + UMI3DNetworkingHelper.Write(entityId)
                + UMI3DNetworkingHelper.WriteCollection(entities.GetValue(user).Select(e => e.Id()).ToList());
        }

        public void Destroy()
        {
            UMI3DEnvironment.Remove(this);
            entityId = 0;
        }


        #region filter
        private HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

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
    }
}