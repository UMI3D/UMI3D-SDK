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
using System.Collections;
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.interaction
{
    public class Toolbox : GlobalTool
    {
        [SerializeField, EditorReadOnly]
        public List<GlobalTool> tools = new List<GlobalTool>();

        protected override void WriteProperties(AbstractToolDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            ToolboxDto tbDto = dto as ToolboxDto;
            tbDto.isInsideToolbox = parent != null;
            if (parent != null) tbDto.toolboxId = parent.Id();
            tbDto.tools = tools.ConvertAll(t => t.ToDto(user) as GlobalToolDto);
        }
        protected override AbstractToolDto CreateDto()
        {
            return new ToolboxDto();
        }

        protected virtual void Awake()
        {
            RecursivelySetParenthood();
        }

        public void RecursivelySetParenthood()
        {
            tools.ForEach(t =>
            {
                t.parent = this;
                if (t is Toolbox)
                    (t as Toolbox).RecursivelySetParenthood();
            });
        }
    }
}