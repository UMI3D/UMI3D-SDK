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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    public static class UMI3DGlobalToolLoader
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

        public static void ReadUMI3DExtension(GlobalToolDto dto, Action finished, Action<Umi3dException> failed, Toolbox parent = null)
        {
            if (GlobalTool.GetGlobalTools().Exists(t => t.id == dto.id))
                return;

            if (dto is ToolboxDto toolbox)
            {
                var tool = new Toolbox(dto, parent);
                var subTools = new Stack<GlobalToolDto>(toolbox.tools);

                Action recursiveSubToolsLoading = null;
                recursiveSubToolsLoading = () =>
                {
                    if (subTools.Count > 0)
                    {
                        ReadUMI3DExtension(subTools.Pop(), recursiveSubToolsLoading, failed, tool);
                    }
                    else
                    {
                        finished?.Invoke();
                        onGlobalToolCreation.Invoke(tool);
                    }
                };
                recursiveSubToolsLoading();
            }
            else
            {
                finished?.Invoke();
                onGlobalToolCreation.Invoke(new GlobalTool(dto, parent));
            }
        }

        public static void RemoveTool(GlobalToolDto tool)
        {
            var t = GlobalTool.GetGlobalTool(tool.id);
            t.Delete();
            onGlobalToolDelete?.Invoke(t);
        }

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = (entity?.dto as GlobalToolDto);

            if (dto == null)
                return false;

            if (UMI3DAbstractToolLoader.SetUMI3DProperty(entity, property))
            {
                onGlobalToolUpdate.Invoke(GlobalTool.GetGlobalTool(dto.id));
                return true;
            }

            switch (property.property)
            {
                case UMI3DPropertyKeys.ToolboxTools:
                    var tb = Toolbox.GetToolbox(dto.id);
                    List<GlobalToolDto> list = tb.tools;
                    switch (property)
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
                            ReadUMI3DExtension(value, null, null);
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
                            list.AddRange(property.value as List<GlobalToolDto>);
                            foreach (GlobalToolDto t in list)
                                ReadUMI3DExtension(t, null, null);
                            break;
                    }
                    onGlobalToolUpdate?.Invoke(tb);
                    return true;
                default:
                    return false;
            }
        }

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var dto = (entity?.dto as GlobalToolDto);
            if (dto == null)
                return false;
            if (UMI3DAbstractToolLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;

            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ToolboxTools:
                    var tb = Toolbox.GetToolbox(dto.id);
                    List<GlobalToolDto> list = tb.tools;
                    switch (operationId)
                    {
                        case UMI3DOperationKeys.SetEntityListAddProperty:
                            int ind = UMI3DNetworkingHelper.Read<int>(container);
                            GlobalToolDto value = UMI3DNetworkingHelper.Read<GlobalToolDto>(container);
                            if (ind == list.Count)
                                list.Add(value);
                            else if (ind < list.Count && ind >= 0)
                                list.Insert(ind, value);
                            else
                            {
                                UMI3DLogger.LogWarning($"Add value ignore for {ind} in collection of size {list.Count}", scope);
                                return false;
                            }
                            ReadUMI3DExtension(value, null, null);
                            break;
                        case UMI3DOperationKeys.SetEntityListRemoveProperty:
                            int i = UMI3DNetworkingHelper.Read<int>(container);
                            RemoveTool(tb.tools[i]);
                            list.RemoveAt(i);
                            break;
                        case UMI3DOperationKeys.SetEntityListProperty:
                            int index = UMI3DNetworkingHelper.Read<int>(container);
                            GlobalToolDto v = UMI3DNetworkingHelper.Read<GlobalToolDto>(container);
                            list[index] = v;
                            break;
                        default:
                            foreach (GlobalToolDto t in list)
                                RemoveTool(t);
                            list.Clear();
                            list.AddRange(UMI3DNetworkingHelper.ReadList<GlobalToolDto>(container));
                            foreach (GlobalToolDto t in list)
                                ReadUMI3DExtension(t, null, null);
                            break;
                    }
                    onGlobalToolUpdate?.Invoke(tb);
                    return true;
                //todo
                default:
                    return false;
            }
        }

        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (UMI3DAbstractToolLoader.ReadUMI3DProperty(ref value, propertyKey, container)) return true;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ToolboxTools:
                    value = UMI3DNetworkingHelper.ReadList<GlobalToolDto>(container);
                    return true;
                default:
                    return false;
            }
        }
    }
}