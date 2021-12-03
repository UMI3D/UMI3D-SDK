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
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    public static class UMI3DGlobalToolLoader
    {
        #region CRUD events
        private static GlobalToolEvent onGlobalToolCreation = new GlobalToolEvent();
        private static GlobalToolEvent onGlobalToolUpdate = new GlobalToolEvent();
        private static GlobalToolEvent onGlobalToolDelete = new GlobalToolEvent();

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

        public static void ReadUMI3DExtension(GlobalToolDto dto, Action finished, Action<Umi3dException> failed)
        {
            GlobalTool tool = (dto is ToolboxDto) ? new Toolbox(dto): new GlobalTool(dto);

            if (dto is ToolboxDto)
            {
                Stack<GlobalToolDto> subTools = new Stack<GlobalToolDto>((dto as ToolboxDto).tools);
                Action recursiveSubToolsLoading = null;
                recursiveSubToolsLoading = () =>
                {
                    if (subTools.Count > 0)
                    {
                        ReadUMI3DExtension(subTools.Pop(), recursiveSubToolsLoading, failed);
                    }
                    else
                    {
                        finished();
                        onGlobalToolCreation.Invoke(tool);
                    }
                };
                recursiveSubToolsLoading();
            }
            else
            {
                finished();
                onGlobalToolCreation.Invoke(tool);
            }            
        }

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = (entity?.dto as GlobalToolDto);

            if (dto == null)
                return false;

            if (UMI3DAbstractToolLoader.SetUMI3DProperty(entity, property)) 
                return true;

            switch (property.property)
            {
                case UMI3DPropertyKeys.ToolboxTools:
                    Toolbox.GetToolbox(dto.id).SetTools(property.value as List<GlobalToolDto>);
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
                    List<GlobalToolDto> tools = UMI3DNetworkingHelper.ReadList<GlobalToolDto>(container);
                    Toolbox.GetToolbox(dto.id).SetTools(tools);
                    return true;
                    
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