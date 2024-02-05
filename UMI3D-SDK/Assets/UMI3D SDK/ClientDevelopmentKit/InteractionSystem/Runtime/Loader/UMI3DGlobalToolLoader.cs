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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Helper class that manages the loading of <see cref="GlobalTool"/> entities.
    /// </summary>
    public class UMI3DGlobalToolLoader : UMI3DAbstractToolLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Interaction | DebugScope.Loading;

        #region CRUD events
        private static readonly GlobalToolEvent onGlobalToolCreation = new GlobalToolEvent();
        private static readonly GlobalToolEvent onGlobalToolUpdate = new GlobalToolEvent();
        private static readonly GlobalToolEvent onGlobalToolDelete = new GlobalToolEvent();

        public static void SubscribeToGlobalToolCreation(UnityAction<GlobalTool> callback)
        {
            onGlobalToolCreation.AddListener(callback);
        }
        public static void UnsubscribeToGlobalToolCreation(UnityAction<GlobalTool> callback)
        {
            onGlobalToolCreation.RemoveListener(callback);
        }
        public static void SubscribeToGlobalToolUpdate(UnityAction<GlobalTool> callback)
        {
            onGlobalToolUpdate.AddListener(callback);
        }
        public static void UnsubscribeToGlobalToolUpdate(UnityAction<GlobalTool> callback)
        {
            onGlobalToolUpdate.RemoveListener(callback);
        }
        public static void SubscribeToGlobalToolDelete(UnityAction<GlobalTool> callback)
        {
            onGlobalToolDelete.AddListener(callback);
        }
        public static void UnsubscribeToGlobalToolDelete(UnityAction<GlobalTool> callback)
        {
            onGlobalToolDelete.RemoveListener(callback);
        }

        #endregion

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is GlobalToolDto;
        }

        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            return ReadUMI3DExtension(value.dto as GlobalToolDto, null);
        }

        /// <summary>
        /// Reads the value of an <see cref="GlobalToolDto"/> and updates it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="dto">Tool dto</param>
        private static async Task ReadUMI3DExtension(GlobalToolDto dto, Toolbox parent)
        {
            if (GlobalTool.GetGlobalTools().Exists(t => t.id == dto.id))
                return;

            if (dto is ToolboxDto toolbox)
            {
                var tool = new Toolbox(dto, parent);
                var subTools = new Stack<GlobalToolDto>(toolbox.tools);
                onGlobalToolCreation?.Invoke(tool);
                while (subTools.Count > 0)
                    await ReadUMI3DExtension(subTools.Pop(), tool);
            }
            else
                onGlobalToolCreation?.Invoke(new GlobalTool(dto, parent));
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            var dto = data.entity?.dto as GlobalToolDto;

            if (dto == null)
                return false;

            if (await base.SetUMI3DProperty(data))
            {
                onGlobalToolUpdate.Invoke(GlobalTool.GetGlobalTool(dto.id));
                return true;
            }

            switch (data.property.property)
            {
                case UMI3DPropertyKeys.ToolboxTools:
                    var tb = Toolbox.GetToolbox(dto.id);
                    List<GlobalToolDto> list = tb.tools;
                    switch (data.property)
                    {
                        case SetEntityListAddPropertyDto add:
                            int ind = add.index;
                            var value = add.value as GlobalToolDto;
                            if (ind == list.Count)
                                list.Add(value);
                            else if (ind < list.Count && ind >= 0)
                                list.Insert(ind, value);
                            else
                            {
                                UMI3DLogger.LogWarning($"Add value ignore for {ind} in collection of size {list.Count}", scope);
                                return false;
                            }
                            await ReadUMI3DExtension(value, null);
                            break;
                        case SetEntityListRemovePropertyDto rem:
                            int i = rem.index;
                            RemoveTool(tb.tools[i]);
                            list.RemoveAt(i);
                            break;
                        case SetEntityListPropertyDto set:
                            int index = set.index;
                            var v = set.value as GlobalToolDto;
                            list[index] = v;
                            break;
                        default:
                            foreach (GlobalToolDto t in list)
                                RemoveTool(t);
                            list.Clear();
                            list.AddRange(data.property.value as List<GlobalToolDto>);
                            foreach (GlobalToolDto t in list)
                                await ReadUMI3DExtension(t, null);
                            break;
                    }
                    onGlobalToolUpdate?.Invoke(tb);
                    return true;
                default:
                    return false;
            }
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            var dto = data.entity?.dto as GlobalToolDto;
            if (dto == null)
                return false;
            if (await base.SetUMI3DProperty(data))
                return true;

            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.ToolboxTools:
                    var tb = Toolbox.GetToolbox(dto.id);
                    List<GlobalToolDto> list = tb.tools;
                    switch (data.operationId)
                    {
                        case UMI3DOperationKeys.SetEntityListAddProperty:
                            int ind = UMI3DSerializer.Read<int>(data.container);
                            GlobalToolDto value = UMI3DSerializer.Read<GlobalToolDto>(data.container);
                            if (ind == list.Count)
                                list.Add(value);
                            else if (ind < list.Count && ind >= 0)
                                list.Insert(ind, value);
                            else
                            {
                                UMI3DLogger.LogWarning($"Add value ignore for {ind} in collection of size {list.Count}", scope);
                                return false;
                            }
                            await ReadUMI3DExtension(value, null);
                            break;
                        case UMI3DOperationKeys.SetEntityListRemoveProperty:
                            int i = UMI3DSerializer.Read<int>(data.container);
                            RemoveTool(tb.tools[i]);
                            list.RemoveAt(i);
                            break;
                        case UMI3DOperationKeys.SetEntityListProperty:
                            int index = UMI3DSerializer.Read<int>(data.container);
                            GlobalToolDto v = UMI3DSerializer.Read<GlobalToolDto>(data.container);
                            list[index] = v;
                            break;
                        default:
                            foreach (GlobalToolDto t in list)
                                RemoveTool(t);
                            list.Clear();
                            list.AddRange(UMI3DSerializer.ReadList<GlobalToolDto>(data.container));
                            foreach (GlobalToolDto t in list)
                                await ReadUMI3DExtension(t, null);
                            break;
                    }
                    onGlobalToolUpdate?.Invoke(tb);
                    return true;
                //todo
                default:
                    return false;
            }
        }



        /// <summary>
        /// Remove a <see cref="GlobalTool"/>.
        /// </summary>
        /// <param name="tool">Tool to remove dto</param>
        public static void RemoveTool(GlobalToolDto tool)
        {
            var t = GlobalTool.GetGlobalTool(tool.id);
            t.Delete();
            onGlobalToolDelete?.Invoke(t);
        }

        /// <summary>
        /// Reads the value of an unknown <see cref="object"/> based on a received <see cref="ByteContainer"/> and updates it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="value">Boxing object to retrieve read value.</param>
        /// <param name="propertyKey">Property to update key in <see cref="UMI3DPropertyKeys"/></param>
        /// <param name="container">Received byte container</param>
        /// <returns>True if property setting was successful</returns>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            if (await base.ReadUMI3DProperty(data)) return true;
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.ToolboxTools:
                    data.result = UMI3DSerializer.ReadList<GlobalToolDto>(data.container);
                    return true;
                default:
                    return false;
            }
        }
    }
}