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
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.edk.volume
{
    /// <summary>
    /// Volume cell represented by an .obj
    /// </summary>
    public class OBJVolume : MonoBehaviour, IVolume
    {
        /// <summary>
        /// Event raised when a user enter the cell.
        /// </summary>
        [SerializeField] private UMI3DUserEvent onUserEnter = new UMI3DUserEvent();

        /// <summary>
        /// Event raised when a user exit the cell.
        /// </summary>
        [SerializeField] private UMI3DUserEvent onUserExit = new UMI3DUserEvent();

        [SerializeField] private string fileURL = null;

        ///<inheritdoc/>
        public UMI3DUserEvent GetUserEnter() => onUserEnter;

        ///<inheritdoc/>
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
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
            };
            return operation;
        }


        private ulong? id = null;
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

        public virtual IEntity ToEntityDto(UMI3DUser user)
        {
            //Taken from UMI3DResourceFile.GetUrl()
            string path = fileURL;
            path = path.Replace(@"\", "/");
            if (path != null && path != "" && !(path.StartsWith("/") /*|| Path.StartsWith(@"\")*/))
            {
                path = "/" + path;
            }
            path = System.Uri.EscapeUriString(Path.Combine(UMI3DServer.GetHttpUrl(), UMI3DNetworkingKeys.files, path));

            UMI3DScene scene = this.GetComponentInParent<UMI3DScene>();


            OBJVolumeDto dto = new OBJVolumeDto()
            {
                id = Id(),
                objFile = path,
                rootNodeId = scene.Id(),
                rootNodeToLocalMatrix = scene.transform.localToWorldMatrix * this.transform.worldToLocalMatrix,
                isTraversable = IsTraversable()
            };
            return dto;
        }

        public Bytable ToBytes(UMI3DUser user)
        {
            throw new System.NotImplementedException();
        }

        [SerializeField]
        private bool isTraversable = true;
        public bool IsTraversable() => isTraversable;

    }
}