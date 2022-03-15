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
using System.Linq;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    public class GlobalToolEvent : UnityEvent<GlobalTool> { }

    public class GlobalTool : AbstractTool
    {
        protected static Dictionary<ulong, GlobalTool> instances = new Dictionary<ulong, GlobalTool>();

        public static List<GlobalTool> GetGlobalTools() => instances.Values.ToList();
        public static GlobalTool GetGlobalTool(ulong id) => instances[id];

        public void Delete() { instances.Remove(id); }

        public AbstractToolDto dto { get; protected set; }

        public Toolbox parent;

        public bool isInsideToolbox => parent != null;

        public GlobalTool(AbstractToolDto abstractDto,Toolbox parent) : base(abstractDto) 
        {
            this.parent = parent;
            instances.Add(id, this);
        }

        protected override AbstractToolDto abstractDto { get => dto; set => dto = value as GlobalToolDto; }
    }
}