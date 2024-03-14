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

using System.Threading.Tasks;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public sealed class UMI3DToolManager 
    {
        debug.UMI3DLogger logger = new();

        public Tool_SO tool_SO;

        [HideInInspector] public MonoBehaviour context;
        [HideInInspector] public UMI3DController controller;
        [HideInInspector] public UMI3DControlManager controlManager;
        [HideInInspector] public UMI3DProjectionManager projectionManager;

        public void Init(
            MonoBehaviour context,
            UMI3DController controller
        )
        {
            logger.MainContext = context;
            logger.MainTag = nameof(UMI3DToolManager);
            this.context = context;
            this.controller = controller;
            this.controlManager = controller.controlManager;
            this.projectionManager = controller.projectionManager;
        }

        public AbstractTool CurrentProjectedTool
        {
            get
            {
                if (tool_SO.projectedTools.Count <= tool_SO.currentSelectedToolIndex)
                {
                    logger.Exception(
                        nameof(CurrentProjectedTool),
                        new IndexOutOfRangeException($"Selected tool index.")
                    );
                    return null;
                }

                return tool_SO.projectedTools[tool_SO.currentSelectedToolIndex];
            }
        }

        /// <summary>
        /// Check if a tool with the given id exists.
        /// </summary>
        public static bool Exists<Tool>(
            ulong environmentId,
            ulong id
        ) where Tool: AbstractTool
        {
            return UMI3DEnvironmentLoader.Instance.TryGetEntity(environmentId, id, out Tool tool);
        }

        /// <summary>
        /// Get the tool with the given id (if any).
        /// </summary>
        public static Tool GetTool<Tool>(
            ulong environmentId,
            ulong id
        ) where Tool: AbstractTool
        {
            UMI3DEnvironmentLoader.Instance.TryGetEntity(environmentId, id, out Tool tool);
            return tool;
        }

        /// <summary>
        /// Return the tools matching a given condition.
        /// </summary>
        public static IEnumerable<AbstractTool> GetTools(Predicate<AbstractTool> condition)
        {
            return UMI3DEnvironmentLoader
                .AllEntities()
                .Where(e => e?.Object is AbstractTool)
                .Select(e => e?.Object as AbstractTool)
                .ToList()
                .FindAll(condition);
        }

        /// <summary>
        /// Return all known tools.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AbstractTool> GetTools()
        {
            return GetTools(t => true);
        }

        /// <summary>
        /// Return true if the tool is currently projected on a controller.<br/>
        /// <br/>
        /// Set <paramref name="controller"/> with the controller associated with the tool.
        /// </summary>
        /// <param name="id">Id of the tool.</param>
        /// <returns></returns>
        public static bool IsProjected(
            ulong environmentId,
            ulong id,
            out AbstractTool tool,
            out UMI3DController controller
        )
        {
            try
            {
                var kv  = Tool_SO.controllerByTool.First(keyValue =>
                {
                    return keyValue.Key.data.environmentId == environmentId
                    && keyValue.Key.data.dto.id == id;
                });

                tool = kv.Key;
                controller = kv.Value;
                return true;
            }
            catch (Exception)
            {
                tool = null;
                controller = null;
                return false;
            }
        }

        /// <summary>
        /// Whether this tool is projected.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool IsProjected(AbstractTool tool)
        {
            return tool_SO.projectedTools.Contains(tool);
        }

        /// <summary>
        /// Project <paramref name="tool"/> on this controller.
        /// </summary>
        /// <param name="tool"></param>
        public void ProjectTool(AbstractTool tool)
        {
            if (IsProjected(tool))
            {
                return;
            }

            tool_SO.projectedTools.Add(tool);
            Tool_SO.controllerByTool.Add(tool, controller);
            tool.onProjected(controller.BoneType);
        }

        /// <summary>
        /// Release tool from this controller.
        /// </summary>
        /// <param name="tool"></param>
        public void ReleaseTool(AbstractTool tool)
        {
            tool_SO.projectedTools.Remove(tool);
            Tool_SO.controllerByTool.Remove(tool);
            tool.onReleased(controller.BoneType);
        }

        /// <summary>
        /// Whether this <paramref name="input"/> is associated to <paramref name="tool"/>.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsAssociated(AbstractTool tool, AbstractControlEntity control)
        {
            if (!tool_SO.controlsByTool.ContainsKey(tool.data.dto.id))
            {
                return false;
            }

            return tool_SO.controlsByTool[tool.data.dto.id].Contains(control);
        }

        /// <summary>
        /// Associates <paramref name="controls"/> to this <paramref name="tool"/>.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="controls"></param>
        public void AssociateControls(AbstractTool tool, params AbstractControlEntity[] controls)
        {
            if (!tool_SO.controlsByTool.ContainsKey(tool.data.dto.id))
            {
                tool_SO.controlsByTool.Add(tool.data.dto.id, controls);
            }
            else
            {
                tool_SO.controlsByTool[tool.data.dto.id] 
                    = tool_SO.controlsByTool[tool.data.dto.id]
                    .Concat(controls)
                    .ToArray();
            }
        }

        /// <summary>
        /// Dissociates <paramref name="control"/> from <paramref name="tool"/>.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="control"></param>
        public void DissociateControl(AbstractTool tool, AbstractControlEntity control)
        {
            if (!tool_SO.controlsByTool.ContainsKey(tool.data.dto.id))
            {
                logger.Exception(
                    nameof(DissociateControl),
                    new NoToolFoundException()
                );
                return;
            }

            var tmp = new List<AbstractControlEntity>(tool_SO.controlsByTool[tool.data.dto.id]);
            tmp.Remove(control);
            tool_SO.controlsByTool[tool.data.dto.id] = tmp.ToArray();
        }

        /// <summary>
        /// Dissociates all inputs from <paramref name="tool"/>.
        /// </summary>
        /// <param name="tool"></param>
        public void DissociateAllControls(AbstractTool tool)
        {
            if (!tool_SO.controlsByTool.ContainsKey(tool.data.dto.id))
            {
                return;
            }

            tool_SO.controlsByTool.Remove(tool.data.dto.id);
        }

        /// <summary>
        /// Request the selection of a Tool.<br/>
        /// <br/>
        /// Be careful, this method could be called before the tool is added for async loading reasons.
        /// </summary>
        /// <param name="toolId">Id of the tool to release.</param>
        /// <param name="releasable">The selected tool releasable.</param>
        /// <param name="hoveredObjectId">The id of the hovered object.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public static void SelectTool(
            ulong environmentId,
            ulong toolId,
            bool releasable,
            ulong hoveredObjectId,
            InteractionMappingReason reason = null,
            UMI3DController controller = null
        )
        {
            if (!Exists<AbstractTool>(environmentId, toolId))
            {
                throw new NoToolFoundException($"For toolId: {toolId}, environmentId: {environmentId}");
            }
            var tool = GetTool<AbstractTool>(environmentId, toolId);

            if (controller == null)
            {
                switch (reason)
                {
                    case RequestedFromMenu:

                        controller = UMI3DController.lastControllerUsed;
                        break;

                    case RequestedUsingSelector:

                        controller = UMI3DController.lastControllerHovering;
                        break;

                    default:

                        UMI3DController firstCompatibleController = null;
                        foreach (var ctrlr in UMI3DController.activeControllers)
                        {
                            if (!ctrlr.controlManager.IsCompatibleWith(tool))
                            {
                                continue;
                            }
                            else if (firstCompatibleController == null)
                            {
                                firstCompatibleController = ctrlr;
                            }

                            if (ctrlr.controlManager.IsAvailableFor(tool))
                            {
                                firstCompatibleController = ctrlr;
                                break;
                            }
                        }
                        controller = firstCompatibleController;
                        break;
                }
            }

            UMI3DToolManager toolManager = controller.toolManager;

            if (!toolManager.controlManager.IsCompatibleWith(tool))
            {
                toolManager.logger.Exception(
                    nameof(SelectTool),
                    new IncompatibleToolException($"For toolId: {toolId}, environmentId: {environmentId}, controller: {controller.context.name}")
                );
                return;
            }

            if (toolManager.IsProjected(tool))
            {
                toolManager.logger.Exception(
                    nameof(SelectTool),
                    new ToolAlreadyProjectedException($"For toolId: {toolId}, environmentId: {environmentId}, controller: {controller.context.name}")
                );
                return;
            }

            if (!controller.controlManager.IsAvailableFor(tool))
            {
                if (reason is not AutoProjectOnHover 
                    || (toolManager.CurrentProjectedTool?.data.selectionReason is AutoProjectOnHover 
                    && reason is AutoProjectOnHover))
                {
                    toolManager.projectionManager.Release(toolManager.CurrentProjectedTool);
                }
                else
                {
                    return;
                }
            }

            tool.data.releasable = releasable;
            tool.data.selectionReason = reason;

            toolManager.projectionManager.Project(tool, hoveredObjectId);
        }

        /// <summary>
        /// Request a Tool to be unselected.
        /// </summary>
        /// <param name="toolId">Id of the tool to release.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        public static void UnselectTool(
            ulong environmentId,
            ulong toolId,
            InteractionMappingReason reason = null
        )
        {
            if (!IsProjected(environmentId, toolId, out AbstractTool tool, out UMI3DController controller))
            {
                controller.toolManager.logger.Exception(
                    nameof(UnselectTool),
                    new ToolNotProjectedException($"For toolId: {toolId}, environmentId: {environmentId}, controller: {controller.context.name}")
                );
                return;
            }

            controller.toolManager.projectionManager.Release(tool);
        }

        /// <summary>
        /// Request a Tool to be replaced by another one.
        /// </summary>
        /// <param name="selected">Id of the tool to select</param>
        /// <param name="released">Id of the tool to release</param>
        /// <param name="releasable">The selected tool releasable.</param>
        /// <param name="hoveredObjectId">The id of the hovered object.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public static void SwitchTools(
            ulong environmentId,
            ulong selected,
            ulong released,
            bool releasable,
            ulong hoveredObjectId,
            InteractionMappingReason reason = null
        )
        {
            if (IsProjected(environmentId, released, out AbstractTool tool, out UMI3DController controller))
            {
                UnselectTool(
                    environmentId,
                    released,
                    reason
                );
            }

            SelectTool(
                environmentId,
                selected,
                releasable,
                hoveredObjectId,
                reason,
                controller
            );
        }

        public static Task UnknownOperationHandler(DtoContainer operation)
        {
            switch (operation.operation)
            {
                case SwitchToolDto switchTool:
                    SwitchTools(
                        operation.environmentId, 
                        switchTool.toolId, 
                        switchTool.replacedToolId, 
                        switchTool.releasable, 
                        0, 
                        new RequestedByEnvironment()
                    );
                    break;
                case ProjectToolDto projection:
                    SelectTool(
                        operation.environmentId, 
                        projection.toolId, 
                        projection.releasable, 
                        0, 
                        new RequestedByEnvironment()
                    );
                    break;
                case ReleaseToolDto release:
                    UnselectTool(
                        operation.environmentId, 
                        release.toolId, 
                        new RequestedByEnvironment()
                    );
                    break;
            }

            return Task.CompletedTask;
        }

        public static Task UnknownOperationHandler(uint operationId, ByteContainer container)
        {
            ulong id;
            bool releasable;

            switch (operationId)
            {
                case UMI3DOperationKeys.SwitchTool:
                    id = UMI3DSerializer.Read<ulong>(container);
                    ulong oldId = UMI3DSerializer.Read<ulong>(container);
                    releasable = UMI3DSerializer.Read<bool>(container);
                    SwitchTools(
                        container.environmentId,
                        id,
                        oldId,
                        releasable,
                        0,
                        new RequestedByEnvironment()
                    );
                    break;
                case UMI3DOperationKeys.ProjectTool:
                    id = UMI3DSerializer.Read<ulong>(container);
                    releasable = UMI3DSerializer.Read<bool>(container);
                    SelectTool(
                        container.environmentId, 
                        id, 
                        releasable, 
                        0, 
                        new RequestedByEnvironment()
                    );
                    break;
                case UMI3DOperationKeys.ReleaseTool:
                    id = UMI3DSerializer.Read<ulong>(container);
                    UnselectTool(
                        container.environmentId, 
                        id, 
                        new RequestedByEnvironment()
                    );
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Request a Tool to be updated.
        /// </summary>
        /// <param name="toolId">Id of the Tool.</param>
        /// <param name="releasable">Is the tool releasable.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public static void UpdateTools(
            ulong environmentId,
            ulong toolId,
            bool releasable,
            InteractionMappingReason reason = null
        )
        {
            if (!IsProjected(environmentId, toolId, out AbstractTool tool, out UMI3DController controller))
            {
                return;
            }

            UnselectTool(
                environmentId,
                toolId,
                new ToolNeedToBeUpdated()
            );
            if (tool.data.dto.interactions.Count == 0)
            {
                SelectTool(
                    environmentId,
                    toolId,
                    releasable,
                    controller.toolManager.tool_SO.currentHoverId, 
                    reason,
                    controller
                );
            }

            tool.ToolUpdated();
        }

        /// <summary>
        /// Request a Tool to be updated when one element was added on the tool.
        /// </summary>
        /// <param name="toolId">Id of the Tool.</param>
        /// <param name="releasable">Is the tool releasable.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public static void UpdateAddOnTools(
            ulong environmentId,
            ulong toolId,
            bool releasable,
            AbstractInteractionDto newInteraction,
            InteractionMappingReason reason = null
        )
        {
            if (!IsProjected(environmentId, toolId, out AbstractTool tool, out UMI3DController controller))
            {
                return;
            }

            controller.toolManager.projectionManager.Project(tool, newInteraction, releasable, reason);

            tool.InteractionAdded(newInteraction);
        }

        /// <summary>
        /// Request a Tool to be updated when one element was removed on the tool.
        /// </summary>
        /// <param name="toolId">Id of the Tool.</param>
        /// <param name="releasable">Is the tool releasable.</param>
        /// <param name="reason">Interaction mapping reason.</param>
        /// <returns></returns>
        public static void UpdateRemoveOnTools(
            ulong environmentId,
            ulong toolId,
            bool releasable,
            AbstractInteractionDto oldInteraction,
            InteractionMappingReason reason = null
        )
        {
            if (!IsProjected(environmentId, toolId, out AbstractTool tool, out UMI3DController controller))
            {
                return;
            }

            controller.toolManager.projectionManager.Release(tool, oldInteraction);

            tool.InteractionRemoved(oldInteraction);
        }
    }
}