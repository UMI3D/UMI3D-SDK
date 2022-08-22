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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// <see cref="GlobalTool"/> that contains a list of <see cref="GlobalTool"/>.
    /// </summary>
    public class Toolbox : GlobalTool, UMI3DLoadableEntity
    {
        /// <summary>
        /// Tools in the toolbox.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Tools in the toolbox.")]
        private List<GlobalTool> tools = new List<GlobalTool>();

#if UNITY_EDITOR
        /// <summary>
        /// getter setter for tool field in editor;
        /// </summary>
        public List<GlobalTool> editorTools { get => tools; set => tools = value; }
#endif

        public UMI3DAsyncListProperty<GlobalTool> objectTools { get { Register(); return _objectTools; } protected set => _objectTools = value; }

        private UMI3DAsyncListProperty<GlobalTool> _objectTools;

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
            objectTools = new UMI3DAsyncListProperty<GlobalTool>(toolId, UMI3DPropertyKeys.ToolboxTools, tools, (i, u) => UMI3DEnvironment.Instance.useDto ? i.ToDto(u) : (object)i);
            objectTools.OnUserValueChanged += (u, l) => { tools = l; };
        }

        /// <inheritdoc/>
        protected override void WriteProperties(AbstractToolDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var tbDto = dto as ToolboxDto;
            tbDto.tools = objectTools.GetValue(user).ConvertAll(t => t.ToDto(user) as GlobalToolDto);
        }

        /// <inheritdoc/>
        protected override AbstractToolDto CreateDto()
        {
            return new ToolboxDto();
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
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
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
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user) as ToolboxDto;
        }
    }
}