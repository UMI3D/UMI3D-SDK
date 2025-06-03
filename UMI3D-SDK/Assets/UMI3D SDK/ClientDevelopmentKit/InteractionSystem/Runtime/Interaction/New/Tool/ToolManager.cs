/*
Copyright 2019 - 2025 Inetum

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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public sealed class ToolManager 
    {
        #region Initialize

        static Lazy<ToolManager> _default = new(() => new());
        public static ToolManager @default => _default.Value;

        ToolManager()
        {

        }

        #endregion

        List<Tool> _tools = new();
        public ReadOnlyCollection<Tool> tools => _tools.AsReadOnly();

        /// <summary>
        /// Searches for an existing tool based on the specified environment ID and DTO (Data Transfer Object). 
        /// If a tool matching the criteria exists, it is returned. Otherwise, a new tool is created, 
        /// added to the internal list, registered in the environment loader, and returned.<br/>
        /// <br/>
        /// <example>
        /// Given an environment ID and a DTO, when calling this method, it will either return an existing tool 
        /// or create a new one and register it in the environment loader.<br/>
        /// <br/>
        /// <code>
        /// bool isNew = toolManager.InstantiateOrGet(out Tool tool, 12345UL, dto);
        /// if (isNew)
        /// {
        ///     Console.WriteLine("A new tool was created and registered.");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("An existing tool was returned.");
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="tool">
        /// The output parameter that will hold the existing or newly created tool.
        /// </param>
        /// <param name="environmentId">
        /// The unique identifier of the environment associated with the tool.
        /// </param>
        /// <param name="dto">
        /// The data transfer object (DTO) containing the tool's details.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether a new tool was created (`true`) or an existing tool was found (`false`).
        /// </returns>
        public bool InstantiateOrGet(out Tool tool, ulong environmentId, AbstractToolDto dto)
        {
            tool = _tools.Find(tool => tool.environmentId == environmentId && tool.dto.id == dto.id);
            if (tool != null)
            {
                UnityEngine.Debug.Log($"[ToolManager] Notice: tool for environment id: '{environmentId}' and dto's id '{dto.id}' already exist.");
                return false;
            }

            UnityEngine.Debug.Log($"[ToolManager] Notice: tool for environment id: '{environmentId}' and dto's id '{dto.id}' created.");
            tool = new(environmentId, dto);
            _tools.Add(tool);
            UMI3DEnvironmentLoader.Instance.RegisterEntity(environmentId, dto.id, dto, tool).NotifyLoaded();
            return true;
        }

        public bool TryToRemoveTool(Tool tool)
        {
            if (!_tools.Contains(tool))
            {
                return false;
            }

            _ = UMI3DEnvironmentLoader.Instance.DeleteEntityInstance(tool.environmentId, tool.dto.id);
            _tools.Remove(tool);
            return true;
        }
        public bool TryToRemoveTool(ulong environmentId, ulong toolId)
        {
            Tool tool = _tools.Find(tool => tool.environmentId == environmentId && tool.dto.id == toolId);
            if (tool == null)
            {
                return false;
            }

            _ = UMI3DEnvironmentLoader.Instance.DeleteEntityInstance(environmentId, toolId);
            _tools.Remove(tool);
            return true;
        }

        public bool TryToFetchTool(out Tool tool, ulong environmentId, ulong dtoId)
        {
            tool = _tools.Find(tool => tool.environmentId == environmentId && tool.dto.id == dtoId);
            if (tool == null)
            {
                UnityEngine.Debug.LogWarning($"[ToolManager] Warning: Cannot find tool for '{environmentId}' and dto's id '{dtoId}'.");
                return false;
            }

            return true;
        }
    }
}