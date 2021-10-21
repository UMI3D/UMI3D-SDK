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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    static public class UMI3DInteractableLoader
    {

        public static void ReadUMI3DExtension(InteractableDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.nodeId, (e) => {
                if (e is UMI3DNodeInstance nodeI)
                {
                    node = nodeI.gameObject;
                    var interactable = node.GetOrAddComponent<InteractableContainer>().Interactable = new Interactable(dto);
                    UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, interactable, interactable.Destroy);
                    finished?.Invoke();
                }
                else
                    failed.Invoke(new Umi3dException($"Entity [{dto.nodeId}] is not a node"));
            });
        }

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = (entity?.dto as InteractableDto);
            if (dto == null) return false;
            if (UMI3DAbstractToolLoader.SetUMI3DProperty(entity, property)) return true;
            switch (property.property)
            {
                case UMI3DPropertyKeys.InteractableNotifyHoverPosition:
                    dto.notifyHoverPosition = (bool)property.value;
                    break;
                case UMI3DPropertyKeys.InteractableNotifySubObject:
                    dto.notifySubObject = (bool)property.value;
                    break;
                case UMI3DPropertyKeys.InteractableNodeId:
                    RemoveInteractableOnNode(dto);
                    dto.nodeId = (ulong)(long)property.value;
                    setInteractableOnNode(dto);
                    break;
                case UMI3DPropertyKeys.InteractableHasPriority:
                    dto.hasPriority = (bool)property.value;
                    break;
                default:
                    return false;
            }
            return true;
        }


        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var dto = (entity?.dto as InteractableDto);
            if (dto == null) return false;
            if (UMI3DAbstractToolLoader.SetUMI3DProperty(entity, operationId, propertyKey, container)) return true;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.InteractableNotifyHoverPosition:
                    dto.notifyHoverPosition = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.InteractableNotifySubObject:
                    dto.notifySubObject = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.InteractableNodeId:
                    RemoveInteractableOnNode(dto);
                    dto.nodeId = UMI3DNetworkingHelper.Read<ulong>(container);
                    setInteractableOnNode(dto);
                    break;
                case UMI3DPropertyKeys.InteractableHasPriority:
                    dto.hasPriority = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        static public bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (UMI3DAbstractToolLoader.ReadUMI3DProperty(ref value, propertyKey, container)) return true;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.InteractableNotifyHoverPosition:
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.InteractableNotifySubObject:
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.InteractableNodeId:
                    value = UMI3DNetworkingHelper.Read<ulong>(container);
                    break;
                case UMI3DPropertyKeys.InteractableHasPriority:
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                default:
                    return false;
            }
            return true;
        }


        static void RemoveInteractableOnNode(InteractableDto dto)
        {
            var node = UMI3DEnvironmentLoader.GetNode(dto.nodeId);
            var interactable = node.gameObject.GetComponent<InteractableContainer>();
            if (interactable != null)
                GameObject.Destroy(interactable);
        }

        static void setInteractableOnNode(InteractableDto dto)
        {
            var node = UMI3DEnvironmentLoader.GetNode(dto.nodeId);
            var interactable = UMI3DEnvironmentLoader.GetEntity(dto.id)?.Object as Interactable;
            if (interactable == null)
                interactable = new Interactable(dto);
            node.gameObject.GetOrAddComponent<InteractableContainer>().Interactable = interactable;
        }

    }
}