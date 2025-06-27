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
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Helper class that manages the loading of <see cref="InteractableDto"/> entities.
    /// </summary>
    public class UMI3DInteractableLoader : UMI3DAbstractToolLoader
    {

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is InteractableDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            InteractableDto dto = value.dto as InteractableDto;

            UMI3DEntityInstance e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(value.environmentId, dto.nodeId,value.tokens);

            if (e is UMI3DNodeInstance nodeI)
            {
                value.node = nodeI.GameObject;
                InteractableContainer container = value.node.GetOrAddComponent<InteractableContainer>();
#if !UMI3D_NEW_LABEL
                Interactable interactable = container.Interactable = new Interactable(value.environmentId, dto);
                UMI3DEnvironmentLoader.RegisterEntityInstance(value.environmentId,dto.id, dto, interactable, interactable.Destroy).NotifyLoaded();


#else
                ToolManager.@default.InstantiateOrGet(out Tool tool, value.environmentId, dto);
                container.tool = tool;
#endif
                //Check if his root is a ScreeSpace Canvas then start process the interaction's binding
                if (nodeI.transform.root.gameObject.TryGetComponent<Canvas>(out Canvas _canvas))
                {
                    if (_canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay)
                    {
                        //Create, add Save Values in InteractionScreenSpace for sending Event
                        InteractionScreenSpace _IntScreenSpace = nodeI.transform.gameObject.AddComponent<InteractionScreenSpace>();
                        _IntScreenSpace._go = nodeI.GameObject;
                        _IntScreenSpace._interactable = interactable;
                        _IntScreenSpace._value = value;

                        //Create and add Unity Button 
                        _IntScreenSpace._button = nodeI.transform.gameObject.AddComponent<Button>();

                        // Loop to search for all parents containing a Canvas in order to add a GraphicRaycaster 
                        // to allow interaction with the previously created button 
                        // stops when the parent IngameUIManager is found indicating that we are no longer on the node's parent but on the global ScreenSpace UI Canvas
                        GameObject go = nodeI.transform.parent.gameObject;
                        for (int i = 0; i < 20; i++)
                        {
                            if (go.name == nodeI.transform.root.name) { break; }
                            if (go.TryGetComponent<Canvas>(out Canvas canvas))
                            {
                                go.GetOrAddComponent<GraphicRaycaster>();
                            }
                            go = go.transform.parent.gameObject;
                        }
                        _IntScreenSpace.AssignListenner();
                    }

                }
                else if (nodeI.transform.name == "PinImage")
                {
                    InteractionScreenSpace _intScreenSpace = nodeI.transform.gameObject.AddComponent<InteractionScreenSpace>();
                    nodeI.transform.parent.gameObject.AddComponent<GraphicRaycaster>();

                    //ajoute un bouton
                    _intScreenSpace._button = nodeI.transform.gameObject.AddComponent<Button>();
                    _intScreenSpace._go = nodeI.GameObject;
                    _intScreenSpace._interactable = interactable;
                    _intScreenSpace._value = value;
                    _intScreenSpace.AssignListenner();
                }
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
                    RemoveInteractableOnNode(value.environmentId, dto);
                    dto.nodeId = (ulong)(long)value.property.value;
                    setInteractableOnNode(value.environmentId, dto);
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
                    RemoveInteractableOnNode(value.environmentId, dto);
                    dto.nodeId = UMI3DSerializer.Read<ulong>(value.container);
                    setInteractableOnNode(value.environmentId, dto);
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
        private static void RemoveInteractableOnNode(ulong environmentId, InteractableDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(environmentId, dto.nodeId);
            InteractableContainer interactable = node.GameObject.GetComponent<InteractableContainer>();
            if (interactable != null)
                GameObject.Destroy(interactable);
        }

        /// <summary>
        /// Set the interactable on the scene graph.
        /// </summary>
        /// <param name="dto">Interactable to add dto</param>
        private static void setInteractableOnNode(ulong environmentId, InteractableDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(environmentId, dto.nodeId);

            InteractableContainer container = node.GameObject.GetOrAddComponent<InteractableContainer>();
#if !UMI3D_NEW_LABEL
            var interactable = UMI3DEnvironmentLoader.GetEntity(environmentId, dto.id)?.Object as Interactable;
            if (interactable == null) interactable = new Interactable(environmentId, dto);
            container.Interactable = interactable;
#else
            bool isSuccess = ToolManager.@default.TryToFetchTool(out Tool tool, environmentId, dto.id);
            if (!isSuccess)
            {
                ToolManager.@default.InstantiateOrGet(out tool, environmentId, dto);
            }
            container.tool = tool;
#endif
        }
    }
}