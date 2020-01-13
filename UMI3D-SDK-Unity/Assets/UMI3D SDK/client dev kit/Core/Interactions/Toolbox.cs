/*
Copyright 2019 Gfi Informatique

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

namespace umi3d.cdk
{

    public class Toolbox
    {
        public static Dictionary<string, Toolbox> instances = new Dictionary<string, Toolbox>();

        /// <summary>
        /// Toolbox Id.
        /// </summary>
        public string id;

        /// <summary>
        /// Toolbox name.
        /// </summary>
        public string name = null;

        /// <summary>
        /// Toolbox description.
        /// </summary>
        public string description = null;

        /// <summary>
        /// 2D icon.
        /// </summary>
        public ResourceDto icon2D = null;

        /// <summary>
        /// 3D icon.
        /// </summary>
        public ResourceDto icon3D = null;

        /// <summary>
        /// Contained tools.
        /// </summary>
        public List<string> tools = new List<string>();

        /// <summary>
        /// Contruct from dto.
        /// </summary>
        /// <param name="dto">Dto to construct from.</param>
        public void UpdateFromDto(ToolboxDto dto)
        {
            id = dto.Id;
            name = dto.name;
            description = dto.description;
            icon2D = dto.icon2D;
            icon3D = dto.icon3D;
            tools = dto.tools.ConvertAll(tool => tool.Id);
        }


        public Toolbox(ToolboxDto dto)
        {
            UpdateFromDto(dto);
            instances.Add(id, this);
        }

        ~Toolbox()
        {
            instances.Remove(id);
        }

    }

}