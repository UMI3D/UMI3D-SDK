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
using umi3d.cdk.menu.interaction;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Client's side interactable object.
    /// </summary>
    /// <see cref="InteractableDto"/>
    public class Toolbox
    {
        public static List<Toolbox> Toolboxes() { return UMI3DEnvironmentLoader.Entities().Where(e => e?.Object is Toolbox).Select(e => e?.Object as Toolbox).ToList(); }
        public static ToolboxSubMenu IdToMenu(string id) { return (UMI3DEnvironmentLoader.GetEntity(id)?.Object as Toolbox).sub; }

        /// <summary>
        /// Interactable dto describing this object.
        /// </summary>
        public ToolboxDto dto;
        public ToolboxSubMenu sub;
        public List<Tool> tools;

        public Toolbox(ToolboxDto dto)
        {
            tools = new List<Tool>();

            this.dto = dto;
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, this, Destroy);
            sub = new ToolboxSubMenu()
            {
                Name = dto.name,
                toolbox = this
            };
        }

        public void Destroy()
        {
            foreach (Tool t in tools)
            {
                UMI3DEnvironmentLoader.DeleteEntity(t.dto.id, null);
            }
        }

    }
}