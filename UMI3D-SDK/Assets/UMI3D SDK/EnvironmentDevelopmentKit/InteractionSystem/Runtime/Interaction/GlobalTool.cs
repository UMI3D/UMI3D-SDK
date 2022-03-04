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
    public class GlobalTool : AbstractTool, UMI3DLoadableEntity
    {
        /// <summary>
        /// Is this tool inside a <see cref="Toolbox"/> ?
        /// </summary>
        public bool isInsideToolbox { get => parent != null; }

        /// <summary>
        /// <see cref="Toolbox"/> parent if this tool is inside a toolbox.
        /// </summary>
        private Toolbox _parent;

        /// <summary>
        /// Getter/setter for <see cref="_parent"/>.
        /// </summary>
        public Toolbox parent
        {
            get => _parent;
            set
            {
                if (value == this)
                    UMI3DLogger.LogError("Impossible to set itself as a parent.", DebugScope.Interaction);
                else
                    _parent = value;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public virtual DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
            };
            return operation;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual IEntity ToEntityDto(UMI3DUser user) => ToDto(user) as GlobalToolDto;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        protected override AbstractToolDto CreateDto()
        {
            return new GlobalToolDto();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="user"></param>
        protected override void WriteProperties(AbstractToolDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            GlobalToolDto gtDto = dto as GlobalToolDto;
            gtDto.isInsideToolbox = this.isInsideToolbox;
            gtDto.toolboxId = isInsideToolbox ? parent.Id() : 0;
        }
    }
}