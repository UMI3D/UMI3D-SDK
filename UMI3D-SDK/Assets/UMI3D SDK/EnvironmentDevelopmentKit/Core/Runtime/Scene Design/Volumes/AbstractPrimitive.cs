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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk.volume
{
    /// <summary>
    /// Base class for volume primitive.
    /// </summary>
    public abstract class AbstractPrimitive : MonoBehaviour, IVolume
    {
        /// <summary>
        /// Triggered when a user enters the volume.
        /// </summary>
        [Tooltip("Triggered when a user enters the volume.")]
        public UMI3DUserEvent onUserEnter = new UMI3DUserEvent();
        /// <summary>
        /// Triggered when a user exits the volume.
        /// </summary>
        [Tooltip("Triggered when a user exits the volume.")]
        public UMI3DUserEvent onUserExit = new UMI3DUserEvent();

        /// <inheritdoc/>
        public UMI3DUserEvent GetUserEnter()
        {
            return onUserEnter;
        }

        /// <inheritdoc/>
        public UMI3DUserEvent GetUserExit()
        {
            return onUserExit;
        }

        /// <summary>
        /// Return the root node to position the primitive in relation to.
        /// </summary>
        /// <returns></returns>
        public UMI3DAbstractNode GetRootNode()
        {
            return this.gameObject.GetComponentInParent<UMI3DAbstractNode>();
        }

        /// <summary>
        /// Return the root node to local position matrix
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetRootNodeToLocalMatrix()
        {
            return GetRootNode().transform.localToWorldMatrix * this.transform.worldToLocalMatrix;
        }

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
                entities = new List<UMI3DLoadableEntity>() { this },
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
            };
            return operation;
        }

        /// <summary>
        /// UMI3D id.
        /// </summary>
        private ulong? id = null;
        /// <inheritdoc/>
        public ulong Id()
        {
            if (id == null)
            {
                id = UMI3DEnvironment.Register(this);
                VolumeManager.volumes.Add(Id(), this);
            }
            return id.Value;
        }

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

        /// <inheritdoc/>
        public abstract IEntity ToEntityDto(UMI3DUser user);

        protected virtual void Awake()
        {
        }

        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// If true, a user can enter the volume.
        /// </summary>
        [SerializeField, Tooltip("If true, a user can enter the volume.")]
        private bool isTraversable = true;
        public bool IsTraversable()
        {
            return isTraversable;
        }
    }
}