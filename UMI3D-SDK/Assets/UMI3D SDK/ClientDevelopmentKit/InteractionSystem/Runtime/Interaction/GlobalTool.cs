﻿/*
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
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    public class GlobalToolEvent : UnityEvent<GlobalTool> { }

    /// <summary>
    /// Direct instanciation of <see cref="AbstractTool"/>.
    /// </summary>
    public class GlobalTool : AbstractTool
    {
        /// <summary>
        /// Global tools instances
        /// </summary>
        protected static Dictionary<ulong, GlobalTool> instances = new Dictionary<ulong, GlobalTool>();

        /// <summary>
        /// Retrieve all the <see cref="GlobalTool"/> instances.
        /// </summary>
        /// <returns></returns>
        public static List<GlobalTool> GetGlobalTools()
        {
            return instances.Values.ToList();
        }

        /// <summary>
        /// Get a global tool by its id in <see cref="instances"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static GlobalTool GetGlobalTool(ulong id)
        {
            return instances[id];
        }

        /// <summary>
        /// Delete tool.
        /// </summary>
        public void Delete() { instances.Remove(id); }

        /// <summary>
        /// DTO associated to the tool.
        /// </summary>
        public AbstractToolDto dto { get; protected set; }

        /// <summary>
        /// Toolbox the global tool belongs to if there is one.
        /// </summary>
        public Toolbox parent;

        /// <summary>
        /// Is the global tool in a toolbox ?
        /// </summary>
        public bool isInsideToolbox => parent != null;

        public GlobalTool(AbstractToolDto abstractDto, Toolbox parent) : base(abstractDto)
        {
            this.parent = parent;
            instances.Add(id, this);
        }

        /// <inheritdoc/>
        protected override AbstractToolDto abstractDto { get => dto; set => dto = value as GlobalToolDto; }
    }
}