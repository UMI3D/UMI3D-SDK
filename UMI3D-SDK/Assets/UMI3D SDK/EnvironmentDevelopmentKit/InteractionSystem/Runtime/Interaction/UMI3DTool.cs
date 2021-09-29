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
using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.edk.interaction
{
    public class UMI3DTool : AbstractTool, UMI3DLoadableEntity, IBytable
    {
        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entity = this,
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
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

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return (ToDto(user) as ToolDto);
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractToolDto CreateDto()
        {
            return new ToolDto();
        }

        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user);
        }

        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            if (parameters.Length < 1)
                ToBytes(null);
            return ToBytes(parameters[0] as UMI3DUser);
        }
        bool IBytable.IsCountable()
        {
            return true;
        }

    }
}