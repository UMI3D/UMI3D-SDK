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
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Helper class that manages the loading of <see cref="Interactable"/> entities.
    /// </summary>
    public class UMI3DInteractableLoader : UMI3DAbstractToolLoader
    {

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is InteractableDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as InteractableDto;

            var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.nodeId);

            if (e is UMI3DNodeInstance nodeI)
            {
                value.node = nodeI.gameObject;
                Interactable interactable = value.node.GetOrAddComponent<InteractableContainer>().Interactable = new Interactable(dto);
                UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, interactable, interactable.Destroy).NotifyLoaded();
            }
            else
                throw (new Umi3dException($"Entity [{dto.nodeId}] is not a node"));
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var dto = value.entity?.dto as InteractableDto;
            if (dto == null) return false;
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.InteractableNotifyHoverPosition:
                    dto.notifyHoverPosition = (bool)value.property.value;
                    break;
                case UMI3DPropertyKeys.InteractableNotifySubObject:
                    dto.notifySubObject = (bool)value.property.value;
                    break;
                case UMI3DPropertyKeys.InteractableNodeId:
                    RemoveInteractableOnNode(dto);
                    dto.nodeId = (ulong)(long)value.property.value;
                    setInteractableOnNode(dto);
                    break;
                case UMI3DPropertyKeys.InteractableHasPriority:
                    dto.hasPriority = (bool)value.property.value;
                    break;
                case UMI3DPropertyKeys.InteractableInteractionDistance:
                    dto.interactionDistance = (float)(double)value.property.value;
                    break;
                case UMI3DPropertyKeys.InteractableHoverEnterAnimation:
                    dto.HoverEnterAnimationId = (ulong)value.property.value;
                    break;
                case UMI3DPropertyKeys.InteractableHoverExitAnimation:
                    dto.HoverExitAnimationId = (ulong)value.property.value;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            var dto = value.entity?.dto as InteractableDto;
            if (dto == null) return false;
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.InteractableNotifyHoverPosition:
                    dto.notifyHoverPosition = UMI3DSerializer.Read<bool>(value.container);
                    break;
                case UMI3DPropertyKeys.InteractableNotifySubObject:
                    dto.notifySubObject = UMI3DSerializer.Read<bool>(value.container);
                    break;
                case UMI3DPropertyKeys.InteractableNodeId:
                    RemoveInteractableOnNode(dto);
                    dto.nodeId = UMI3DSerializer.Read<ulong>(value.container);
                    setInteractableOnNode(dto);
                    break;
                case UMI3DPropertyKeys.InteractableHasPriority:
                    dto.hasPriority = UMI3DSerializer.Read<bool>(value.container);
                    break;
                case UMI3DPropertyKeys.InteractableInteractionDistance:
                    dto.interactionDistance = UMI3DSerializer.Read<float>(value.container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Reads the value of an unknown <see cref="object"/> based on a received <see cref="ByteContainer"/> and updates it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="value">Unknown object</param>
        /// <param name="propertyKey">Property to update key in <see cref="UMI3DPropertyKeys"/></param>
        /// <param name="container">Received byte container</param>
        /// <returns>True if property setting was successful</returns>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            if (await base.ReadUMI3DProperty(data)) return true;
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.InteractableNotifyHoverPosition:
                    data.result = UMI3DSerializer.Read<bool>(data.container);
                    break;
                case UMI3DPropertyKeys.InteractableNotifySubObject:
                    data.result = UMI3DSerializer.Read<bool>(data.container);
                    break;
                case UMI3DPropertyKeys.InteractableNodeId:
                    data.result = UMI3DSerializer.Read<ulong>(data.container);
                    break;
                case UMI3DPropertyKeys.InteractableHasPriority:
                    data.result = UMI3DSerializer.Read<bool>(data.container);
                    break;
                case UMI3DPropertyKeys.InteractableInteractionDistance:
                    data.result = UMI3DSerializer.Read<float>(data.container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Remove the interactable on the scene graph.
        /// </summary>
        /// <param name="dto">Interactable to remove dto</param>
        private static void RemoveInteractableOnNode(InteractableDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.nodeId);
            InteractableContainer interactable = node.gameObject.GetComponent<InteractableContainer>();
            if (interactable != null)
                GameObject.Destroy(interactable);
        }

        /// <summary>
        /// Set the interactable on the scene graph.
        /// </summary>
        /// <param name="dto">Interactable to add dto</param>
        private static void setInteractableOnNode(InteractableDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.nodeId);
            var interactable = UMI3DEnvironmentLoader.GetEntity(dto.id)?.Object as Interactable;
            if (interactable == null)
                interactable = new Interactable(dto);
            node.gameObject.GetOrAddComponent<InteractableContainer>().Interactable = interactable;
        }
    }
}