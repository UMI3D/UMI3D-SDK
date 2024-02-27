/*
Copyright 2019 - 2024 Inetum

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

using inetum.unityUtils.saveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public abstract class AbstractToolDelegate : SerializableScriptableObject
    {
        /// <summary>
        /// The currently projected tool.
        /// </summary>
        public virtual AbstractTool Tool {  get; set; }

        /// <summary>
        /// The currently hovered tool.
        /// </summary>
        public virtual AbstractTool CurrentHoverTool { get; set; }

        /// <summary>
        /// The currently hovered tool.
        /// </summary>
        public virtual List<AbstractTool> ProjectedTools { get; set; }

        /// <summary>
        /// Whether or not <paramref name="tool"/> can be projected on this controller.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public abstract bool IsCompatibleWith(AbstractTool tool);

        /// <summary>
        /// Whether or not <paramref name="tool"/> is currently projected on this controller.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        /// <see cref="IsCompatibleWith(AbstractToolDto)"/>
        public virtual bool IsAvailableFor(AbstractTool tool)
        {
            if (!IsCompatibleWith(tool))
            {
                return false;
            }

            return Tool == null;
        }

        /// <summary>
        /// Whether or not <paramref name="tool"/> requires the generation of a menu to be projected.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public abstract bool RequiresMenu(AbstractTool tool);

        /// <summary>
        /// Create a menu to access each interactions of a tool separately.
        /// </summary>
        /// <param name="interactions"></param>
        public abstract void CreateInteractionsMenuFor(AbstractTool tool);
    }
}