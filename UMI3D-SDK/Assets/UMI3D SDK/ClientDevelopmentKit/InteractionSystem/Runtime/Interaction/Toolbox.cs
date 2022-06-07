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
using System.Collections;
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine;
using System.Linq;

namespace umi3d.cdk.interaction
{
    public class Toolbox : GlobalTool
    {
        public static List<Toolbox> GetToolboxes() => instances.Values.ToList().Where(tool => tool is Toolbox).ToList().ConvertAll(t => t as Toolbox);
        public static Toolbox GetToolbox(ulong id) => instances[id] as Toolbox;

        public Toolbox(AbstractToolDto abstractDto, Toolbox parent) : base(abstractDto, parent) { }

        public List<GlobalToolDto> tools { get => (abstractDto as ToolboxDto).tools; set => (abstractDto as ToolboxDto).tools = value; }

        protected override AbstractToolDto abstractDto { get => dto; set => dto = value as ToolboxDto; }

    }
}