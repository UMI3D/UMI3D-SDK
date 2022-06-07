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

namespace umi3d.edk
{
    //[CreateAssetMenu(fileName = "Umi3DMaterial", menuName = "UMI3D/Umi3DMaterial")]
    public abstract class MaterialSO : ScriptableObject, UMI3DLoadableEntity
    {


        public Dictionary<string, object> shaderProperties = new Dictionary<string, object>();
        public UMI3DAsyncDictionnaryProperty<string, object> objectShaderProperties { get { Id(); return _objectShaderProperties; } protected set => _objectShaderProperties = value; }

        private UMI3DAsyncDictionnaryProperty<string, object> _objectShaderProperties;

        public enum AlphaMode
        {
            OPAQUE, MASK, BLEND
        }
        public AlphaMode alphaMode = AlphaMode.BLEND;

        protected abstract void OnEnable();

        protected abstract ulong GetId();

        protected abstract void SetId(ulong id);

        public abstract GlTFMaterialDto ToDto();


        public abstract Bytable ToBytes(UMI3DUser user);

        public ulong Id()
        {
            return GetId();
        }
        protected abstract void InitDefinition(ulong id);

        protected void RegisterMaterial(AbstractEntityDto mat)
        {
            if (mat.id != 0 || UMI3DEnvironment.GetEntity<MaterialSO>(mat.id) == null)
            {
                mat.id = UMI3DEnvironment.Register(this);
                SetId(mat.id);
                InitDefinition(mat.id);
            }
        }

        public abstract IEntity ToEntityDto(UMI3DUser user);

        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
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
    }
}
